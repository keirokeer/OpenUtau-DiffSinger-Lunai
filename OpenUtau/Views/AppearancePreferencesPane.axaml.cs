using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenUtau.App;
using OpenUtau.App.Controls;
using OpenUtau.App.ViewModels;
using OpenUtau.Colors;
using OpenUtau.Core;
using OpenUtau.Core.Util;
using ReactiveUI;

namespace OpenUtau.App.Views {
    public partial class AppearancePreferencesPane : UserControl {
        PreferencesViewModel ViewModel => EnsureViewModel();
        readonly WorkspaceSectionExpanderChrome sectionChrome = new();
        int scrollStyleApplyGeneration;

        public AppearancePreferencesPane() {
            InitializeComponent();
            AttachedToVisualTree += (_, _) => {
                ScheduleApplyScrollStyle();
                UpdateSectionBrushes();
            };
            DetachedFromVisualTree += (_, _) => scrollStyleApplyGeneration++;
            MessageBus.Current.Listen<ScrollbarsStyleChangedEvent>()
                .Subscribe(_ => ScheduleApplyScrollStyle());
            MessageBus.Current.Listen<PianorollRefreshEvent>()
                .Subscribe(_ => Dispatcher.UIThread.Post(UpdateSectionBrushes, DispatcherPriority.Background));
            MessageBus.Current.Listen<ThemeChangedEvent>()
                .Subscribe(_ => Dispatcher.UIThread.Post(UpdateSectionBrushes, DispatcherPriority.Background));
        }

        void UpdateSectionBrushes() {
            if (!IsLoaded) {
                return;
            }
            sectionChrome.UpdateFromTrackColor(GetTrackColorName());
            sectionChrome.Apply(
                this,
                sectionChrome.SectionHeaderBackground,
                sectionChrome.SectionHeaderBackgroundPointerOver,
                sectionChrome.SectionHeaderBackgroundPressed,
                sectionChrome.SectionContentBackground);
        }

        string GetTrackColorName() {
            var pianoRoll = this.GetVisualAncestors().OfType<PianoRoll>().FirstOrDefault();
            var part = pianoRoll?.ViewModel?.NotesViewModel?.Part;
            if (part != null && DocManager.Inst.Project != null) {
                return DocManager.Inst.Project.tracks[part.trackNo].TrackColor;
            }
            return "Blue";
        }

        void ScheduleApplyScrollStyle() {
            if (!WorkspaceScrollbarHelper.IsInVisualTree(this)) {
                return;
            }
            int generation = ++scrollStyleApplyGeneration;
            Dispatcher.UIThread.Post(() => {
                if (generation != scrollStyleApplyGeneration || !WorkspaceScrollbarHelper.IsInVisualTree(this)) {
                    return;
                }
                ApplyScrollStyle();
            }, DispatcherPriority.Loaded);
        }

        void ApplyScrollStyle() {
            if (!WorkspaceScrollbarHelper.IsInVisualTree(this)) {
                return;
            }
            WorkspaceScrollbarHelper.ApplyScrollViewer(ContentScroll, Preferences.Default.UseClassicScrollbars);
        }

        PreferencesViewModel EnsureViewModel() {
            if (DataContext is PreferencesViewModel vm) {
                return vm;
            }
            var prefs = new PreferencesViewModel();
            DataContext = prefs;
            return prefs;
        }

        Window? GetOwnerWindow() => TopLevel.GetTopLevel(this) as Window;

        void OpenCustomThemeEditor(object? sender, RoutedEventArgs e) {
            var vm = ViewModel;
            if (string.IsNullOrEmpty(vm.ThemeName) || !CustomTheme.Themes.TryGetValue(vm.ThemeName, out var path)) {
                return;
            }
            ThemeEditorWindow.Show(path);
        }

        void OnCustomThemeCreate(object? sender, RoutedEventArgs e) {
            var vm = ViewModel;
            var owner = GetOwnerWindow();
            var dialog = new TypeInDialog {
                Title = ThemeManager.GetString("prefs.appearance.customtheme.create.title")
            };
            dialog.SetPrompt(ThemeManager.GetString("prefs.appearance.customtheme.create.prompt"));
            dialog.onFinish = s => {
                if (string.IsNullOrEmpty(s)) {
                    if (owner != null) {
                        MessageBox.ShowModal(owner,
                            ThemeManager.GetString("prefs.appearance.customtheme.create.empty"),
                            ThemeManager.GetString("prefs.appearance.customtheme.create.title"));
                    }
                    return;
                }

                string filename = string.Join("", s.Where(c => char.IsLetterOrDigit(c) || c == ' '))
                    .Replace(" ", "-").ToLower() + ".yaml";

                var themeYaml = new CustomTheme.ThemeYaml { Name = s };

                File.WriteAllText(Path.Join(PathManager.Inst.ThemesPath, filename),
                    Yaml.DefaultSerializer.Serialize(themeYaml));
                vm.RefreshThemes();
            };
            if (owner != null) {
                dialog.ShowDialog(owner);
            } else {
                dialog.Show();
            }
        }

        async void OnCustomThemeDelete(object? sender, RoutedEventArgs e) {
            var vm = ViewModel;
            var owner = GetOwnerWindow();
            if (owner == null) {
                return;
            }
            var result = await MessageBox.Show(
                owner,
                ThemeManager.GetString("prefs.appearance.customtheme.delete.message"),
                ThemeManager.GetString("prefs.appearance.customtheme.delete.title"),
                MessageBox.MessageBoxButtons.YesNo);
            if (result == MessageBox.MessageBoxResult.Yes) {
                if (string.IsNullOrEmpty(vm.ThemeName) || !CustomTheme.Themes.TryGetValue(vm.ThemeName, out var path)) {
                    return;
                }
                string previousTheme = vm.ThemeItems.TakeWhile(x => x != vm.ThemeName).LastOrDefault()
                    ?? vm.ThemeItems.FirstOrDefault()
                    ?? "Dark";
                File.Delete(path);
                vm.RefreshThemes();
                vm.ThemeName = previousTheme;
            }
        }
    }
}
