using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using OpenUtau.App.Controls;
using OpenUtau.Core.Util;
using ReactiveUI;

namespace OpenUtau.App {
    class ThemeChangedEvent { }

    class ThemeManager {
        public static bool IsDarkMode = false;
        public static IBrush ForegroundBrush = Brushes.Black;
        public static IBrush BackgroundBrush = Brushes.White;
        public static IBrush NeutralAccentBrush = Brushes.Gray;
        public static IBrush NeutralAccentBrushSemi = Brushes.Gray;
        public static IPen NeutralAccentPen = new Pen(Brushes.Black);
        public static IPen NeutralAccentPenSemi = new Pen(Brushes.Black);
        public static IBrush AccentBrush1 = Brushes.White;
        public static IPen AccentPen1 = new Pen(Brushes.White);
        public static IPen AccentPen1Thickness2 = new Pen(Brushes.White);
        public static IPen AccentPen1Thickness3 = new Pen(Brushes.White);
        public static IBrush AccentBrush1Semi = Brushes.Gray;
        public static IBrush AccentBrush1Note = Brushes.White;
        public static IBrush AccentBrush1NoteSemi = Brushes.Gray;
        public static IBrush AccentBrush2 = Brushes.Gray;
        public static IPen AccentPen2 = new Pen(Brushes.White);
        public static IPen AccentPen2Thickness2 = new Pen(Brushes.White);
        public static IPen AccentPen2Thickness3 = new Pen(Brushes.White);
        public static IBrush AccentBrush2Semi = Brushes.Gray;
        public static IBrush AccentBrush3 = Brushes.Gray;
        public static IPen AccentPen3 = new Pen(Brushes.White);
        public static IPen AccentPen3Thick = new Pen(Brushes.White);
        public static IBrush AccentBrush3Semi = Brushes.Gray;
        public static IPen NoteBorderPen = new Pen(Brushes.White, 1);
        public static IPen NoteBorderPenPressed = new Pen(Brushes.White, 1);
        public static IBrush NoteEmptyBrush = Brushes.White;
        public static IBrush NoteBrush = Brushes.White;
        public static IBrush NoteBrushPressed = Brushes.Gray;
        public static IBrush TickLineBrushLow = Brushes.Black;
        public static IBrush BarNumberBrush = Brushes.Black;
        public static IPen BarNumberPen = new Pen(Brushes.White);
        public static IBrush FinalPitchBrush = Brushes.Gray;
        public static IPen FinalPitchPen = new Pen(Brushes.Gray);
        public static IPen FinalPitchPenTransparent = new Pen(Brushes.White, 1);
        public static IBrush WhiteKeyBrush = Brushes.White;
        public static IBrush WhiteKeyNameBrush = Brushes.Black;
        public static IBrush CenterKeyBrush = Brushes.White;
        public static IBrush CenterKeyNameBrush = Brushes.Black;
        public static IBrush BlackKeyBrush = Brushes.Black;
        public static IBrush BlackKeyNameBrush = Brushes.White;
        public static IBrush ExpBrush = Brushes.White;
        public static IBrush ExpNameBrush = Brushes.Black;
        public static IBrush ExpShadowBrush = Brushes.Gray;
        public static IBrush ExpShadowNameBrush = Brushes.White;
        public static IBrush ExpActiveBrush = Brushes.Black;
        public static IBrush ExpActiveNameBrush = Brushes.White;

