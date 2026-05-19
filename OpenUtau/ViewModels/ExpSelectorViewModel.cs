using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Media;
using OpenUtau.App.Controls;
using OpenUtau.Core;
using OpenUtau.Core.Render;
using OpenUtau.Core.Ustx;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace OpenUtau.App.ViewModels {
    public class ExpSelectorViewModel : ViewModelBase, ICmdSubscriber {
        [Reactive] public int Index { get; set; }
        [Reactive] public int SelectedIndex { get; set; }
        [Reactive] public ExpDisMode DisplayMode { get; set; }
        [Reactive] public UExpressionDescriptor? Descriptor { get; set; }
        public string Abbr {
            get{
                if (Descriptor == null) {
                    return "";
                }
                return Descriptor.abbr;
            }
        }
        public ObservableCollection<UExpressionDescriptor> Descriptors => descriptors;
        public string Header => header.Value;
        [Reactive] public IBrush TagBrush { get; set; }
        [Reactive] public IBrush Background { get; set; }

        ObservableCollection<UExpressionDescriptor> descriptors = new ObservableCollection<UExpressionDescriptor>();
        ObservableAsPropertyHelper<string> header;
        int currentTrackNo = -1;

        public ExpSelectorViewModel() {
            DocManager.Inst.AddSubscriber(this);
            this.WhenAnyValue(x => x.DisplayMode)
                .Subscribe(_ => RefreshBrushes());
            this.WhenAnyValue(x => x.Descriptor)
                .Select(descriptor => descriptor == null ? string.Empty : descriptor.abbr.ToUpperInvariant())
                .ToProperty(this, x => x.Header, out header);
            this.WhenAnyValue(x => x.Descriptor)
                .Subscribe(SelectionChanged);
            this.WhenAnyValue(x => x.Index, x => x.Descriptors)
                .Subscribe(tuple => {
                    SetExp(DocManager.Inst.Project.expSelectors[tuple.Item1]);
                });
            MessageBus.Current.Listen<ThemeChangedEvent>()
                .Subscribe(_ => RefreshBrushes());
            TagBrush = ThemeManager.ExpNameBrush;
            Background = ThemeManager.ExpBrush;
            OnListChange();
        }

        public bool SetExp(string abbr) {
            if(Descriptors.Any(d => d.abbr == abbr)) {
                Descriptor = Descriptors.First(d => d.abbr == abbr);
                return true;
            } else {
                if (Descriptors != null && Descriptors.Count > Index) {
                    Descriptor = Descriptors[Index];
                }
                return false;
            }
        }

        public void OnSelected(bool store) {
            if (DisplayMode != ExpDisMode.Visible && Descriptor != null) {
                DocManager.Inst.ExecuteCmd(new SelectExpressionNotification(Descriptor.abbr, Index, true));
            }
            if(store) {
                var project = DocManager.Inst.Project;
                project.expSecondary = project.expPrimary;
                project.expPrimary = Index;
            }
        }

        void SelectionChanged(UExpressionDescriptor? descriptor) {
            if (descriptor != null) {
                DocManager.Inst.ExecuteCmd(new SelectExpressionNotification(descriptor.abbr, Index, DisplayMode != ExpDisMode.Visible));
            }
            if (!string.IsNullOrEmpty(Abbr)) {
                DocManager.Inst.Project.expSelectors[Index] = Abbr;
            }
        }

        public void OnNext(UCommand cmd, bool isUndo) {
            if (cmd is LoadProjectNotification) {
                currentTrackNo = -1;
                OnListChange();
            } else if (cmd is LoadPartNotification loadPart) {
                currentTrackNo = loadPart.part.trackNo;
                OnListChange();
            } else if (cmd is ConfigureExpressionsCommand ||
                cmd is ValidateProjectNotification ||
                cmd is SingersRefreshedNotification) {
                OnListChange();
            } else if (cmd is SelectExpressionNotification) {
                OnSelectExp((SelectExpressionNotification)cmd);
            }
        }

        private void OnListChange() {
            var selectedIndex = SelectedIndex;
            var savedAbbr = Descriptor?.abbr ?? DocManager.Inst.Project.expSelectors[Index];
            Descriptors.Clear();
            foreach (var descriptor in GetVisibleDescriptors(currentTrackNo)) {
                Descriptors.Add(descriptor);
            }
            if (Descriptors.Count == 0) {
                return;
            }
            if (!string.IsNullOrEmpty(savedAbbr) && Descriptors.Any(d => d.abbr == savedAbbr)) {
                SelectedIndex = Descriptors.IndexOf(Descriptors.First(d => d.abbr == savedAbbr));
            } else if (selectedIndex >= Descriptors.Count) {
                SelectedIndex = Math.Min(Index, Descriptors.Count - 1);
            } else {
                SelectedIndex = selectedIndex;
            }
        }

        static IEnumerable<UExpressionDescriptor> GetVisibleDescriptors(int trackNo) {
            var project = DocManager.Inst.Project;
            if (trackNo >= 0 && trackNo < project.tracks.Count) {
                var track = project.tracks[trackNo];
                if (track.RendererSettings.Renderer?.SingerType == USingerType.DiffSinger) {
                    return track.GetSupportedExps(project);
                }
            }
            return project.expressions.Values;
        }

        private void OnSelectExp(SelectExpressionNotification cmd) {
            if (Descriptors.Count == 0) {
                return;
            }
            if (cmd.SelectorIndex == Index) {
                if (Descriptors[SelectedIndex].abbr != cmd.ExpKey) {
                    SelectedIndex = Descriptors.IndexOf(Descriptors.First(d => d.abbr == cmd.ExpKey));
                }
                DisplayMode = ExpDisMode.Visible;
            } else if (cmd.UpdateShadow) {
                DisplayMode = DisplayMode == ExpDisMode.Visible ? ExpDisMode.Shadow : ExpDisMode.Hidden;
            }
        }

        private void RefreshBrushes() {
            TagBrush = DisplayMode == ExpDisMode.Visible
                    ? ThemeManager.ExpActiveNameBrush
                    : DisplayMode == ExpDisMode.Shadow
                    ? ThemeManager.ExpShadowNameBrush
                    : ThemeManager.ExpNameBrush;
            Background = DisplayMode == ExpDisMode.Visible
                    ? ThemeManager.ExpActiveBrush
                    : DisplayMode == ExpDisMode.Shadow
                    ? ThemeManager.ExpShadowBrush
                    : ThemeManager.ExpBrush;
        }
    }
}
