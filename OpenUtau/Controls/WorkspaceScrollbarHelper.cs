using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenUtau.Core.Util;

namespace OpenUtau.App.Controls {
    /// <summary>Applies classic vs overlay scrollbar layout and classes for workspace panels.</summary>
    public static class WorkspaceScrollbarHelper {
        public const double OverlayThumbThickness = 8;
        public const double OverlayBarThickness = 10;

        public static bool IsInVisualTree(Control? control) =>
            control != null && control.GetVisualRoot() != null;

        public static void ApplyHorizontalScrollBar(ScrollBar bar, bool classic) {
            if (!IsInVisualTree(bar)) {
                return;
            }
            bar.Classes.Set("overlay", !classic);
            bar.Classes.Set("music", classic);
            bar.AllowAutoHide = false;
            if (classic) {
                bar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                bar.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                bar.Width = double.NaN;
                bar.Height = double.NaN;
                bar.Margin = new Thickness(0, 4, 0, 0);
                bar.ZIndex = 0;
            } else {
                bar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                bar.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom;
                bar.Width = double.NaN;
                bar.Height = OverlayBarThickness;
                bar.ZIndex = 350;
            }
        }

        public static void ApplyVerticalScrollBar(ScrollBar bar, bool classic) {
            if (!IsInVisualTree(bar)) {
                return;
            }
            bar.Classes.Set("overlay", !classic);
            bar.Classes.Set("music", classic);
            bar.AllowAutoHide = false;
            if (classic) {
                bar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
                bar.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                bar.Width = double.NaN;
                bar.Height = double.NaN;
                bar.Margin = new Thickness(4, 0, 4, 0);
                bar.ZIndex = 0;
            } else {
                bar.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                bar.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch;
                bar.Width = OverlayBarThickness;
                bar.Height = double.NaN;
                bar.Margin = new Thickness(0, 0, 3, 0);
                bar.ZIndex = 400;
            }
        }

        public static void ApplyScrollViewer(ScrollViewer scrollViewer, bool classic) {
            if (!IsInVisualTree(scrollViewer)) {
                return;
            }
            scrollViewer.Classes.Set("overlay", !classic);
            void applyBars() {
                foreach (var bar in scrollViewer.GetVisualDescendants().OfType<ScrollBar>()) {
                    ApplyVerticalScrollBar(bar, classic);
                }
            }
            if (scrollViewer.IsInitialized) {
                applyBars();
            } else {
                scrollViewer.Loaded += (_, _) => applyBars();
            }
        }
    }
}
