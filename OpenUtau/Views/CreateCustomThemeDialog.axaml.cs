using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace OpenUtau.App.Views;

public partial class CreateCustomThemeDialog : Window {
    public Action<string, string>? onFinish;

    public CreateCustomThemeDialog() {
        InitializeComponent();
        OkButton.Click += OkButtonClick;
        NameBox.AttachedToVisualTree += (_, _) => {
            NameBox.SelectAll();
            NameBox.Focus();
        };
    }

    void OkButtonClick(object? sender, RoutedEventArgs e) {
        Finish();
    }

    void Finish() {
        var name = NameBox.Text ?? string.Empty;
        var baseTheme = BaseThemeBox.SelectedIndex == 1 ? "Dark" : "Light";
        onFinish?.Invoke(name, baseTheme);
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        if (e.Key == Key.Escape) {
            e.Handled = true;
            Close();
        } else if (e.Key == Key.Enter) {
            e.Handled = true;
            Finish();
        } else {
            base.OnKeyDown(e);
        }
    }
}
