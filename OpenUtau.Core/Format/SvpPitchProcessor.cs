using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace OpenUtau.Core.Format {
  /// <summary>
  /// Converts SynthV pitch curves (pitchDelta + vibrato) into OpenUtau PITD points (cents).
  /// Algorithm follows UtaFormatix3 core/process/pitch/SynthVPitchConversion.kt.
  /// </summary>
  internal static class SvpPitchProcessor {
    internal const long BlicksPerTick = 1470000L;
    internal const int TicksPerBeat = 480;
    internal const long SamplingIntervalTick = 4L;

    const double VibratoDefaultStartSec = 0.25;
    const double VibratoDefaultEaseInSec = 0.2;
    const double VibratoDefaultEaseOutSec = 0.2;
    const double VibratoDefaultDepthSemitone = 1.0;
    const double VibratoDefaultFrequencyHz = 5.5;
    const double VibratoDefaultPhaseRad = 0.0;

    internal readonly struct PitchPoint {
      public long Tick { get; }
      public double Semitones { get; }
      public PitchPoint(long tick, double semitones) {
        Tick = tick;
        Semitones = semitones;
      }
    }

    internal sealed class NoteVibrato {
      public long NoteStartTick { get; init; }
      public long NoteLengthTick { get; init; }
      public long NoteEndTick => NoteStartTick + NoteLengthTick;
      public double? VibratoStartSec { get; init; }
      public double? EaseInSec { get; init; }
      public double? EaseOutSec { get; init; }
      public double? DepthSemitone { get; init; }
      public double? FrequencyHz { get; init; }
      public double? PhaseRad { get; init; }
    }

    internal sealed class DefaultVibrato {
      public double? VibratoStartSec { get; init; }
      public double? EaseInSec { get; init; }
      public double? EaseOutSec { get; init; }
      public double? DepthSemitone { get; init; }
      public double? FrequencyHz { get; init; }
    }

    internal static List<PitchPoint> ProcessInput(
        List<PitchPoint> points,
        string? pitchMode,
        IReadOnlyList<NoteVibrato> notes,
        IReadOnlyList<(long tick, double bpm)> tempos,
        List<PitchPoint> vibratoEnvPoints,
        string? vibratoEnvMode,
        DefaultVibrato? defaultVibrato) {
      var merged = Merge(points);
      var interpolated = Interpolate(merged, pitchMode);
      var mergedEnv = Merge(vibratoEnvPoints);
      var interpolatedEnv = Interpolate(mergedEnv, vibratoEnvMode);
      var vibratoMap = ExtendEveryTick(interpolatedEnv);
      var withVibrato = AppendVibrato(interpolated, notes, defaultVibrato, tempos, vibratoMap);
      return RemoveRedundant(withVibrato);
    }

    internal static List<PitchPoint> ParseCurvePoints(SvpCurveJson? curve, long blickOffset) {
      var raw = ParsePointValues(curve?.points);
      if (raw == null || raw.Length < 2) {
        return new List<PitchPoint>();
      }
      var list = new List<PitchPoint>();
      for (int i = 0; i + 1 < raw.Length; i += 2) {
        long blick = (long)Math.Round(raw[i]) + blickOffset;
        long tick = BlickToTick(blick);
        double semitones = raw[i + 1] / 100.0;
        list.Add(new PitchPoint(tick, semitones));
      }
      return list;
    }

    internal static double[]? ParsePointValues(JToken? token) {
      if (token == null || token.Type == JTokenType.Null) {
        return null;
      }
      if (token.Type != JTokenType.Array) {
        return null;
      }
      var arr = (JArray)token;
      if (arr.Count == 0) {
        return null;
      }
      if (arr[0].Type == JTokenType.Object) {
        var values = new List<double>();
        foreach (var item in arr.OfType<JObject>()) {
          var pos = item["position"] ?? item["pos"] ?? item["x"] ?? item["onset"] ?? item["blick"];
          var val = item["value"] ?? item["val"] ?? item["y"] ?? item["cents"];
          if (pos == null || val == null) {
            continue;
          }
          values.Add(pos.Value<double>());
          values.Add(val.Value<double>());
        }
        return values.Count >= 2 ? values.ToArray() : null;
      }
      var flat = arr.ToObject<double[]>();
      return flat != null && flat.Length >= 2 ? flat : null;
    }

    internal static long BlickToTick(long blick) => blick / BlicksPerTick;

    internal static List<(int tick, int cents)> ToPitdPoints(IReadOnlyList<PitchPoint> points) {
      return points
          .Select(p => (tick: (int)p.Tick, cents: (int)Math.Round(Math.Clamp(p.Semitones * 100.0, -1200, 1200))))
          .ToList();
    }

    static List<PitchPoint> Merge(List<PitchPoint> points) {
      return points
          .GroupBy(p => p.Tick)
          .Select(g => new PitchPoint(g.Key, g.Average(p => p.Semitones)))
          .OrderBy(p => p.Tick)
          .ToList();
    }

    static List<PitchPoint> Interpolate(List<PitchPoint> points, string? mode) {
      return mode switch {
        "linear" => InterpolateLinear(points),
        _ => InterpolateCosineEaseInOut(points),
      };
    }

    static List<PitchPoint> InterpolateLinear(List<PitchPoint> points) {
      if (points.Count < 2) {
        return points;
      }
      var result = new List<PitchPoint>();
      for (int i = 0; i < points.Count - 1; i++) {
        var start = points[i];
        var end = points[i + 1];
        result.Add(start);
        if (end.Tick <= start.Tick) {
          continue;
        }
        for (long t = start.Tick + 1; t < end.Tick; t++) {
          if ((t - start.Tick) % SamplingIntervalTick == 0) {
            double ratio = (double)(t - start.Tick) / (end.Tick - start.Tick);
            double value = start.Semitones + ratio * (end.Semitones - start.Semitones);
            result.Add(new PitchPoint(t, value));
          }
        }
      }
      result.Add(points[^1]);
      return result;
    }

    static List<PitchPoint> InterpolateCosineEaseInOut(List<PitchPoint> points) {
      if (points.Count < 2) {
        return points;
      }
      var result = new List<PitchPoint>();
      for (int i = 0; i < points.Count - 1; i++) {
        var start = points[i];
        var end = points[i + 1];
        result.Add(start);
        long deltaX = end.Tick - start.Tick;
        if (deltaX <= 0) {
          continue;
        }
        double deltaY = end.Semitones - start.Semitones;
        long interval = SamplingIntervalTick;
        const long maxPoints = 500;
        if (deltaX > maxPoints * interval) {
          interval = Math.Max(1, deltaX / maxPoints);
        }
        for (long t = start.Tick + interval; t < end.Tick; t += interval) {
          double normalized = (double)(t - start.Tick) / deltaX;
          double cosValue = Math.Cos(Math.PI * normalized);
          double value = start.Semitones + deltaY * (0.5 - 0.5 * cosValue);
          result.Add(new PitchPoint(t, value));
        }
      }
      result.Add(points[^1]);
      return result;
    }

    static Dictionary<long, double> ExtendEveryTick(List<PitchPoint> points) {
      var map = new Dictionary<long, double>();
      if (points.Count == 0) {
        return map;
      }
      PitchPoint? last = null;
      foreach (var point in points) {
        if (last is { } prev) {
          for (long t = prev.Tick + 1; t < point.Tick; t++) {
            map[t] = prev.Semitones;
          }
        }
        map[point.Tick] = point.Semitones;
        last = point;
      }
      return map;
    }

    static List<PitchPoint> AppendVibrato(
        List<PitchPoint> pitchPoints,
        IReadOnlyList<NoteVibrato> notes,
        DefaultVibrato? defaults,
        IReadOnlyList<(long tick, double bpm)> tempos,
        Dictionary<long, double> vibratoEnv) {
      var transformer = new TickTimeTransformer(tempos);
      var ranges = new List<(long start, long end, NoteVibrato? note)>();
      long lastTick = 0;
      foreach (var note in notes.OrderBy(n => n.NoteStartTick)) {
        if (lastTick < note.NoteStartTick) {
          ranges.Add((lastTick, note.NoteStartTick, null));
        }
        ranges.Add((note.NoteStartTick, note.NoteEndTick, note));
        lastTick = note.NoteEndTick;
      }
      ranges.Add((lastTick, long.MaxValue, null));

      var sorted = pitchPoints.OrderBy(p => p.Tick).ToList();
      var result = new List<PitchPoint>();
      int pitchIndex = 0;
      foreach (var (start, end, note) in ranges) {
        while (pitchIndex < sorted.Count && sorted[pitchIndex].Tick < start) {
          pitchIndex++;
        }
        int startIndex = pitchIndex;
        while (pitchIndex < sorted.Count && sorted[pitchIndex].Tick < end) {
          pitchIndex++;
        }
        if (startIndex < pitchIndex) {
          var subset = sorted.GetRange(startIndex, pitchIndex - startIndex);
          result.AddRange(AppendVibratoInNote(subset, note, defaults, transformer, tempos, vibratoEnv));
        }
      }
      return result;
    }

    static List<PitchPoint> AppendVibratoInNote(
        List<PitchPoint> basePoints,
        NoteVibrato? note,
        DefaultVibrato? defaults,
        TickTimeTransformer transformer,
        IReadOnlyList<(long tick, double bpm)> tempos,
        Dictionary<long, double> vibratoEnv) {
      if (note == null || note.NoteStartTick < 0) {
        return basePoints;
      }

      double noteStartSec = transformer.TickToSec(note.NoteStartTick);
      double noteEndSec = transformer.TickToSec(note.NoteEndTick);

      double vibratoStartSec = (note.VibratoStartSec ?? defaults?.VibratoStartSec ?? VibratoDefaultStartSec) + noteStartSec;
      double easeIn = note.EaseInSec ?? defaults?.EaseInSec ?? VibratoDefaultEaseInSec;
      double easeOut = note.EaseOutSec ?? defaults?.EaseOutSec ?? VibratoDefaultEaseOutSec;
      double depth = (note.DepthSemitone ?? defaults?.DepthSemitone ?? VibratoDefaultDepthSemitone) * 0.5;
      if (depth == 0) {
        return basePoints;
      }
      double phase = note.PhaseRad ?? VibratoDefaultPhaseRad;
      double frequency = note.FrequencyHz ?? defaults?.FrequencyHz ?? VibratoDefaultFrequencyHz;

      double bpm = tempos.Where(t => t.tick <= note.NoteStartTick).Select(t => t.bpm).LastOrDefault(120);
      double secPerTick = 60.0 / TicksPerBeat / bpm;

      double VibratoAt(long tick) {
        double sec = transformer.TickToSec(tick);
        if (sec < vibratoStartSec) {
          return 0;
        }
        double easeInFactor = easeIn > 0 ? Math.Clamp((sec - vibratoStartSec) / easeIn, 0, 1) : 1;
        double easeOutFactor = easeOut > 0 ? Math.Clamp((noteEndSec - sec) / easeOut, 0, 1) : 1;
        long vibratoStartTick = transformer.SecToTick(vibratoStartSec);
        double rad = 2 * Math.PI * frequency * secPerTick * (tick - vibratoStartTick) + phase;
        double envelope = vibratoEnv.TryGetValue(tick, out var env) ? env : 1.0;
        return envelope * depth * easeInFactor * easeOutFactor * Math.Sin(rad);
      }

      var points = basePoints.Count > 0
          ? new List<PitchPoint>(basePoints)
          : new List<PitchPoint> {
            new(note.NoteStartTick, 0),
            new(note.NoteEndTick, 0),
          };
      if (points[^1].Tick != note.NoteEndTick) {
        points.Add(new PitchPoint(note.NoteEndTick, points[^1].Semitones));
      }

      var result = new List<PitchPoint>();
      PitchPoint? prev = null;
      foreach (var point in points) {
        if (prev == null) {
          result.Add(new PitchPoint(point.Tick, point.Semitones + VibratoAt(point.Tick)));
        } else {
          for (long t = prev.Value.Tick + SamplingIntervalTick; t < point.Tick; t += SamplingIntervalTick) {
            result.Add(new PitchPoint(t, prev.Value.Semitones + VibratoAt(t)));
          }
          result.Add(new PitchPoint(point.Tick, point.Semitones + VibratoAt(point.Tick)));
        }
        prev = point;
      }
      return result;
    }

    static List<PitchPoint> RemoveRedundant(List<PitchPoint> points) {
      var result = new List<PitchPoint>();
      double? lastValue = null;
      foreach (var point in points) {
        if (lastValue == null || Math.Abs(point.Semitones - lastValue.Value) > 0.00001) {
          result.Add(point);
          lastValue = point.Semitones;
        }
      }
      return result;
    }

    sealed class TickTimeTransformer {
      readonly List<Segment> segments;

      sealed class Segment {
        public long Start;
        public long End;
        public double OffsetSec;
        public double SecPerTick;
      }

      public TickTimeTransformer(IReadOnlyList<(long tick, double bpm)> tempos) {
        segments = new List<Segment>();
        if (tempos.Count == 0) {
          segments.Add(new Segment { Start = 0, End = long.MaxValue, OffsetSec = 0, SecPerTick = 60.0 / TicksPerBeat / 120 });
          return;
        }
        for (int i = 0; i < tempos.Count; i++) {
          long start = tempos[i].tick;
          long end = i + 1 < tempos.Count ? tempos[i + 1].tick : long.MaxValue;
          double secPerTick = 60.0 / TicksPerBeat / tempos[i].bpm;
          double offset = segments.Count == 0
              ? 0
              : segments[^1].OffsetSec + (segments[^1].End - segments[^1].Start) * segments[^1].SecPerTick;
          segments.Add(new Segment { Start = start, End = end, OffsetSec = offset, SecPerTick = secPerTick });
        }
      }

      Segment GetSegment(long tick) {
        foreach (var seg in segments) {
          if (tick >= seg.Start && tick < seg.End) {
            return seg;
          }
        }
        return segments[0];
      }

      public double TickToSec(long tick) {
        var seg = GetSegment(tick);
        return seg.OffsetSec + (tick - seg.Start) * seg.SecPerTick;
      }

      public long SecToTick(double sec) {
        Segment seg = segments.LastOrDefault(s => s.OffsetSec <= sec) ?? segments[0];
        return (long)Math.Round((sec - seg.OffsetSec) / seg.SecPerTick) + seg.Start;
      }
    }
  }
}
