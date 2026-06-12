using Avalonia.Media;

namespace OpenUtau.Colors;

public static class ThemeColorStorage {
    public static bool TryParse(string? colorString, out Color color) {
        color = default;
        if (string.IsNullOrWhiteSpace(colorString)) {
            return false;
        }
        return Color.TryParse(colorString.Trim(), out color);
    }

    public static Color ParseOrDefault(string? colorString, Color fallback) {
        return TryParse(colorString, out var color) ? color : fallback;
    }

    public static string ToStorageString(Color color) {
        if (color.A < 255) {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
