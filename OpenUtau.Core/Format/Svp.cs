using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenUtau.Core.Ustx;
using Serilog;

namespace OpenUtau.Core.Format {
  /// <summary>
  /// Imports Synthesizer V (.svp) projects into OpenUtau.
  /// Conversion logic adapted from utaformatix3 by Colin SDE (sdercolin):
  /// https://github.com/sdercolin/utaformatix3
  /// </summary>
  public static class Svp {
    static readonly JsonSerializerSettings JsonSettings = new() {
      MissingMemberHandling = MissingMemberHandling.Ignore,
      NullValueHandling = NullValueHandling.Ignore,
      FloatParseHandling = FloatParseHandling.Double,
    };

    public static UProject Load(string file, ProjectImportOptions options) {
      options ??= ProjectImportOptions.CreateDefault();
      var svp = ReadSvpFile(file);
      var project = new UProject();
      Ustx.AddDefaultExpressions(project);
      project.FilePath = file;
      project.name = Path.GetFileNameWithoutExtension(file);

      if (options.ImportTimeSignature && svp.time?.meter is { Count: > 0 } meters) {
        project.timeSignatures = meters
            .Select(m => new UTimeSignature(m.index, m.numerator, m.denominator))
            .ToList();
      } else {
        project.timeSignatures = new List<UTimeSignature> { new(0, 4, 4) };
      }

      if (options.ImportTempo && svp.time?.tempo is { Count: > 0 } tempos) {
        project.tempos = tempos
            .Select(t => new UTempo((int)SvpPitchProcessor.BlickToTick((long)Math.Round(t.position)), t.bpm))
            .ToList();
      } else {
        project.tempos = new List<UTempo> { new(0, 120) };
      }

      var tempoList = project.tempos
          .Select(t => ((long)t.position, t.bpm))
          .ToList();

      if (!options.ImportNotes) {
        project.ValidateFull();
        return project;
      }

      var library = svp.library ?? new List<SvpGroupJson>();
      var tracks = (svp.tracks ?? new List<SvpTrackJson>())
          .OrderBy(t => t.dispOrder)
          .ToList();

      int trackIndex = 0;
      foreach (var svpTrack in tracks) {
        var part = ParseTrack(project, svpTrack, library, options, tempoList, trackIndex);
        if (part == null) {
          continue;
        }
        var track = new UTrack(project) { TrackName = part.name };
        track.TrackNo = project.tracks.Count;
        part.trackNo = track.TrackNo;
        part.AfterLoad(project, track);
        project.tracks.Add(track);
        project.parts.Add(part);
        trackIndex++;
      }

      project.ValidateFull();
      Log.Information("Loaded SVP project {File} with {Tracks} tracks", file, project.tracks.Count);
      return project;
    }

    static SvpFileJson ReadSvpFile(string file) {
      var text = ReadSvpText(file);
      if (string.IsNullOrWhiteSpace(text)) {
        throw new FileFormatException("Empty SVP file");
      }
      var chunks = ExtractJsonChunks(text);
      if (chunks.Count == 0) {
        throw new FileFormatException("No JSON content in SVP file");
      }
      var projects = new List<SvpFileJson>();
      foreach (var chunk in chunks) {
        try {
          var doc = JsonConvert.DeserializeObject<SvpFileJson>(chunk, JsonSettings);
          if (doc != null && HasProjectContent(doc)) {
            projects.Add(doc);
          }
        } catch (JsonException ex) {
          Log.Warning(ex, "Skipping invalid SVP JSON chunk in {File}", file);
        }
      }
      if (projects.Count == 0) {
        throw new FileFormatException("No valid JSON documents in SVP file");
      }
      return projects.OrderByDescending(GetVersionScore).First();
    }

