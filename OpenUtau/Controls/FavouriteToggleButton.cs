using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using OpenUtau.App;

namespace OpenUtau.App.Controls;

public class FavouriteToggleButton : ToggleButton {
    static readonly IBrush MenuHoverBrush = Brushes.White;

    readonly Path _iconPath;
    readonly Viewbox _iconHost;
    bool useSingerMenuColors;

    public FavouriteToggleButton() {
        ClipToBounds = false;
        Padding = new Thickness(0);
        _iconPath = new Path {
            Fill = Brushes.Transparent,
            Stroke = ThemeManager.AccentBrush3,
            StrokeThickness = 2,
            Data = Geometry.Parse("M12,21.35L10.55,20.03C5.4,15.36,2,12.28,2,8.5C2,5.42,4.42,3,7.5,3C9.24,3,10.91,3.81,12,5.09C13.09,3.81,14.76,3,16.5,3C19.58,3,22,5.42,22,8.5C22,12.28,18.6,15.36,13.45,20.04L12,21.35Z"),
        };
        _iconHost = new Viewbox {
            Stretch = Stretch.Uniform,
            Margin = new Thickness(-1, 0, 2, 0),
            Child = _iconPath,
        };
        Content = _iconHost;
        PropertyChanged += (_, e) => {
            if (e.Property == IsCheckedProperty) {
                UpdateIcon();
            }
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        useSingerMenuColors = Classes.Contains("singerMenuFavourite");
        UpdateIcon();
    }

    protected override void OnPointerEntered(PointerEventArgs e) {
        base.OnPointerEntered(e);
        UpdateIcon();
    }

    protected override void OnPointerExited(PointerEventArgs e) {
        base.OnPointerExited(e);
        UpdateIcon();
    }

    void UpdateIcon() {
        if (useSingerMenuColors) {
            var brush = IsPointerOver ? MenuHoverBrush : ThemeManager.MutedIconBrush;
            _iconPath.StrokeThickness = 1.5;
            _iconHost.Margin = new Thickness(-1, 0, 2, 0);
            _iconPath.Stroke = brush;
            _iconPath.Fill = (IsChecked ?? false) ? brush : Brushes.Transparent;
            return;
        }
        _iconPath.StrokeThickness = 2;
        _iconHost.Margin = new Thickness(0);
        if (IsChecked ?? false) {
            _iconPath.Fill = ThemeManager.AccentBrush3;
            _iconPath.Stroke = ThemeManager.AccentBrush3;
        } else {
            _iconPath.Fill = Brushes.Transparent;
            _iconPath.Stroke = ThemeManager.AccentBrush3;
        }
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
