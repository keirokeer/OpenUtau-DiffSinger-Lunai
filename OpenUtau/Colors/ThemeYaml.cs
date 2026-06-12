using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OpenUtau.Core;
using Serilog;

namespace OpenUtau.Colors;

public class ThemeYaml {
    static readonly Dictionary<string, FieldInfo> ColorFields = typeof(ThemeYaml)
        .GetFields(BindingFlags.Public | BindingFlags.Instance)
        .Where(field => field.FieldType == typeof(string) && field.Name != nameof(Name))
        .ToDictionary(field => field.Name);

    public string Name = "Custom YAML";
    public bool IsDarkMode;

    public string BackgroundColor = "#FFFFFF";
    public string BackgroundColorPointerOver = "#F0F0F0";
    public string TransportToolbarOffHoverColor = "#F6F6F6";
    public string BackgroundColorPressed = "#E0E0E0";
    public string BackgroundColorDisabled = "#D0D0D0";

    public string ForegroundColor = "#000000";
    public string ForegroundColorPointerOver = "#000000";
    public string ForegroundColorPressed = "#202020";
    public string ForegroundColorDisabled = "#808080";

    public string BorderColor = "#707070";
    public string BorderColorPointerOver = "#B0B0B0";

    public string SystemAccentColor = "#4EA6EA";
    public string SystemAccentColorLight1 = "#90CAF9";
    public string SystemAccentColorDark1 = "#1E88E5";

    public string NeutralAccentColor = "#ADA1B3";
    public string NeutralAccentColorPointerOver = "#948A99";
    public string AccentColor1 = "#4EA6EA";
    public string AccentColor1Note = "#A9A6CD";
    public string AccentColor2 = "#FF679D";
    public string AccentColor3 = "#E62E6E";

    public string NoteBorderColor = "#7B79D9";
    public string NoteBorderColorPressed = "#4C4B98";

    public string TickLineColor = "#AFA3B5";
    public string BarNumberColor = "#AFA3B5";
    public string FinalPitchColor = "#C0C0C0";
    public string TrackBackgroundAltColor = "#F0F0F0";
    public string WarningColor = "#FFF4CE";

    public string ToolbarCheckedHoverColor = "#E0E0E0";
    public string ToolTipForegroundColor = "#FFFFFF";
    public string WorkspaceCanvasColor = "#E4E4E8";
    public string WorkspaceCardColor = "#FCFCFC";
    public string WorkspaceElevatedSurfaceColor = "#E8E8EC";
    public string MutedIconColor = "#808080";
    public string PianoRollWaveformPeakColor = "#59999999";

    public string PianoRollToolbarStripColor = "#202020";
    public string PianoRollToolbarButtonHoverColor = "#313131";
    public string PianoRollTimelineStripColor = "#1B1B1B";
    public string AppTopBarTransportStripColor = "#E8E8EC";
    public string AppTopBarTransportHoverColor = "#F4F4F8";
    public string AppTopBarValueStripColor = "#D8D8DE";
    public string AppTopBarValueDividerColor = "#C0C0C8";

    public string WhiteKeyColorLeft = "Transparent";
    public string WhiteKeyColorRight = "Transparent";
    public string ToolbarCheckedPianoLightColor = "#FCFCFC";
    public string WhiteKeyNameColor = "#343434";

    public string CenterKeyColorLeft = "#A9A6CD";
    public string CenterKeyColorRight = "#D3CFFF";
    public string CenterKeyNameColor = "#111111";

    public string BlackKeyColorLeft = "#45445C";
    public string BlackKeyColorRight = "#45445C";
    public string BlackKeyNameColor = "#FCFCFC";

    public string? GetColor(string key) {
        if (!ColorFields.TryGetValue(key, out var field)) {
            return null;
        }
        return (string?)field.GetValue(this);
    }

    public void SetColor(string key, string value) {
        if (ColorFields.TryGetValue(key, out var field)) {
            field.SetValue(this, value);
        }
    }

    public void ImportFromResources(IResourceDictionary resources) {
        foreach (var entry in resources) {
            if (entry.Key is not string key) {
                continue;
            }
            if (key == nameof(IsDarkMode) && entry.Value is bool dark) {
                IsDarkMode = dark;
                continue;
            }
            if (entry.Value is Color color && ColorFields.ContainsKey(key)) {
                SetColor(key, ThemeColorStorage.ToStorageString(color));
            }
        }
    }

    public void FillMissingFrom(ThemeYaml defaults) {
        foreach (var key in ThemeColorCatalog.AllResourceKeys) {
            if (string.IsNullOrWhiteSpace(GetColor(key))) {
                SetColor(key, defaults.GetColor(key) ?? string.Empty);
            }
        }
    }

    public void ApplyToResources() {
        if (Application.Current == null) {
            return;
        }
        var defaults = BuiltInThemeLoader.CreateFromBuiltIn(IsDarkMode ? "Dark" : "Light", Name);
        FillMissingFrom(defaults);

        ThemeApplicator.ApplyCustomBase();
        Application.Current.Resources["IsDarkMode"] = IsDarkMode;
        foreach (var key in ThemeColorCatalog.AllResourceKeys) {
            SetResourceColor(key, GetColor(key) ?? string.Empty);
        }
    }

    public static ThemeYaml LoadFromFile(string path) {
        try {
            var yaml = Yaml.DefaultDeserializer.Deserialize<ThemeYaml>(File.ReadAllText(path, Encoding.UTF8));
            if (HasBrokenBasePalette(yaml)) {
                var repaired = BuiltInThemeLoader.CreateFromBuiltIn(
                    yaml.IsDarkMode ? "Dark" : "Light",
                    yaml.Name);
                yaml = repaired;
            }
            var defaults = BuiltInThemeLoader.CreateFromBuiltIn(yaml.IsDarkMode ? "Dark" : "Light", yaml.Name);
            yaml.FillMissingFrom(defaults);
            return yaml;
        } catch (Exception exception) {
            Log.Error(exception, "Failed to parse yaml in {Path}", path);
            return BuiltInThemeLoader.CreateFromBuiltIn("Light", "Custom YAML");
        }
    }

    /// <summary>Themes saved before built-in import was fixed: is_dark_mode set but palette left at ctor defaults.</summary>
    static bool HasBrokenBasePalette(ThemeYaml yaml) {
        if (yaml.IsDarkMode) {
            return string.Equals(yaml.BackgroundColor, "#FFFFFF", StringComparison.OrdinalIgnoreCase)
                && string.Equals(yaml.AccentColor1, "#4EA6EA", StringComparison.OrdinalIgnoreCase);
        }
        return string.Equals(yaml.AccentColor1, "#E0E0E0", StringComparison.OrdinalIgnoreCase)
            && (string.Equals(yaml.BackgroundColor, "#252525", StringComparison.OrdinalIgnoreCase)
                || string.Equals(yaml.BackgroundColor, "#222222", StringComparison.OrdinalIgnoreCase)
                || string.Equals(yaml.BackgroundColor, "#212121", StringComparison.OrdinalIgnoreCase));
    }

    public void SaveToFile(string path) {
        File.WriteAllText(path, Yaml.DefaultSerializer.Serialize(this), Encoding.UTF8);
    }

    static void SetResourceColor(string resourceKey, string colorString) {
        if (ThemeColorStorage.TryParse(colorString, out var color)) {
            Application.Current!.Resources[resourceKey] = color;
        } else {
            Log.Error("Failed to parse color \"{Color}\" in custom theme", colorString);
        }
    }
}
