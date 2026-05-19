using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using OpenUtau.App;

namespace OpenUtau.App.Controls {
    public class ApplyToAllTracksButton : Button {
        static readonly IBrush MenuHoverBrush = Brushes.White;

        readonly Path _iconPath;

        public ApplyToAllTracksButton() {
            Padding = new Thickness(0);
            BorderThickness = new Thickness(0);
            Background = Brushes.Transparent;
            Focusable = false;
            ClipToBounds = false;
            _iconPath = new Path {
                Stroke = ThemeManager.MutedIconBrush,
                StrokeThickness = 1.75,
                Data = Geometry.Parse("M3,4 H11 M3,8 H11 M3,12 H11 M10,2 L13,4 L10,6 M10,6 L13,4 L10,2"),
            };
            Content = _iconPath;
        }

        protected override void OnPointerEntered(PointerEventArgs e) {
            base.OnPointerEntered(e);
            _iconPath.Stroke = MenuHoverBrush;
        }

        protected override void OnPointerExited(PointerEventArgs e) {
            base.OnPointerExited(e);
            _iconPath.Stroke = ThemeManager.MutedIconBrush;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
            e.Handled = true;
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            e.Handled = true;
        }
    }
}
