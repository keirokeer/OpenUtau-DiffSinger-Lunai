using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Avalonia;
using OpenUtau.Core;
using OpenUtau.Core.Util;
using Serilog;

namespace OpenUtau.Colors;

public class CustomTheme {
    public static Dictionary<string, string> Themes = [];
    public static ThemeYaml Default;

    static CustomTheme() {
        Default = BuiltInThemeLoader.CreateFromBuiltIn("Light", "Custom YAML");
        ListThemes();
    }

    public static void Load(string themeName) {
        if (!string.IsNullOrEmpty(themeName) && Themes.TryGetValue(themeName, out var themePath) && File.Exists(themePath)) {
            Default = ThemeYaml.LoadFromFile(themePath);
            return;
        }

        Preferences.Default.ThemeName = "Light";
        Default = BuiltInThemeLoader.CreateFromBuiltIn("Light", "Custom YAML");
    }

    public static void ListThemes() {
        Themes.Clear();
        Directory.CreateDirectory(PathManager.Inst.ThemesPath);
        foreach (var item in Directory.EnumerateFiles(PathManager.Inst.ThemesPath, "*.yaml")) {
            try {
                string baseName = Yaml.DefaultDeserializer.Deserialize<ThemeYaml>(File.ReadAllText(item, Encoding.UTF8)).Name;
                string resolvedName = baseName;
                int dupIter = 1;
                while (Themes.ContainsKey(resolvedName)) {
                    resolvedName = $"{baseName} ({dupIter})";
                    dupIter++;
                }
                Themes.Add(resolvedName, item);
            } catch (Exception exception) {
                Log.Error(exception, "Failed to parse yaml in {Path}", item);
            }
        }
    }

    public static void ApplyTheme(string themeName) {
        Load(themeName);
        ThemeApplicator.Apply(Default);
    }
}
