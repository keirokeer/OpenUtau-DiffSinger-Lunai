using System;
using Avalonia;
using Avalonia.Media.TextFormatting;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;
using OpenUtau.Core.DiffSinger;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;

namespace OpenUtau.App.Controls {
    static class PhonemeUIRender {

        public static string getLangCode(UVoicePart part){
            int trackNo = part.trackNo;
            var track = DocManager.Inst.Project.tracks[trackNo];
            string langCode = "";
            if (track.Phonemizer is DiffSingerG2pPhonemizer g2pPhonemizer) {
                langCode = g2pPhonemizer.GetLangCode();
            } else if (track.Phonemizer is DiffSingerBasePhonemizer basePhonemizer) {
                langCode = basePhonemizer.GetLangCode();
            }
            return langCode;
        }

        /// <summary>
        /// Splits phoneme text into language tag (e.g. "ja") and phoneme without tag (e.g. "a").
        /// Same logic as "hide language prefix" preference: prefix is langCode + "/".
        /// Fallback: if langCode is empty but phoneme contains "/", split on first "/".
        /// </summary>
        public static (string tagText, string phonemeOnly) SplitTagAndPhoneme(string phonemeText, string? langCode) {
            if (string.IsNullOrEmpty(phonemeText)) {
                return ("", "");
            }
            // Match langCode + "/" prefix (case-insensitive for robustness)
            if (!string.IsNullOrEmpty(langCode)) {
                var prefix = langCode + "/";
                if (phonemeText.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) {
                    return (langCode, phonemeText.Substring(prefix.Length));
                }
            }
            // Fallback: split on first "/" (e.g. "ja/o" -> "ja", "o")
            var slashIdx = phonemeText.IndexOf('/');
            if (slashIdx > 0 && slashIdx < phonemeText.Length - 1) {
                return (phonemeText.Substring(0, slashIdx), phonemeText.Substring(slashIdx + 1));
            }
            return ("", phonemeText);
        }
        //Calculates the position of a phoneme alias on a piano roll view, 
        //considering factors like tick width, phoneme text, and text layout. 
        //It returns the x-coordinate and text y-coordinate of the alias
        public static (double textX, double textY, Size size, TextLayout textLayout) 
            AliasPosition(NotesViewModel viewModel, UPhoneme phoneme, string? langCode, ref double lastTextEndX, ref bool raiseText){

            string phonemeText = !string.IsNullOrEmpty(phoneme.phonemeMapped) ? phoneme.phonemeMapped : phoneme.phoneme;
            if (Preferences.Default.DiffSingerLangCodeHide && !string.IsNullOrEmpty(langCode) && phonemeText.StartsWith(langCode+"/")) {
                phonemeText = phonemeText.Substring(langCode.Length + 1);
            }
            var x = viewModel.TickToneToPoint(phoneme.position, 0).X;
            var bold = phoneme.phoneme != phoneme.rawPhoneme;
            var textLayout = TextLayoutCache.Get(phonemeText, ThemeManager.ForegroundBrush!, 12, bold);
            if (x < lastTextEndX) {
                raiseText = !raiseText;
            } else {
                raiseText = false;
            }
            double textY = raiseText ? 2 : 18;
            var size = new Size(textLayout.Width + 4, textLayout.Height - 2);
            //var rect = new Rect(new Point(x - 2, textY + 1.5), size);
            /*if (rect.Contains(mousePos)) {
                result.phoneme = phoneme;
                result.hit = true;
                return result;
            }*/
            lastTextEndX = x + size.Width;
            return (x, textY, size, textLayout);
        }
    }
}
