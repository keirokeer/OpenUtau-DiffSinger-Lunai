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
        int sectionBrushApplyGeneration;

        public AppearancePreferencesPane() {
            InitializeComponent();
            AttachedToVisualTree += (_, _) => {
                ClosePanelButton.IsVisible = IsHostedInPianoRollDock();
                ScheduleApplyScrollStyle();
                ScheduleUpdateSectionBrushes(retryIfNeeded: true);
            };
            Loaded += (_, _) => ScheduleUpdateSectionBrushes(retryIfNeeded: true);
            DetachedFromVisualTree += (_, _) => {
                scrollStyleApplyGeneration++;
                sectionBrushApplyGeneration++;
            };
            this.GetObservable(IsVisibleProperty).Subscribe(visible => {
                if (visible) {
                    ScheduleUpdateSectionBrushes(retryIfNeeded: true);
                }
            });
            MessageBus.Current.Listen<ScrollbarsStyleChangedEvent>()
                .Subscribe(_ => ScheduleApplyScrollStyle());
            MessageBus.Current.Listen<PianorollRefreshEvent>()
                .Subscribe(e => {
                    if (e.refreshItem is "TrackColor" or "Part") {
                        ScheduleUpdateSectionBrushes(retryIfNeeded: true);
                    }
                });
            MessageBus.Current.Listen<ThemeChangedEvent>()
                .Subscribe(_ => ScheduleUpdateSectionBrushes(retryIfNeeded: true));
        }

        public void ScheduleUpdateSectionBrushes(bool retryIfNeeded = false) {
            if (!WorkspaceScrollbarHelper.IsInVisualTree(this)) {
                return;
            }
            int generation = ++sectionBrushApplyGeneration;
            Dispatcher.UIThread.Post(() => {
                if (generation != sectionBrushApplyGeneration
                    || !WorkspaceScrollbarHelper.IsInVisualTree(this)) {
                    return;
                }
                ApplySectionBrushes();
                if (retryIfNeeded
                    && generation == sectionBrushApplyGeneration
                    && !sectionChrome.HasAppliedNotePropsExpanders(this)) {
                    Dispatcher.UIThread.Post(() => {
                        if (generation == sectionBrushApplyGeneration) {
                            ApplySectionBrushes();
                        }
                    }, DispatcherPriority.Render);
                }
            }, DispatcherPriority.Loaded);
        }

        void ApplySectionBrushes() {
            if (!IsLoaded) {
                return;
            }
            sectionChrome.UpdateFromWorkspace(GetTrackColorName());
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
            WorkspaceScrollbarHelper.ApplyScrollViewer(ContentScroll, !Preferences.Default.UseOverlayScrollbars);
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

        bool IsHostedInPianoRollDock() {
            return this.GetVisualAncestors().OfType<PianoRoll>().Any();
        }

        void OnCloseDockedPanel(object? sender, RoutedEventArgs e) {
            var pianoRoll = this.GetVisualAncestors().OfType<PianoRoll>().FirstOrDefault();
            if (pianoRoll?.ViewModel != null) {
                pianoRoll.ViewModel.ShowAppearancePanel = false;
            }
        }

        void OpenCustomThemeEditor(object? sender, RoutedEventArgs e) {
            var vm = ViewModel;
            if (string.IsNullOrEmpty(vm.ThemeName) || !CustomTheme.Themes.TryGetValue(vm.ThemeName, out var path)) {
                return;
            }
            if (IsHostedInPianoRollDock()) {
                ThemeEditorWindow.CloseIfOpen();
                MessageBus.Current.SendMessage(new OpenDockedThemeEditorEvent { Path = path });
            } else {
                MessageBus.Current.SendMessage(new CloseDockedThemeEditorEvent());
                ThemeEditorWindow.Show(path);
            }
        }

        void OnCustomThemeCreate(object? sender, RoutedEventArgs e) {
            var vm = ViewModel;
            var owner = GetOwnerWindow();
            var dialog = new CreateCustomThemeDialog();
            dialog.onFinish = (name, baseTheme) => {
                if (string.IsNullOrEmpty(name)) {
                    if (owner != null) {
                        MessageBox.ShowModal(owner,
                            ThemeManager.GetString("prefs.appearance.customtheme.create.empty"),
                            ThemeManager.GetString("prefs.appearance.customtheme.create.title"));
                    }
                    return;
                }

                string filename = string.Join("", name.Where(c => char.IsLetterOrDigit(c) || c == ' '))
                    .Replace(" ", "-").ToLower() + ".yaml";

                string themePath = Path.Join(PathManager.Inst.ThemesPath, filename);
                var themeYaml = BuiltInThemeLoader.CreateFromBuiltIn(baseTheme, name);
                themeYaml.SaveToFile(themePath);
                CustomTheme.ListThemes();
                vm.RefreshThemes();
                var themeKey = CustomTheme.Themes.FirstOrDefault(pair => pair.Value == themePath).Key;
                if (!string.IsNullOrEmpty(themeKey)) {
                    vm.ThemeName = themeKey;
                }
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

