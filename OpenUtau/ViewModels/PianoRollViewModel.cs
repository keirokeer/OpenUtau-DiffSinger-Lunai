using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using DynamicData.Binding;
using OpenUtau.App.Controls;
using OpenUtau.Classic;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using OpenUtau.ViewModels;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtau.App.ViewModels {
    public class PhonemeMouseoverEvent {
        public readonly UPhoneme? mouseoverPhoneme;
        public PhonemeMouseoverEvent(UPhoneme? mouseoverPhoneme) {
            this.mouseoverPhoneme = mouseoverPhoneme;
        }
    }

    public class NotesContextMenuArgs {
        public PianoRollViewModel? ViewModel { get; set; }

        public bool ForNote { get; set; }
        public NoteHitInfo NoteHitInfo { get; set; }

        public bool ForPitchPoint { get; set; }
        public bool PitchPointIsFirst { get; set; }
        public bool PitchPointCanDel { get; set; }
        public bool PitchPointCanAdd { get; set; }
        public PitchPointHitInfo PitchPointHitInfo { get; set; }
    }

    public class PianorollRefreshEvent {
        public readonly string refreshItem;
        public PianorollRefreshEvent(string refreshItem) {
            this.refreshItem = refreshItem;
        }
    }

    public class PianoRollViewModel : ViewModelBase, ICmdSubscriber {

        [Reactive] public NotesViewModel NotesViewModel { get; set; }
        [Reactive] public PlaybackViewModel? PlaybackViewModel { get; set; }
        [Reactive] public CurveViewModel CurveViewModel { get; set; }
        [Reactive] public Dictionary<string, string> Hotkeys { get; set; } = new Dictionary<string, string>();

        public double Width => Preferences.Default.PianorollWindowSize.Width;
        public double Height => Preferences.Default.PianorollWindowSize.Height;

        public bool LockPitchPoints { get => Preferences.Default.LockUnselectedNotesPitch; }
        public bool LockVibrato { get => Preferences.Default.LockUnselectedNotesVibrato; }
        public bool LockExpressions { get => Preferences.Default.LockUnselectedNotesExpressions; }
        public bool ShowPortrait { get => Preferences.Default.ShowPortrait; }
        public bool ShowIcon { get => Preferences.Default.ShowIcon; }
        public bool ShowGhostNotes { get => Preferences.Default.ShowGhostNotes; }
        public bool ShowNoteBorder { get => Preferences.Default.ShowNoteBorder; }
        public bool UseTrackColor { get => Preferences.Default.UseTrackColor; }
        public bool DegreeStyle0 { get => Preferences.Default.DegreeStyle == 0 ? true : false; }
        public bool DegreeStyle1 { get => Preferences.Default.DegreeStyle == 1 ? true : false; }
        public bool DegreeStyle2 { get => Preferences.Default.DegreeStyle == 2 ? true : false; }
        public bool LockStartTime0 { get => Preferences.Default.LockStartTime == 0 ? true : false; }
        public bool LockStartTime1 { get => Preferences.Default.LockStartTime == 1 ? true : false; }
        public bool LockStartTime2 { get => Preferences.Default.LockStartTime == 2 ? true : false; }
        public bool PlaybackAutoScroll0 { get => Preferences.Default.PlaybackAutoScroll == 0 ? true : false; }
        public bool PlaybackAutoScroll1 { get => Preferences.Default.PlaybackAutoScroll == 1 ? true : false; }
        public bool PlaybackAutoScroll2 { get => Preferences.Default.PlaybackAutoScroll == 2 ? true : false; }
        public bool PianoRollDetached { get => Preferences.Default.DetachPianoRoll; }
        [Reactive] public bool PianoRollFullscreen { get; set; }
        public bool UsesExpandedPianoRollLayout => PianoRollDetached || PianoRollFullscreen;
        public bool IsSidePanelVisible => !UsesExpandedPianoRollLayout;
        public bool IsAppearancePanelVisible => ShowAppearancePanel && !UsesExpandedPianoRollLayout;
        public GridLength PianoRollSideColumnWidth => UsesExpandedPianoRollLayout ? new GridLength(0) : new GridLength(48);
        public GridLength PianoRollSideGapWidth => UsesExpandedPianoRollLayout ? new GridLength(0) : new GridLength(8);
        [Reactive] public bool ShowAppearancePanel { get; set; }
        PreferencesViewModel? appearancePreferences;
        static PreferencesViewModel? sharedAppearancePreferences;

        public static void WarmUpAppearancePreferences() {
            if (sharedAppearancePreferences != null) {
                return;
            }
            sharedAppearancePreferences = new PreferencesViewModel();
        }

        public PreferencesViewModel AppearancePreferences =>
            appearancePreferences ??= sharedAppearancePreferences ??= new PreferencesViewModel();
        public GridLength AppearancePanelLeadingGapWidth =>
            UsesExpandedPianoRollLayout || !ShowAppearancePanel ? new GridLength(0) : new GridLength(8);
        public GridLength AppearancePanelColumnWidth =>
            UsesExpandedPianoRollLayout || !ShowAppearancePanel ? new GridLength(0) : new GridLength(300);
        public ReactiveCommand<Unit, Unit> ApplyDiffSingerQualityPresetCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyDiffSingerMediumPresetCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyDiffSingerLowPresetCommand { get; }
        public ReactiveCommand<Unit, Unit> ApplyDiffSingerExtraLowPresetCommand { get; }
        public bool ShowPhonemizerTags {
            get => Preferences.Default.ShowPhonemizerTags;
            set {
                Preferences.Default.ShowPhonemizerTags = value;
                Preferences.Save();
                this.RaisePropertyChanged(nameof(ShowPhonemizerTags));
            }
        }

        [Reactive] public bool IsTikTokMode { get; set; }

        public EditTool EditTool { get; set; } = Preferences.Default.EditTool;
        [Reactive] public int ToolIndex { get; set; } = Preferences.Default.EditTool.BaseTool;
        [Reactive] public int PenToolIndex { get; set; } = Preferences.Default.EditTool.PenToolVariation;
        [Reactive] public int DrawPitchToolIndex { get; set; } = Preferences.Default.EditTool.DrawPitchToolVariation;
        [Reactive] public int DrawLinePitchToolIndex { get; set; } = Preferences.Default.EditTool.DrawLinePitchToolVariation;
        [Reactive] public bool PitchFocusDim { get; private set; }

        public bool CursorTool => ToolIndex == 0;
        public bool PenTool => ToolIndex == 1 && PenToolIndex == 0;
        public bool PenPlusTool => ToolIndex == 1 && PenToolIndex == 1;
        public bool EraserTool => ToolIndex == 2;
        public bool DrawPitchTool => ToolIndex == 3 && DrawPitchToolIndex == 0;
        public bool OverwritePitchTool => ToolIndex == 3 && DrawPitchToolIndex == 1;
        public bool DrawLinePitchTool => ToolIndex == 4 && DrawLinePitchToolIndex == 0;
        public bool OverwriteLinePitchTool => ToolIndex == 4 && DrawLinePitchToolIndex == 1;
        public bool KnifeTool => ToolIndex == 5;

        public ObservableCollectionExtended<MenuItemViewModel> LegacyPlugins { get; private set; }
            = new ObservableCollectionExtended<MenuItemViewModel>();
        public ObservableCollectionExtended<MenuItemViewModel> NoteBatchEdits { get; private set; }
            = new ObservableCollectionExtended<MenuItemViewModel>();
        public ObservableCollectionExtended<MenuItemViewModel> LyricBatchEdits { get; private set; }
            = new ObservableCollectionExtended<MenuItemViewModel>();
        public ObservableCollectionExtended<MenuItemViewModel> ResetBatchEdits { get; private set; }
            = new ObservableCollectionExtended<MenuItemViewModel>();
        public ObservableCollectionExtended<MenuItemViewModel> ExternalBatchEdits { get; private set; }
            = new ObservableCollectionExtended<MenuItemViewModel>();
        public ObservableCollectionExtended<MenuItemViewModel> NotesContextMenuItems { get; private set; }
            = new ObservableCollectionExtended<MenuItemViewModel>();
        public Dictionary<Key, MenuItemViewModel> LegacyPluginShortcuts { get; private set; }
            = new Dictionary<Key, MenuItemViewModel>();

        [Reactive] public double Progress { get; set; }
        [Reactive] public bool CanUndo { get; set; } = false;
        [Reactive] public bool CanRedo { get; set; } = false;
        [Reactive] public string UndoText { get; set; } = ThemeManager.GetString("menu.edit.undo");
        [Reactive] public string RedoText { get; set; } = ThemeManager.GetString("menu.edit.redo");

        public ReactiveCommand<NoteHitInfo, Unit> NoteDeleteCommand { get; set; }
        public ReactiveCommand<NoteHitInfo, Unit> NoteCopyCommand { get; set; }
        public ReactiveCommand<NoteHitInfo, Unit> ClearPhraseCacheCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitEaseInOutCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitLinearCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitEaseInCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitEaseOutCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitSplineCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitSnapCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitDelCommand { get; set; }
        public ReactiveCommand<PitchPointHitInfo, Unit> PitAddCommand { get; set; }

        private ReactiveCommand<Classic.Plugin, Unit> legacyPluginCommand;

        public void ReloadShortcuts() {
            var newHotkeys = new Dictionary<string, string>();
            
            foreach (var sc in Preferences.Default.Shortcuts) {
                Enum.TryParse<KeyModifiers>(sc.ModifiersName, out var parsedMods);
                
                string mods = KeyTranslator.GetFriendlyModifiersName(parsedMods);
                string key = KeyTranslator.GetFriendlyName(sc.KeyName); 
                
                if (string.IsNullOrEmpty(mods) || sc.ModifiersName == "None") {
                    newHotkeys[sc.ActionId] = key;
                } else {
                    // Mac gets no separator, Windows gets standard "+" for menus
                    newHotkeys[sc.ActionId] = KeyTranslator.IsMac ? $"{mods}{key}" : $"{mods.Replace(" + ", "+")}+{key}";
                }
            }
            
            Hotkeys = newHotkeys;
        }

        public PianoRollViewModel() {
            ReloadShortcuts();
            NotesViewModel = new NotesViewModel();
            CurveViewModel = new CurveViewModel();
            ShowAppearancePanel = Preferences.Default.ShowAppearancePanel;
            ApplyDiffSingerQualityPresetCommand = ReactiveCommand.Create(() => ApplyDiffSingerRenderPreset(0));
            ApplyDiffSingerMediumPresetCommand = ReactiveCommand.Create(() => ApplyDiffSingerRenderPreset(1));
            ApplyDiffSingerLowPresetCommand = ReactiveCommand.Create(() => ApplyDiffSingerRenderPreset(2));
            ApplyDiffSingerExtraLowPresetCommand = ReactiveCommand.Create(() => ApplyDiffSingerRenderPreset(3));
            this.WhenAnyValue(vm => vm.ShowAppearancePanel)
                .Subscribe(show => {
                    Preferences.Default.ShowAppearancePanel = show;
                    Preferences.Save();
                    this.RaisePropertyChanged(nameof(IsAppearancePanelVisible));
                    this.RaisePropertyChanged(nameof(AppearancePanelLeadingGapWidth));
                    this.RaisePropertyChanged(nameof(AppearancePanelColumnWidth));
                });

            this.WhenAnyValue(vm => vm.ToolIndex)
                .Subscribe(index => EditTool.BaseTool = index);
            this.WhenAnyValue(vm => vm.PenToolIndex)
                .Subscribe(index => EditTool.PenToolVariation = index);
            this.WhenAnyValue(vm => vm.DrawPitchToolIndex)
                .Subscribe(index => EditTool.DrawPitchToolVariation = index);
            this.WhenAnyValue(vm => vm.DrawLinePitchToolIndex)
                .Subscribe(index => EditTool.DrawLinePitchToolVariation = index);
            this.WhenAnyValue(vm => vm.ToolIndex, vm => vm.DrawPitchToolIndex, vm => vm.DrawLinePitchToolIndex)
                .Subscribe(_ => UpdatePitchFocusDim());
            UpdatePitchFocusDim();

            NoteDeleteCommand = ReactiveCommand.Create<NoteHitInfo>(info => {
                NotesViewModel.DeleteSelectedNotes();
            });
            NoteCopyCommand = ReactiveCommand.Create<NoteHitInfo>(info => {
                NotesViewModel.CopyNotes();
            });
            ClearPhraseCacheCommand = ReactiveCommand.Create<NoteHitInfo>(info => {
                NotesViewModel.ClearPhraseCache();
            });
            PitEaseInOutCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.editpoint");
                DocManager.Inst.ExecuteCmd(new ChangePitchPointShapeCommand(NotesViewModel.Part, info.Note.pitch.data[info.Index], PitchPointShape.io));
                DocManager.Inst.EndUndoGroup();
            });
            PitLinearCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.editpoint");
                DocManager.Inst.ExecuteCmd(new ChangePitchPointShapeCommand(NotesViewModel.Part, info.Note.pitch.data[info.Index], PitchPointShape.l));
                DocManager.Inst.EndUndoGroup();
            });
            PitEaseInCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.editpoint");
                DocManager.Inst.ExecuteCmd(new ChangePitchPointShapeCommand(NotesViewModel.Part, info.Note.pitch.data[info.Index], PitchPointShape.i));
                DocManager.Inst.EndUndoGroup();
            });
            PitEaseOutCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.editpoint");
                DocManager.Inst.ExecuteCmd(new ChangePitchPointShapeCommand(NotesViewModel.Part, info.Note.pitch.data[info.Index], PitchPointShape.o));
                DocManager.Inst.EndUndoGroup();
            });
            PitSplineCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.editpoint");
                DocManager.Inst.ExecuteCmd(new ChangePitchPointShapeCommand(NotesViewModel.Part, info.Note.pitch.data[info.Index], PitchPointShape.sp));
                DocManager.Inst.EndUndoGroup();
            });
            PitSnapCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.editpoint");
                DocManager.Inst.ExecuteCmd(new SnapPitchPointCommand(NotesViewModel.Part, info.Note));
                DocManager.Inst.EndUndoGroup();
            });
            PitDelCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.delete");
                DocManager.Inst.ExecuteCmd(new DeletePitchPointCommand(NotesViewModel.Part, info.Note, info.Index));
                DocManager.Inst.EndUndoGroup();
            });
            PitAddCommand = ReactiveCommand.Create<PitchPointHitInfo>(info => {
                if (NotesViewModel.Part == null) { return; }
                DocManager.Inst.StartUndoGroup("command.pitch.add");
                DocManager.Inst.ExecuteCmd(new AddPitchPointCommand(NotesViewModel.Part, info.Note, new PitchPoint(info.X, info.Y, NotePresets.Default.DefaultPitchShape), info.Index + 1));
                DocManager.Inst.EndUndoGroup();
            });

            legacyPluginCommand = ReactiveCommand.Create<Classic.Plugin>(async plugin => {
                if (NotesViewModel.Part == null || NotesViewModel.Part.notes.Count == 0) {
                    return;
                }
                DocManager.Inst.ExecuteCmd(new LoadingNotification(typeof(PianoRoll), true, "legacy plugin"));
                
                try {
                    var part = NotesViewModel.Part;
                    UNote? first;
                    UNote? last;
                    if (NotesViewModel.Selection.IsEmpty) {
                        first = part.notes.First();
                        last = part.notes.Last();
                    } else {
                        first = NotesViewModel.Selection.FirstOrDefault();
                        last = NotesViewModel.Selection.LastOrDefault();
                    }
                    var runner = PluginRunner.from(PathManager.Inst, DocManager.Inst);
                    await runner.Execute(NotesViewModel.Project, part, first, last, plugin);

                } catch (Exception e) {
                    DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(e));
                } finally {
                    DocManager.Inst.ExecuteCmd(new LoadingNotification(typeof(PianoRoll), false, "legacy plugin"));
                }
            });
            LoadLegacyPlugins();
            MessageBus.Current.Listen<ShortcutsRefreshEvent>()
                .Subscribe(_ => ReloadShortcuts());
            DocManager.Inst.AddSubscriber(this);
        }

        private void SetUndoState() {
            CanUndo = DocManager.Inst.GetUndoState(out string? undoNameKey);
            if (!string.IsNullOrWhiteSpace(undoNameKey)) {
                UndoText = $"{ThemeManager.GetString("menu.edit.undo")}: {ThemeManager.GetString(undoNameKey)}";
            } else {
                UndoText = ThemeManager.GetString("menu.edit.undo");
            }
            CanRedo = DocManager.Inst.GetRedoState(out string? redoNameKey);
            if (!string.IsNullOrWhiteSpace(redoNameKey)) {
                RedoText = $"{ThemeManager.GetString("menu.edit.redo")}:  {ThemeManager.GetString(redoNameKey)}";
            } else {
                RedoText = ThemeManager.GetString("menu.edit.redo");
            }
        }

        private void LoadLegacyPlugins() {
            LegacyPlugins.Clear();
            
            LegacyPlugins.AddRange(DocManager.Inst.Plugins.Select(plugin => new MenuItemViewModel() {
                Header = plugin.Name,
                InputGesture = KeyTranslator.GetGesture(plugin.Name),
                Command = legacyPluginCommand,
                CommandParameter = plugin,
            }));

            LegacyPluginShortcuts.Clear();
            foreach (MenuItemViewModel menu in LegacyPlugins) {
                if (menu.InputGesture != null && !LegacyPluginShortcuts.ContainsKey(menu.InputGesture.Key)) {
                    LegacyPluginShortcuts.Add(menu.InputGesture.Key, menu);
                }
            }

            LegacyPlugins.Add(new MenuItemViewModel() { Header = "-", Height = 1 });
            LegacyPlugins.Add(new MenuItemViewModel() {
                Header = ThemeManager.GetString("pianoroll.menu.plugin.openfolder"),
                Command = ReactiveCommand.Create(() => {
                    try { OS.OpenFolder(PathManager.Inst.PluginsPath); } 
                    catch (Exception e) { DocManager.Inst.ExecuteCmd(new ErrorMessageNotification(e)); }
                })
            });
            LegacyPlugins.Add(new MenuItemViewModel() {
                Header = ThemeManager.GetString("pianoroll.menu.plugin.reload"),
                Command = ReactiveCommand.Create(() => {
                    DocManager.Inst.SearchAllLegacyPlugins();
                    LoadLegacyPlugins();
                })
            });
        }

        public void Undo() => DocManager.Inst.Undo();
        public void Redo() => DocManager.Inst.Redo();
        public void Cut() {
            if (CurveViewModel.IsSelected(NotesViewModel.PrimaryKey)) {
                CurveViewModel.Cut(NotesViewModel.Part!);
            } else {
                NotesViewModel.CutNotes();
            }
        }
        public void Copy() {
            if (CurveViewModel.IsSelected(NotesViewModel.PrimaryKey)) {
                CurveViewModel.Copy(NotesViewModel.Part!);
            } else {
                NotesViewModel.CopyNotes();
            }
        }
        public void Paste() {
            if (DocManager.Inst.NotesClipboard != null && DocManager.Inst.NotesClipboard.Count > 0) {
                NotesViewModel.PasteNotes();
            } else if (DocManager.Inst.CurvesClipboard != null && NotesViewModel.Part != null) {
                var track = NotesViewModel.Project.tracks[NotesViewModel.Part.trackNo];
                if (track.TryGetExpDescriptor(NotesViewModel.Project, NotesViewModel.PrimaryKey, out var descriptor)) {
                    CurveViewModel.Paste(NotesViewModel.Part, descriptor);
                }
            }
        }
        public void PastePlain() => NotesViewModel.PastePlainNotes();
        public void Delete() => NotesViewModel.DeleteSelectedNotes();
        public void SelectAll() => NotesViewModel.SelectAllNotes();

        public void MouseoverPhoneme(UPhoneme? phoneme) {
            MessageBus.Current.SendMessage(new PhonemeMouseoverEvent(phoneme));
        }

        void UpdatePitchFocusDim() {
            bool active = EditTool.IsMatch([
                EditTools.DrawPitchTool,
                EditTools.OverwritePitchTool,
                EditTools.DrawLinePitchTool,
                EditTools.OverwriteLinePitchTool,
            ]);
            if (PitchFocusDim == active) {
                return;
            }
            PitchFocusDim = active;
            MessageBus.Current.SendMessage(new NotesRefreshEvent());
        }

        static readonly string[] DiffSingerPresetLabels = { "HQ", "MQ", "LQ", "ELQ" };

        void ApplyDiffSingerRenderPreset(int preset) {
            int acoustic;
            int variance;
            int pitch;
            switch (preset) {
                case 0:
                    acoustic = Preferences.Default.DiffSingerSteps = 50;
                    variance = Preferences.Default.DiffSingerStepsVariance = 50;
                    pitch = Preferences.Default.DiffSingerStepsPitch = 20;
                    break;
                case 1:
                    acoustic = Preferences.Default.DiffSingerSteps = 20;
                    variance = Preferences.Default.DiffSingerStepsVariance = 20;
                    pitch = Preferences.Default.DiffSingerStepsPitch = 20;
                    break;
                case 2:
                    acoustic = Preferences.Default.DiffSingerSteps = 10;
                    variance = Preferences.Default.DiffSingerStepsVariance = 10;
                    pitch = Preferences.Default.DiffSingerStepsPitch = 20;
                    break;
                case 3:
                    acoustic = Preferences.Default.DiffSingerSteps = 1;
                    variance = Preferences.Default.DiffSingerStepsVariance = 1;
                    pitch = Preferences.Default.DiffSingerStepsPitch = 1;
                    break;
                default:
                    return;
            }
            Preferences.Save();
            if (appearancePreferences != null) {
                appearancePreferences.DiffSingerSteps = acoustic;
                appearancePreferences.DiffSingerStepsVariance = variance;
                appearancePreferences.DiffSingerStepsPitch = pitch;
            }
            var label = DiffSingerPresetLabels[preset];
            var message = string.Format(
                ThemeManager.GetString("progress.diffsinger.preset"),
                label, acoustic, variance, pitch);
            DocManager.Inst.ExecuteCmd(new ProgressBarNotification(0, message, autoClearSeconds: 4));
        }

        #region ICmdSubscriber

        public void OnNext(UCommand cmd, bool isUndo) {
            if (cmd is ProgressBarNotification progressBarNotification) {
                if (PianoRollDetached) {
                    Dispatcher.UIThread.InvokeAsync(() => {
                        Progress = progressBarNotification.Progress;
                    }, DispatcherPriority.Background);
                }
            }
            SetUndoState();
        }

        #endregion
    }
}
