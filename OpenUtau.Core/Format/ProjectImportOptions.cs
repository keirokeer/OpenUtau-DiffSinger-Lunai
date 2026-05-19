using System.Collections.Generic;



namespace OpenUtau.Core.Format {

  /// <summary>User-selected options when importing a foreign project format (e.g. SynthV .svp).</summary>

  public sealed class ProjectImportOptions {

    public bool ImportNotes { get; set; } = true;

    public bool ImportPitch { get; set; } = true;

    public bool ImportTempo { get; set; } = true;

    public bool ImportTimeSignature { get; set; } = true;

    public string DefaultLyric { get; set; } = "a";

    /// <summary>Extend notes to close gaps shorter than 1/<see cref="FillShortGapsMaxLengthDenominator"/> of a whole note.</summary>

    public bool FillShortGaps { get; set; } = true;

    /// <summary>Exclusive max gap length as 1/denominator of a whole note (UtaFormatix default: 64).</summary>

    public int FillShortGapsMaxLengthDenominator { get; set; } = 64;

    /// <summary>Add space before the first note when the group starts with a note at its beginning (1/denominator of a whole note).</summary>

    public bool PartLeadingPadding { get; set; } = true;

    public int PartLeadingPaddingDenominator { get; set; } = 16;

    /// <summary>Trim note ends so imported notes do not overlap.</summary>

    public bool ResolveNoteOverlaps { get; set; } = true;



    public JapaneseLyricsConversionMode JapaneseLyricsConversion { get; set; } = JapaneseLyricsConversionMode.None;

    public bool ConvertChineseToPinyin { get; set; }

    public IReadOnlyList<LyricsMappingEntry>? LyricsMapping { get; set; }

    public IReadOnlyList<LyricsReplacementRule>? LyricsReplacement { get; set; }



    public static ProjectImportOptions CreateDefault() => new();

  }

}


