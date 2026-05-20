using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using OpenUtau.App;

namespace OpenUtau.App.Controls {
    public sealed class WorkspaceSectionExpanderChrome {
        public IBrush SectionHeaderBackground { get; private set; } = Brushes.Transparent;
        public IBrush SectionHeaderBackgroundPointerOver { get; private set; } = Brushes.Transparent;
        public IBrush SectionHeaderBackgroundPressed { get; private set; } = Brushes.Transparent;
        public IBrush SectionContentBackground { get; private set; } = Brushes.Transparent;

        public void UpdateFromTrackColor(string trackColorName) {
            Color noteColor = ThemeManager.GetTrackColor(trackColorName).NoteColor.Color;
            SectionHeaderBackground = new SolidColorBrush(NoteColorWithOpacity(noteColor, 0.25));
            SectionHeaderBackgroundPointerOver = new SolidColorBrush(NoteColorWithOpacity(noteColor, 0.55));
            SectionHeaderBackgroundPressed = new SolidColorBrush(NoteColorWithOpacity(noteColor, 0.80));
            SectionContentBackground = ThemeManager.WorkspaceElevatedSurfaceBrush;
        }

        public void Apply(Control root, IBrush header, IBrush headerPointerOver, IBrush headerPressed, IBrush content) {
            SectionHeaderBackground = header;
            SectionHeaderBackgroundPointerOver = headerPointerOver;
            SectionHeaderBackgroundPressed = headerPressed;
            SectionContentBackground = content;
            Apply(root);
        }

        public bool HasAppliedNotePropsExpanders(Control root) {
            if (!root.IsLoaded) {
                return false;
            }
            foreach (var expander in root.GetVisualDescendants().OfType<Expander>()) {
                if (!expander.Classes.Contains("notePropsExpander")) {
                    continue;
                }
                expander.ApplyTemplate();
                if (expander.GetVisualDescendants().OfType<ToggleButton>().FirstOrDefault(b => b.Name == "ExpanderHeader") is ToggleButton header
                    && header.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "ToggleButtonBackground") is Border headerBg
                    && headerBg.Tag is HeaderBrushState) {
                    return true;
                }
            }
            return false;
        }

        public void Apply(Control root) {
            if (!root.IsLoaded) {
                return;
            }
            foreach (var expander in root.GetVisualDescendants().OfType<Expander>()) {
                if (!expander.Classes.Contains("notePropsExpander")) {
                    continue;
                }
                expander.ApplyTemplate();
                if (expander.GetVisualDescendants().OfType<ToggleButton>().FirstOrDefault(b => b.Name == "ExpanderHeader") is not ToggleButton header) {
                    continue;
                }
                header.ApplyTemplate();
                if (header.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "ToggleButtonBackground") is not Border headerBg) {
                    continue;
                }
                if (headerBg.Tag is not HeaderBrushState state) {
                    state = new HeaderBrushState();
                    headerBg.Tag = state;
                    if (!header.Classes.Contains("notePropsHeaderWired")) {
                        header.Classes.Add("notePropsHeaderWired");
                        header.PointerEntered += (_, _) => UpdateHeaderBrush(header, headerBg);
                        header.PointerExited += (_, _) => UpdateHeaderBrush(header, headerBg);
                        header.PointerPressed += (_, _) => UpdateHeaderBrush(header, headerBg);
                        header.PointerReleased += (_, _) => UpdateHeaderBrush(header, headerBg);
                        header.GetObservable(ToggleButton.IsPressedProperty)
                            .Subscribe(_ => UpdateHeaderBrush(header, headerBg));
                    }
                }
                state.Normal = SectionHeaderBackground;
                state.PointerOver = SectionHeaderBackgroundPointerOver;
                state.Pressed = SectionHeaderBackgroundPressed;
                UpdateHeaderBrush(header, headerBg);

                if (expander.GetVisualDescendants().OfType<Border>().FirstOrDefault(b => b.Name == "ExpanderContent") is Border content) {
                    content.Background = SectionContentBackground;
                    content.BorderBrush = Brushes.Transparent;
                    content.BorderThickness = new Thickness(0);
                }
            }
        }

        static void UpdateHeaderBrush(ToggleButton header, Border headerBg) {
            if (headerBg.Tag is not HeaderBrushState state) {
                return;
            }
            HeaderBrushKind kind = header.IsPressed
                ? HeaderBrushKind.Pressed
                : header.IsPointerOver ? HeaderBrushKind.PointerOver : HeaderBrushKind.Normal;
            headerBg.Background = kind switch {
                HeaderBrushKind.PointerOver => state.PointerOver,
                HeaderBrushKind.Pressed => state.Pressed,
                _ => state.Normal,
            };
        }

        enum HeaderBrushKind { Normal, PointerOver, Pressed }

        sealed class HeaderBrushState {
            public IBrush? Normal;
            public IBrush? PointerOver;
            public IBrush? Pressed;
        }

        public static Color NoteColorWithOpacity(Color color, double opacity) {
            return Color.FromArgb(
                (byte)Math.Clamp((int)Math.Round(255 * opacity), 0, 255),
                color.R,
                color.G,
                color.B);
        }
    }
}
