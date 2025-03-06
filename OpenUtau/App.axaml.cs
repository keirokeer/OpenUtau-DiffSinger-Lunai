using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using OpenUtau.App.Views;
using OpenUtau.Classic;
using OpenUtau.Core;
using Serilog;
using Vortice;
using YamlDotNet.Core.Tokens;

namespace OpenUtau.App {
    public class App : Application {
        public override void Initialize() {
            Log.Information("Initializing application.");
            AvaloniaXamlLoader.Load(this);
            InitializeCulture();
            InitializeTheme();
            Log.Information("Initialized application.");
        }

        public override void OnFrameworkInitializationCompleted() {
            Log.Information("Framework initialization completed.");
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new SplashWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        public void InitializeCulture() {
            Log.Information("Initializing culture.");
            string sysLang = CultureInfo.InstalledUICulture.Name;
            string prefLang = Core.Util.Preferences.Default.Language;
            var languages = GetLanguages();
            if (languages.ContainsKey(prefLang)) {
                SetLanguage(prefLang);
            } else if (languages.ContainsKey(sysLang)) {
                SetLanguage(sysLang);
                Core.Util.Preferences.Default.Language = sysLang;
                Core.Util.Preferences.Save();
            } else {
                SetLanguage("en-US");
            }

            // Force using InvariantCulture to prevent issues caused by culture dependent string conversion, especially for floating point numbers.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Log.Information("Initialized culture.");
        }

        public static Dictionary<string, IResourceProvider> GetLanguages() {
            if (Current == null) {
                return new();
            }
            var result = new Dictionary<string, IResourceProvider>();
            foreach (string key in Current.Resources.Keys.OfType<string>()) {
                if (key.StartsWith("strings-") &&
                    Current.Resources.TryGetResource(key, ThemeVariant.Default, out var res) &&
                    res is IResourceProvider rp) {
                    result.Add(key.Replace("strings-", ""), rp);
                }
            }
            return result;
        }

        public static void SetLanguage(string language) {
            if (Current == null) {
                return;
            }
            var languages = GetLanguages();
            foreach (var res in languages.Values) {
                Current.Resources.MergedDictionaries.Remove(res);
            }
            if (language != "en-US") {
                Current.Resources.MergedDictionaries.Add(languages["en-US"]);
            }
            if (languages.TryGetValue(language, out var res1)) {
                Current.Resources.MergedDictionaries.Add(res1);
            }
        }

        static void InitializeTheme() {
            Log.Information("Initializing theme.");
            SetTheme();
            Log.Information("Initialized theme.");
        }

        public static void SetTheme() {
            if (Current == null) {
                return;
            }
            var light = (IResourceProvider)Current.Resources["themes-light"]!;
            var dark = (IResourceProvider)Current.Resources["themes-dark"]!;
            var moon = (IResourceProvider)Current.Resources["themes-moon"]!;
            var cherry = (IResourceProvider)Current.Resources["themes-cherry"]!;
            var wine = (IResourceProvider)Current.Resources["themes-wine"]!;
            var barbie = (IResourceProvider)Current.Resources["themes-barbie"]!;
            var pearl = (IResourceProvider)Current.Resources["themes-pearl"]!;
            var purple = (IResourceProvider)Current.Resources["themes-purple"]!;
            var lilac = (IResourceProvider)Current.Resources["themes-lilac"]!;
            var ocean = (IResourceProvider)Current.Resources["themes-ocean"]!;
            var sky = (IResourceProvider)Current.Resources["themes-sky"]!;
            var teal = (IResourceProvider)Current.Resources["themes-teal"]!;
            var mint = (IResourceProvider)Current.Resources["themes-mint"]!;
            var olive = (IResourceProvider)Current.Resources["themes-olive"]!;
            var gold = (IResourceProvider)Current.Resources["themes-gold"]!;
            var cheese = (IResourceProvider)Current.Resources["themes-cheese"]!;
            var peach = (IResourceProvider)Current.Resources["themes-peach"]!;
            var silver = (IResourceProvider)Current.Resources["themes-silver"]!;
            var coconut = (IResourceProvider)Current.Resources["themes-coconut"]!;
            var lavender = (IResourceProvider)Current.Resources["themes-lavender"]!;
            var coffee = (IResourceProvider)Current.Resources["themes-coffee"]!;
            Current.Resources.MergedDictionaries.Remove(light);
            Current.Resources.MergedDictionaries.Remove(dark);
            Current.Resources.MergedDictionaries.Remove(moon);
            Current.Resources.MergedDictionaries.Remove(cherry);
            Current.Resources.MergedDictionaries.Remove(wine);
            Current.Resources.MergedDictionaries.Remove(barbie);
            Current.Resources.MergedDictionaries.Remove(pearl);
            Current.Resources.MergedDictionaries.Remove(purple);
            Current.Resources.MergedDictionaries.Remove(lilac);
            Current.Resources.MergedDictionaries.Remove(ocean);
            Current.Resources.MergedDictionaries.Remove(sky);
            Current.Resources.MergedDictionaries.Remove(teal);
            Current.Resources.MergedDictionaries.Remove(mint);
            Current.Resources.MergedDictionaries.Remove(olive);
            Current.Resources.MergedDictionaries.Remove(gold);
            Current.Resources.MergedDictionaries.Remove(cheese);
            Current.Resources.MergedDictionaries.Remove(peach);
            Current.Resources.MergedDictionaries.Remove(silver);
            Current.Resources.MergedDictionaries.Remove(coconut);
            Current.Resources.MergedDictionaries.Remove(lavender);
            Current.Resources.MergedDictionaries.Remove(coffee);
            var themeType = (ThemeType)Core.Util.Preferences.Default.Theme;
            switch (themeType) {
                case ThemeType.Light:
                    Current.Resources.MergedDictionaries.Add(light);
                    Current.RequestedThemeVariant = ThemeVariant.Light;
                    break;
                case ThemeType.Dark:
                    Current.Resources.MergedDictionaries.Add(dark);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Moon:
                    Current.Resources.MergedDictionaries.Add(moon);
                    Current.RequestedThemeVariant = ThemeVariant.Default;
                    break;
                case ThemeType.Cherry:
                    Current.Resources.MergedDictionaries.Add(cherry);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Wine:
                    Current.Resources.MergedDictionaries.Add(wine);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Barbie:
                    Current.Resources.MergedDictionaries.Add(barbie);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Pearl:
                    Current.Resources.MergedDictionaries.Add(pearl);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Purple:
                    Current.Resources.MergedDictionaries.Add(purple);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Lilac:
                    Current.Resources.MergedDictionaries.Add(lilac);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Ocean:
                    Current.Resources.MergedDictionaries.Add(ocean);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Sky:
                    Current.Resources.MergedDictionaries.Add(sky);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Teal:
                    Current.Resources.MergedDictionaries.Add(teal);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Mint:
                    Current.Resources.MergedDictionaries.Add(mint);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Olive:
                    Current.Resources.MergedDictionaries.Add(olive);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Gold:
                    Current.Resources.MergedDictionaries.Add(gold);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Cheese:
                    Current.Resources.MergedDictionaries.Add(cheese);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Peach:
                    Current.Resources.MergedDictionaries.Add(peach);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Silver:
                    Current.Resources.MergedDictionaries.Add(silver);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Coconut:
                    Current.Resources.MergedDictionaries.Add(coconut);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Lavender:
                    Current.Resources.MergedDictionaries.Add(lavender);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                case ThemeType.Coffee:
                    Current.Resources.MergedDictionaries.Add(coffee);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
                default:
                    Current.Resources.MergedDictionaries.Add(dark);
                    Current.RequestedThemeVariant = ThemeVariant.Dark;
                    break;
            }
            ThemeManager.LoadTheme();
        }
    }
}