        public static List<TrackColor> TrackColors = new List<TrackColor>(){
                new TrackColor("Flamingo", "#D491AA", "#E06C96", "#E8B0C6", "#EBCFDC", "#66AC7288", "#C2708E", "#D491AA", "#99AC7288", "#1AAC7288"), // note, note pressed, border, border pressed, note empty
                new TrackColor("Cherry", "#D93A3F", "#C02A2F", "#D9454A", "#F5B7B9", "#669E2E32", "#AF3136", "#C02A2F", "#999E2E32", "#1A9E2E32"),
                new TrackColor("Peach", "#FF8A65", "#FF7043", "#FFAB91", "#FFD5C8", "#70F5683D", "#E07352", "#FFAB91", "#99F5683D", "#1AF5683D"),
                new TrackColor("Banana", "#FBC13A", "#FBAB32", "#FDD13F", "#FFF4C0", "#70FAC038", "#DFAD49", "#FFD97F", "#99FAC038", "#1AFAC038"),
                new TrackColor("Olive", "#CDDC39", "#B0B931", "#DCE775", "#F2F7CE", "#70CDD926", "#99A12B", "#E8F764", "#99CDD926", "#1ACDD926"),
                new TrackColor("Mint", "#66BB8A", "#43A06A", "#A5D6BA", "#D2EBDD", "#7033CC73", "#45A16B", "#4DCB82", "#9933CC73", "#1A33CC73"),
                new TrackColor("Sky", "#80D9FF", "#3DC7F5", "#9EE3FA", "#C4EFFD", "#5980D9FF", "#4D99B3", "#9EE3FA", "#9980D9FF", "#1A80D9FF"),
                new TrackColor("Blue", "#7266EE", "#4435E6", "#B1ABFB", "#E4E2FD", "#704C4C7A", "#50509B", "#7B79D9", "#994C4C7A", "#1A4C4C7A"),
                new TrackColor("Purple", "#BA68C8", "#AB47BC", "#CE93D8", "#E7C9EC", "#70BA68C8", "#AB47BC", "#CE93D8", "#99BA68C8", "#1ABA68C8"),
                new TrackColor("Barbie", "#E91E63", "#C2185B", "#F06292", "#F8B1C9", "#70DB5781", "#DA3E7A", "#F28CAD", "#99DB5781", "#1ADB5781"),
                new TrackColor("Wine", "#AE3442", "#96212F", "#D25664", "#FAA8B1", "#666A252E", "#96212F", "#AE3442", "#996A252E", "#1A6A252E"),
                new TrackColor("Orange", "#EE582B", "#C33C13", "#F06B42", "#FFC2B3", "#70E65427", "#C33C13", "#F06B42", "#99E65427", "#1AE65427"),
                new TrackColor("Gold", "#FF8F00", "#FF7F00", "#FFB300", "#FFE097", "#70EC9A2F", "#C07326", "#FFAF4D", "#99EC9A2F", "#1AEC9A2F"),
                new TrackColor("BRAT", "#C5E233", "#BAE61A", "#DBF075", "#F3FAD1", "#70BAE61A", "#8BAA0E", "#E1FF4D", "#99BAE61A", "#1AC4E61A"),
                new TrackColor("Forest", "#2E7D32", "#1B5E20", "#43A047", "#A1D0A3", "#701B5E20", "#2E7D32", "#43A047", "#991B5E20", "#1A1B5E20"),
                new TrackColor("Teal", "#0AC2C2", "#008080", "#2196F3", "#90CBF9", "#70008080", "#238B8B", "#14B8B8", "#99008080", "#1A008080"),
                new TrackColor("Violet", "#7B1FA2", "#4A148C", "#AB47BC", "#D5A3DE", "#70CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE", "#1AAF6B85"),
                new TrackColor("Moon", "#707070", "#4A4A4A", "#909090", "#C9C9C9", "#704A4A4A", "#707070", "#909090", "#99C9C9C9", "#1AAF6B85"),
            };

