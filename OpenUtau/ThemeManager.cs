﻿using System.Collections.Generic;
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
                new TrackColor("Flamingo", "#DC799D", "#DA5789", "#E19EBA", "#EBCFDC", "#80AF6B85", "#C2708E", "#D194AA", "#99AF6B85"), // note, note pressed, border, border pressed
                new TrackColor("Cherry", "#F3383E", "#E61E25", "#E6454A", "#F3AAAD", "#809E2E32", "#B93136", "#B93136", "#999E2E32"),
                new TrackColor("Peach", "#FF8A65", "#FF7043", "#FFAB91", "#FFD5C8", "#80F5683D", "#E07352", "#FFAB91", "#99F5683D"),
                new TrackColor("Banana", "#FBC13A", "#FBAB32", "#FDD13F", "#FFF4C0", "#80FAC038", "#E6B34D", "#FFD97F", "#99FAC038"),
                new TrackColor("Light Green", "#CDDC39", "#C0CA33", "#DCE775", "#F2F7CE", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Green", "#66BB6A", "#43A047", "#A5D6A7", "#D2EBD3", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Light Blue", "#4FC3F7", "#29B6F6", "#81D4FA", "#C0EAFD", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Blue", "#7266EE", "#4435E6", "#B1ABFB", "#E4E2FD", "#804A4A4A", "#707070", "#909090", "#99C9C9C9"),
                new TrackColor("Purple", "#BA68C8", "#AB47BC", "#CE93D8", "#E7C9EC", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Barbie", "#E91E63", "#C2185B", "#F06292", "#F8B1C9", "#80DB5781", "#DA3E7A", "#F28CAD", "#99F6ACC3"),
                new TrackColor("Wine", "#D42F3D", "#B81C29", "#F14F5D", "#F8A9AF", "#8071272E", "#9F232D", "#AE343E", "#99C33A46"),
                new TrackColor("Orange", "#FF5722", "#E64A19", "#FF7043", "#FFB8A1", "#80E65427", "#C33C13", "#E66741", "#99E1633D"),
                new TrackColor("Gold", "#FF8F00", "#FF7F00", "#FFB300", "#FFE097", "#80EC9A2F", "#C07326", "#FEB557", "#99EA9B4D"),
                new TrackColor("Light Green2", "#AFB42B", "#9E9D24", "#CDDC39", "#E6EE9C", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Green2", "#2E7D32", "#1B5E20", "#43A047", "#A1D0A3", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Light Blue2", "#1976D2", "#0D47A1", "#2196F3", "#90CBF9", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
                new TrackColor("Blue2", "#7266EE", "#4435E6", "#B1ABFB", "#E4E2FD", "#804C4C7A", "#7B79D9", "#7271C9", "#99ADACFC"),
                new TrackColor("Purple2", "#7B1FA2", "#4A148C", "#AB47BC", "#D5A3DE", "#80CDDC39", "#C0CA33", "#DCE775", "#99F2F7CE"),
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

        public TrackColor(string name, string accentColor, string darkColor, string lightColor, string centerKey, string noteColor, string noteColorPressed, string noteBorderColor, string noteBorderColorPressed) {
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
        }
    }
}
