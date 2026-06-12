using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using OpenUtau.App;

namespace OpenUtau.Colors;

public static class ThemeApplicator {
    public static void ApplyCustomBase() {
        if (Application.Current == null) {
            return;
        }
        if (Application.Current.Resources["themes-custom"] is not IResourceDictionary custom) {
            return;
        }
        foreach (var item in custom) {
            Application.Current.Resources[item.Key] = item.Value;
        }
    }

    public static void Apply(ThemeYaml yaml) {
        if (Application.Current == null) {
            return;
        }
        yaml.ApplyToResources();
        Application.Current.RequestedThemeVariant = yaml.IsDarkMode
            ? ThemeVariant.Dark
            : ThemeVariant.Light;
        ThemeManager.LoadTheme();
    }
}
