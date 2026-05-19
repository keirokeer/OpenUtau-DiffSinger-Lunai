using System.Linq;
using OpenUtau.App;
using OpenUtau.Core.Format;

namespace OpenUtau.App.ViewModels {
  public class ImportProjectDialogViewModel {
    public string FileName { get; }
    public bool ImportNotes { get; set; } = true;
    public bool ImportPitch { get; set; } = true;
    public bool ImportTempo { get; set; } = true;
    public bool ImportTimeSignature { get; set; } = true;
    public string DefaultLyric { get; set; } = "a";
    public bool FillShortGaps { get; set; } = true;
    public bool PartLeadingPadding { get; set; } = true;
    public bool ResolveNoteOverlaps { get; set; } = true;
    public bool ConvertChineseToPinyin { get; set; }
    public ImportGapThresholdOption[] GapThresholdOptions { get; }
    public ImportGapThresholdOption SelectedGapThreshold { get; set; }
    public ImportGapThresholdOption[] LeadingPaddingOptions { get; }
    public ImportGapThresholdOption SelectedLeadingPadding { get; set; }
    public JapaneseLyricsOption[] JapaneseLyricsOptions { get; }
    public JapaneseLyricsOption SelectedJapaneseLyrics { get; set; }
    public string LyricsMappingText { get; set; } = "";
    public string LyricsReplacementText { get; set; } = "";

    public ImportProjectDialogViewModel(string fileName) {
      FileName = fileName;
      GapThresholdOptions = CreateThresholdOptions();
      SelectedGapThreshold = GapThresholdOptions.First(o => o.Denominator == 64);
      LeadingPaddingOptions = CreateThresholdOptions();
      SelectedLeadingPadding = LeadingPaddingOptions.First(o => o.Denominator == 16);
      JapaneseLyricsOptions = CreateJapaneseOptions();
      SelectedJapaneseLyrics = JapaneseLyricsOptions[0];
    }

    static ImportGapThresholdOption[] CreateThresholdOptions() {
      int[] denominators = { 4, 8, 16, 32, 64, 128, 256 };
      string format = ThemeManager.GetString("dialogs.importproject.fillgaps.thresholditem");
      return denominators
          .Select(d => new ImportGapThresholdOption {
            Denominator = d,
            Label = string.Format(format, d),
          })
          .ToArray();
    }

    static JapaneseLyricsOption[] CreateJapaneseOptions() => new[] {
      new JapaneseLyricsOption(JapaneseLyricsConversionMode.None,
          ThemeManager.GetString("dialogs.importproject.japanese.none")),
      new JapaneseLyricsOption(JapaneseLyricsConversionMode.RomajiToHiragana,
          ThemeManager.GetString("dialogs.importproject.japanese.hiragana")),
      new JapaneseLyricsOption(JapaneseLyricsConversionMode.HiraganaToRomaji,
          ThemeManager.GetString("dialogs.importproject.japanese.romaji")),
    };

    public ProjectImportOptions ToOptions() => new() {
      ImportNotes = ImportNotes,
      ImportPitch = ImportPitch && ImportNotes,
      ImportTempo = ImportTempo,
      ImportTimeSignature = ImportTimeSignature,
      DefaultLyric = string.IsNullOrWhiteSpace(DefaultLyric) ? "a" : DefaultLyric,
      FillShortGaps = FillShortGaps && ImportNotes,
      FillShortGapsMaxLengthDenominator = SelectedGapThreshold?.Denominator ?? 64,
      PartLeadingPadding = PartLeadingPadding && ImportNotes,
      PartLeadingPaddingDenominator = SelectedLeadingPadding?.Denominator ?? 16,
      ResolveNoteOverlaps = ResolveNoteOverlaps && ImportNotes,
      JapaneseLyricsConversion = SelectedJapaneseLyrics?.Mode ?? JapaneseLyricsConversionMode.None,
      ConvertChineseToPinyin = ConvertChineseToPinyin,
      LyricsMapping = SvpLyricsProcessor.ParseMappingLines(LyricsMappingText),
      LyricsReplacement = SvpLyricsProcessor.ParseReplacementLines(LyricsReplacementText),
    };
  }

  public sealed class JapaneseLyricsOption {
    public JapaneseLyricsConversionMode Mode { get; }
    public string Label { get; }
    public JapaneseLyricsOption(JapaneseLyricsConversionMode mode, string label) {
      Mode = mode;
      Label = label;
    }
    public override string ToString() => Label;
  }
}