        public static void LoadTheme() {
            if (Application.Current == null) {
                return;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            object? outVar;
            IsDarkMode = false;
            var themeVariant = ThemeVariant.Default;
            if (resDict.TryGetResource("IsDarkMode", themeVariant, out outVar)) {
                if (outVar is bool b) {
                    IsDarkMode = b;
                }
            }
            if (resDict.TryGetResource("SystemControlForegroundBaseHighBrush", themeVariant, out outVar)) {
                ForegroundBrush = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("SystemControlBackgroundAltHighBrush", themeVariant, out outVar)) {
                BackgroundBrush = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("NeutralAccentBrush", themeVariant, out outVar)) {
                NeutralAccentBrush = (IBrush)outVar!;
                NeutralAccentPen = new Pen(NeutralAccentBrush, 1);
            }
            if (resDict.TryGetResource("NeutralAccentBrushSemi", themeVariant, out outVar)) {
                NeutralAccentBrushSemi = (IBrush)outVar!;
                NeutralAccentPenSemi = new Pen(NeutralAccentBrushSemi, 1);
            }
            if (resDict.TryGetResource("AccentBrush1", themeVariant, out outVar)) {
                AccentBrush1 = (IBrush)outVar!;
                AccentPen1 = new Pen(AccentBrush1);
                AccentPen1Thickness2 = new Pen(AccentBrush1, 2);
                AccentPen1Thickness3 = new Pen(AccentBrush1, 3);
            }
            if (resDict.TryGetResource("AccentBrush1Semi", themeVariant, out outVar)) {
                AccentBrush1Semi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("AccentBrush1Note", themeVariant, out outVar)) {
                AccentBrush1Note = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("AccentBrush1NoteSemi", themeVariant, out outVar)) {
                AccentBrush1NoteSemi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("AccentBrush2", themeVariant, out outVar)) {
                AccentBrush2 = (IBrush)outVar!;
                AccentPen2 = new Pen(AccentBrush2, 1);
                AccentPen2Thickness2 = new Pen(AccentBrush2, 2);
                AccentPen2Thickness3 = new Pen(AccentBrush2, 3);
            }
            if (resDict.TryGetResource("AccentBrush2Semi", themeVariant, out outVar)) {
                AccentBrush2Semi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("AccentBrush3", themeVariant, out outVar)) {
                AccentBrush3 = (IBrush)outVar!;
                AccentPen3 = new Pen(AccentBrush3, 1);
                AccentPen3Thick = new Pen(AccentBrush3, 3);
            }
            if (resDict.TryGetResource("AccentBrush3Semi", themeVariant, out outVar)) {
                AccentBrush3Semi = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("NoteBorderBrush", themeVariant, out outVar)) {
                NoteBorderPen = new Pen((IBrush)outVar!, 1);
            }
            if (resDict.TryGetResource("NoteBorderBrushPressed", themeVariant, out outVar)) {
                NoteBorderPenPressed = new Pen((IBrush)outVar!, 1);
            }
            if (resDict.TryGetResource("FinalPitchBrushTransparent", themeVariant, out outVar)) {
                FinalPitchPenTransparent = new Pen((IBrush)outVar!, 1);
            }
            if (resDict.TryGetResource("TickLineBrushLow", themeVariant, out outVar)) {
                TickLineBrushLow = (IBrush)outVar!;
            }
            if (resDict.TryGetResource("BarNumberBrush", themeVariant, out outVar)) {
                BarNumberBrush = (IBrush)outVar!;
                BarNumberPen = new Pen(BarNumberBrush, 1);
            }
            if (resDict.TryGetResource("FinalPitchBrush", themeVariant, out outVar)) {
                FinalPitchBrush = (IBrush)outVar!;
                FinalPitchPen = new Pen(FinalPitchBrush, 1);
            }
            SetKeyboardBrush();
            TextLayoutCache.Clear();
            MessageBus.Current.SendMessage(new ThemeChangedEvent());
        }

        public static void ChangePianorollColor(string color) {
            if (Application.Current == null) {
                return;
            }
            try {
                IResourceDictionary resDict = Application.Current.Resources;
                TrackColor tcolor = GetTrackColor(color);
                
                resDict["SelectedTrackAccentBrush"] = tcolor.AccentColor;
                resDict["SelectedTrackAccentLightBrush"] = tcolor.AccentColorLight;
                resDict["SelectedTrackAccentLightBrushSemi"] = tcolor.AccentColorLightSemi;
                resDict["SelectedTrackAccentDarkBrush"] = tcolor.AccentColorDark;
                resDict["SelectedTrackCenterKeyBrush"] = tcolor.AccentColorCenterKey;

                NoteBrush = tcolor.NoteColor;
                NoteBorderPen = new Pen(tcolor.NoteBorderColor);
                NoteBrushPressed = tcolor.NoteColorPressed;
                NoteBorderPenPressed = new Pen(tcolor.NoteBorderColorPressed);
                NoteEmptyBrush = tcolor.NoteColorEmpty;

                SetKeyboardBrush();
            } catch { }
            MessageBus.Current.SendMessage(new ThemeChangedEvent());
        }
        private static void SetKeyboardBrush() {
            if (Application.Current == null) {
                return;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            object? outVar;
            var themeVariant = ThemeVariant.Default;

            if (Preferences.Default.UseTrackColor) {
                if (IsDarkMode) {
                    if (resDict.TryGetResource("SelectedTrackAccentBrush", themeVariant, out outVar)) {
                        CenterKeyNameBrush = (IBrush)outVar!;
                        WhiteKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("SelectedTrackCenterKeyBrush", themeVariant, out outVar)) {
                        CenterKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("WhiteKeyNameBrush", themeVariant, out outVar)) {
                        WhiteKeyNameBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("BlackKeyBrush", themeVariant, out outVar)) {
                        BlackKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("BlackKeyNameBrush", themeVariant, out outVar)) {
                        BlackKeyNameBrush = (IBrush)outVar!;
                    }
                    ExpBrush = BlackKeyBrush;
                    ExpNameBrush = BlackKeyNameBrush;
                    ExpActiveBrush = WhiteKeyBrush;
                    ExpActiveNameBrush = WhiteKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                } else { // LightMode
                    if (resDict.TryGetResource("SelectedTrackAccentBrush", themeVariant, out outVar)) {
                        CenterKeyNameBrush = (IBrush)outVar!;
                        WhiteKeyNameBrush = (IBrush)outVar!;
                        BlackKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("SelectedTrackCenterKeyBrush", themeVariant, out outVar)) {
                        CenterKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("WhiteKeyBrush", themeVariant, out outVar)) {
                        WhiteKeyBrush = (IBrush)outVar!;
                    }
                    if (resDict.TryGetResource("BlackKeyNameBrush", themeVariant, out outVar)) {
                        BlackKeyNameBrush = (IBrush)outVar!;
                    }
                    ExpBrush = WhiteKeyBrush;
                    ExpNameBrush = WhiteKeyNameBrush;
                    ExpActiveBrush = BlackKeyBrush;
                    ExpActiveNameBrush = BlackKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                }
            } else { // DefColor
                if (resDict.TryGetResource("WhiteKeyBrush", themeVariant, out outVar)) {
                    WhiteKeyBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("WhiteKeyNameBrush", themeVariant, out outVar)) {
                    WhiteKeyNameBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("CenterKeyBrush", themeVariant, out outVar)) {
                    CenterKeyBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("CenterKeyNameBrush", themeVariant, out outVar)) {
                    CenterKeyNameBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("BlackKeyBrush", themeVariant, out outVar)) {
                    BlackKeyBrush = (IBrush)outVar!;
                }
                if (resDict.TryGetResource("BlackKeyNameBrush", themeVariant, out outVar)) {
                    BlackKeyNameBrush = (IBrush)outVar!;
                }
                if (!IsDarkMode) {
                    ExpBrush = WhiteKeyBrush;
                    ExpNameBrush = WhiteKeyNameBrush;
                    ExpActiveBrush = BlackKeyBrush;
                    ExpActiveNameBrush = BlackKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                } else {
                    ExpBrush = BlackKeyBrush;
                    ExpNameBrush = BlackKeyNameBrush;
                    ExpActiveBrush = WhiteKeyBrush;
                    ExpActiveNameBrush = WhiteKeyNameBrush;
                    ExpShadowBrush = CenterKeyBrush;
                    ExpShadowNameBrush = CenterKeyNameBrush;
                }
            }
        }

        public static string GetString(string key) {
            if (Application.Current == null) {
                return key;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            if (resDict.TryGetResource(key, ThemeVariant.Default, out var outVar) && outVar is string s) {
                return s;
            }
            return key;
        }

        public static bool TryGetString(string key, out string value) {
            if (Application.Current == null) {
                value = key;
                return false;
            }
            IResourceDictionary resDict = Application.Current.Resources;
            if (resDict.TryGetResource(key, ThemeVariant.Default, out var outVar) && outVar is string s) {
                value = s;
                return true;
            }
            value = key;
            return false;
        }

        public static TrackColor GetTrackColor(string name) {
            if (TrackColors.Any(c => c.Name == name)) {
                return TrackColors.First(c => c.Name == name);
            }
            return TrackColors.First(c => c.Name == "Blue");
        }
    }

    public class TrackColor {
        public string Name { get; set; } = "";
        public SolidColorBrush AccentColor { get; set; }
        public SolidColorBrush AccentColorDark { get; set; } // Pressed
        public SolidColorBrush AccentColorLight { get; set; } // PointerOver
        public SolidColorBrush AccentColorLightSemi { get; set; } // BackGround
        public SolidColorBrush AccentColorCenterKey { get; set; } // Keyboard
        public SolidColorBrush NoteColor { get; set; }
        public SolidColorBrush NoteColorPressed { get; set; }
        public SolidColorBrush NoteBorderColor { get; set; }
        public SolidColorBrush NoteBorderColorPressed { get; set; }
        public SolidColorBrush NoteColorEmpty { get; set; }

        public TrackColor(string name, string accentColor, string darkColor, string lightColor, string centerKey, string noteColor, string noteColorPressed, string noteBorderColor, string noteBorderColorPressed, string noteColorEmpty) {
            Name = name;
            AccentColor = SolidColorBrush.Parse(accentColor);
            AccentColorDark = SolidColorBrush.Parse(darkColor);
            AccentColorLight = SolidColorBrush.Parse(lightColor);
            AccentColorLightSemi = SolidColorBrush.Parse(lightColor);
            AccentColorLightSemi.Opacity = 0.5;
            AccentColorCenterKey = SolidColorBrush.Parse(centerKey);
            NoteColor = SolidColorBrush.Parse(noteColor);
            NoteColorPressed = SolidColorBrush.Parse(noteColorPressed);
            NoteBorderColor = SolidColorBrush.Parse(noteBorderColor);
            NoteBorderColorPressed = SolidColorBrush.Parse(noteBorderColorPressed);
            NoteColorEmpty = SolidColorBrush.Parse(noteColorEmpty);
        }
    }
}
