namespace OpenUtau.App.Controls {
    public static class WorkspaceDockPanelMetrics {
        public const double DefaultWidth = 300;
        public const double MinWidth = 300;
        public const double MaxWidth = 450;

        /// <summary>Panel edge to content / after scrollbar.</summary>
        public const double ContentInsetHorizontal = 10;

        /// <summary>Gap between content right edge and scrollbar lane.</summary>
        public const double GapBeforeScrollbar = 10;

        /// <summary>Right margin on scroll content in overlay mode (gap before scrollbar lane).</summary>
        public const double OverlayContentRightMargin = GapBeforeScrollbar;

        public static double ClampWidth(double width) =>
            System.Math.Clamp(width, MinWidth, MaxWidth);
    }
}
