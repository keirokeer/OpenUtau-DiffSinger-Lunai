using System;

using System.Linq;

using Avalonia;

using Avalonia.Controls;

using Avalonia.Controls.Primitives;

using Avalonia.Layout;

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

                bar.HorizontalAlignment = HorizontalAlignment.Stretch;

                bar.VerticalAlignment = VerticalAlignment.Stretch;

                bar.Width = double.NaN;

                bar.Height = double.NaN;

                bar.Margin = new Thickness(0, 4, 0, 0);

                bar.ZIndex = 0;

            } else {

                bar.HorizontalAlignment = HorizontalAlignment.Stretch;

                bar.VerticalAlignment = VerticalAlignment.Bottom;

                bar.Width = double.NaN;

                bar.Height = OverlayBarThickness;

                bar.ZIndex = 350;

            }

        }



        public static void ApplyVerticalScrollBar(ScrollBar bar, bool classic, ScrollViewer? hostScrollViewer = null) {

            if (!IsInVisualTree(bar)) {

                return;

            }

            bar.Classes.Set("overlay", !classic);

            bar.Classes.Set("music", classic);

            bar.AllowAutoHide = false;

            if (classic) {

                bar.HorizontalAlignment = HorizontalAlignment.Stretch;

                bar.VerticalAlignment = VerticalAlignment.Stretch;

                bar.Width = double.NaN;

                bar.Height = double.NaN;

                bar.Margin = new Thickness(4, 0, 4, 0);

                bar.ZIndex = 0;

            } else {

                bool dockPanelScroll = hostScrollViewer?.Classes.Contains("workspaceDockPanelScroll") == true;

                bar.HorizontalAlignment = HorizontalAlignment.Right;

                bar.VerticalAlignment = VerticalAlignment.Stretch;

                bar.Width = OverlayBarThickness;

                bar.Height = double.NaN;

                bar.Margin = dockPanelScroll ? new Thickness(0) : new Thickness(0, 0, 3, 0);

                bar.ZIndex = 400;

            }

        }



        public static void ApplyScrollViewer(ScrollViewer scrollViewer, bool classic) {

            if (!IsInVisualTree(scrollViewer)) {

                return;

            }

            scrollViewer.Classes.Set("overlay", !classic);

            void apply() {

                ApplyDockPanelScrollLayout(scrollViewer, classic);

                foreach (var bar in scrollViewer.GetVisualDescendants().OfType<ScrollBar>()) {

                    ApplyVerticalScrollBar(bar, classic, scrollViewer);

                }

            }

            if (scrollViewer.IsInitialized) {

                apply();

            } else {

                scrollViewer.Loaded += (_, _) => apply();

            }

        }



        static void ApplyDockPanelScrollLayout(ScrollViewer scrollViewer, bool classic) {

            if (!scrollViewer.Classes.Contains("workspaceDockPanelScroll")) {

                return;

            }

            scrollViewer.Padding = new Thickness(0);

            scrollViewer.ClipToBounds = false;

            if (scrollViewer.Content is not Control content) {

                return;

            }

            content.HorizontalAlignment = HorizontalAlignment.Stretch;

            double right = classic ? 0 : WorkspaceDockPanelMetrics.OverlayContentRightMargin;

            var margin = content.Margin;
            content.Margin = new Thickness(0, margin.Top, right, margin.Bottom);

        }

    }

}