    static string ReadSvpText(string file) {
      var bytes = File.ReadAllBytes(file);
      if (bytes.Length == 0) {
        return string.Empty;
      }
      if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) {
        return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2).TrimStart('\uFEFF');
      }
      if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) {
        return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2).TrimStart('\uFEFF');
      }
      var utf8 = Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF');
      if (LooksLikeSvpJson(utf8)) {
        return utf8;
      }
      var utf16 = Encoding.Unicode.GetString(bytes).TrimStart('\uFEFF');
      if (LooksLikeSvpJson(utf16)) {
        return utf16;
      }
      return utf8;
    }

    static bool LooksLikeSvpJson(string text) {
      var trimmed = text.TrimStart();
      return trimmed.StartsWith('{') || trimmed.Contains("\"version\"");
    }

    static List<string> ExtractJsonChunks(string text) {
      var nullChunks = text
          .Split('\0')
          .Select(c => c.Trim('\0', ' ', '\r', '\n', '\t'))
          .Where(c => c.Length > 0 && c.TrimStart().StartsWith('{'))
          .ToList();
      if (nullChunks.Count > 1) {
        return nullChunks;
      }
      var trimmed = text.Trim('\0', ' ', '\r', '\n', '\t');
      if (trimmed.StartsWith('{')) {
        return new List<string> { trimmed };
      }
      return nullChunks;
    }

    static bool HasProjectContent(SvpFileJson doc) =>
        (doc.tracks != null && doc.tracks.Count > 0)
        || (doc.library != null && doc.library.Count > 0)
        || doc.time != null;

    static int GetVersionScore(SvpFileJson doc) {
      var token = doc.version;
      if (token == null) {
        return 0;
      }
      if (token.Type == JTokenType.Integer) {
        return token.Value<int>();
      }
      if (token.Type == JTokenType.Float) {
        return (int)token.Value<double>();
      }
      if (int.TryParse(token.ToString(), out int parsed)) {
        return parsed;
      }
      return 0;
    }

    static UVoicePart? ParseTrack(
        UProject project,
        SvpTrackJson svpTrack,
        List<SvpGroupJson> library,
        ProjectImportOptions options,
        List<(long tick, double bpm)> tempos,
        int trackIndex) {
      var notes = new List<ImportedNote>();
      var pitchPoints = new List<SvpPitchProcessor.PitchPoint>();

      long anchorBlick = (long)Math.Round(svpTrack.mainRef?.blickOffset ?? 0);

      void AddGroup(SvpGroupJson? group, SvpRefJson? reference) {
        if (group == null || reference == null) {
          return;
        }
        notes.AddRange(ParseNotes(group, reference, options, anchorBlick));
        if (options.ImportPitch) {
          long blickOffset = (long)Math.Round(reference.blickOffset);
          var parameters = ResolveParameters(group, library, svpTrack.parameters);
          pitchPoints.AddRange(SvpPitchProcessor.ParseCurvePoints(parameters?.pitchDelta, blickOffset));
          pitchPoints.AddRange(SvpPitchProcessor.ParseCurvePoints(reference.systemPitchDelta, blickOffset));
        }
      }

      AddGroup(svpTrack.mainGroup, svpTrack.mainRef);
      if (svpTrack.groups != null) {
        foreach (var reference in svpTrack.groups) {
          var group = library.FirstOrDefault(g => g.uuid == reference.groupID);
          AddGroup(group, reference);
        }
      }

      notes = notes.Where(n => n.TickOn >= 0).OrderBy(n => n.TickOn).ToList();
      if (notes.Count == 0) {
        return null;
      }
      if (options.FillShortGaps) {
        SvpNoteGapFiller.FillShortGaps(
            notes,
            options.FillShortGapsMaxLengthDenominator,
            n => n.TickOn,
            n => n.TickOff,
            (n, tickOff) => n.TickOff = tickOff);
      }
      if (options.ResolveNoteOverlaps) {
        SvpNoteOverlapResolver.Resolve(
            notes,
            n => n.TickOn,
            n => n.TickOff,
            (n, tickOff) => n.TickOff = tickOff,
            (n, tickOn) => n.TickOn = tickOn);
      }

      int partPosition = (int)SvpPitchProcessor.BlickToTick(anchorBlick);
      int minTick = (int)notes.Min(n => n.TickOn);
      if (minTick < 0) {
        partPosition += minTick;
        ShiftImportedNotes(notes, -minTick);
      }
      if (options.PartLeadingPadding && notes.Min(n => n.TickOn) == 0) {
        int pad = SvpNoteGapFiller.TicksPerFullNote / options.PartLeadingPaddingDenominator;
        ShiftImportedNotes(notes, pad);
      }

      int partEnd = notes.Max(n => n.TickOff);
      var part = new UVoicePart {
        name = svpTrack.name ?? $"Track {trackIndex + 1}",
        position = partPosition,
        Duration = partEnd,
      };

      foreach (var n in notes) {
        var note = project.CreateNote(n.Tone, n.TickOn, n.TickOff - n.TickOn);
        note.lyric = n.Lyric;
        part.notes.Add(note);
      }

      if (options.ImportPitch) {
        var mainParams = svpTrack.mainGroup != null
            ? ResolveParameters(svpTrack.mainGroup, library, svpTrack.parameters)
            : svpTrack.parameters;
        long mainOffset = (long)Math.Round(svpTrack.mainRef?.blickOffset ?? 0);
        var vibratoEnv = SvpPitchProcessor.ParseCurvePoints(mainParams?.vibratoEnv, mainOffset);
        var processed = SvpPitchProcessor.ProcessInput(
            pitchPoints,
            mainParams?.pitchDelta?.mode,
            notes.Select(n => n.Vibrato).ToList(),
            tempos,
            vibratoEnv,
            mainParams?.vibratoEnv?.mode,
            BuildDefaultVibrato(svpTrack.mainRef));
        if (processed.Count == 0) {
          Log.Warning("SVP pitch import produced no points for track {Track}", svpTrack.name);
        } else {
          ApplyPitd(project, part, processed, partPosition);
        }
      }

      return part;
    }

    static List<ImportedNote> ParseNotes(
        SvpGroupJson group, SvpRefJson reference, ProjectImportOptions options, long anchorBlick) {
      var list = new List<ImportedNote>();
      foreach (var svpNote in group.notes ?? Enumerable.Empty<SvpNoteJson>()) {
        long startBlick = (long)Math.Round(svpNote.onset + reference.blickOffset);
        long endBlick = startBlick + (long)Math.Round(svpNote.duration);
        int tickOn = (int)SvpPitchProcessor.BlickToTick(startBlick - anchorBlick);
        int tickOff = (int)SvpPitchProcessor.BlickToTick(endBlick - anchorBlick);
        string lyric = SvpLyricsProcessor.Process(
            NormalizeLyric(svpNote.lyrics, options.DefaultLyric),
            options);
        long length = tickOff - tickOn;
        list.Add(new ImportedNote {
          TickOn = tickOn,
          TickOff = tickOff,
          Tone = svpNote.pitch + reference.pitchOffset,
          Lyric = lyric,
          Vibrato = BuildNoteVibrato(svpNote, tickOn, length),
        });
      }
      return list;
    }

    static SvpPitchProcessor.NoteVibrato BuildNoteVibrato(SvpNoteJson note, long tickOn, long length) {
      double? Attr(string key) =>
          note.attributes != null && note.attributes.TryGetValue(key, out var v) ? v : null;
      return new SvpPitchProcessor.NoteVibrato {
        NoteStartTick = tickOn,
        NoteLengthTick = length,
        VibratoStartSec = Attr("tF0VbrStart"),
        EaseInSec = Attr("tF0VbrLeft"),
        EaseOutSec = Attr("tF0VbrRight"),
        DepthSemitone = Attr("dF0Vbr"),
        PhaseRad = Attr("pF0Vbr"),
        FrequencyHz = Attr("fF0Vbr"),
      };
    }

    static SvpPitchProcessor.DefaultVibrato? BuildDefaultVibrato(SvpRefJson? reference) {
      var voice = reference?.voice;
      if (voice?.vocalModeParams == null) {
        return null;
      }
      double? Param(string key) =>
          voice.vocalModeParams.TryGetValue(key, out var v) ? v : null;
      return new SvpPitchProcessor.DefaultVibrato {
        VibratoStartSec = Param("tF0VbrStart"),
        EaseInSec = Param("tF0VbrLeft"),
        EaseOutSec = Param("tF0VbrRight"),
        DepthSemitone = Param("dF0Vbr"),
        FrequencyHz = Param("fF0Vbr"),
      };
    }

    static void ShiftImportedNotes(List<ImportedNote> notes, int delta) {
      if (delta == 0) {
        return;
      }
      foreach (var n in notes) {
        n.TickOn += delta;
        n.TickOff += delta;
        var v = n.Vibrato;
        n.Vibrato = new SvpPitchProcessor.NoteVibrato {
          NoteStartTick = v.NoteStartTick + delta,
          NoteLengthTick = v.NoteLengthTick,
          VibratoStartSec = v.VibratoStartSec,
          EaseInSec = v.EaseInSec,
          EaseOutSec = v.EaseOutSec,
          DepthSemitone = v.DepthSemitone,
          PhaseRad = v.PhaseRad,
          FrequencyHz = v.FrequencyHz,
        };
      }
    }

    static string NormalizeLyric(string? lyrics, string defaultLyric) {
      string lyric = string.IsNullOrWhiteSpace(lyrics) ? defaultLyric : lyrics;
      if (lyric == "-") {
        return "+~";
      }
      if (lyric.StartsWith('.')) {
        return $"[{lyric[1..]}]";
      }
      return lyric;
    }

    static SvpParametersJson? ResolveParameters(
        SvpGroupJson group, List<SvpGroupJson> library, SvpParametersJson? trackParameters = null) {
      if (group.parameters != null) {
        return group.parameters;
      }
      if (!string.IsNullOrEmpty(group.uuid)) {
        var fromLibrary = library.FirstOrDefault(g => g.uuid == group.uuid)?.parameters;
        if (fromLibrary != null) {
          return fromLibrary;
        }
      }
      return trackParameters;
    }

    static void ApplyPitd(
        UProject project,
        UVoicePart part,
        List<SvpPitchProcessor.PitchPoint> points,
        int partStart) {
      var pitdPoints = SvpPitchProcessor.ToPitdPoints(points);
      if (pitdPoints.Count == 0) {
        return;
      }
      if (!GetOrCreateCurve(project, part, Ustx.PITD, out var curve) || curve == null) {
        return;
      }
      int lastRelTick = 0;
      int lastCents = 0;
      foreach (var (tick, cents) in pitdPoints.OrderBy(p => p.tick)) {
        int relTick = tick - partStart;
        if (relTick < 0) {
          continue;
        }
        curve.Set(relTick, cents, relTick, 0);
        lastRelTick = relTick;
        lastCents = cents;
      }
      curve.Set(Math.Max(lastRelTick, part.Duration), lastCents, lastRelTick, 0);
    }

    static bool GetOrCreateCurve(UProject project, UVoicePart part, string abbr, out UCurve? curve) {
      curve = part.curves.Find(c => c.abbr == abbr);
      if (curve != null) {
        return true;
      }
      if (project.expressions.TryGetValue(abbr, out var desc)) {
        curve = new UCurve(desc);
        part.curves.Add(curve);
        return true;
      }
      curve = null;
      return false;
    }

    sealed class ImportedNote {
      public int TickOn { get; set; }
      public int TickOff { get; set; }
      public int Tone { get; init; }
      public string Lyric { get; init; } = "";
      public SvpPitchProcessor.NoteVibrato Vibrato { get; set; } = new();
    }
  }
}
