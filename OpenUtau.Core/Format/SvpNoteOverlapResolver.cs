using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace OpenUtau.Core.Format {
  /// <summary>
  /// Trims note ends so consecutive notes do not overlap (OpenUtau sets OverlapError when Prev.End &gt; position).
  /// UtaFormatix fillRests can also create boundary conflicts after gap filling and blick→tick rounding.
  /// </summary>
  internal static class SvpNoteOverlapResolver {
    internal const int MinNoteDurationTicks = 10;

    internal static void Resolve<T>(IList<T> notes, Func<T, int> getTickOn, Func<T, int> getTickOff,
        Action<T, int> setTickOff, Action<T, int> setTickOn) {
      if (notes.Count < 2) {
        return;
      }
      var ordered = notes.OrderBy(getTickOn).ToList();
      bool anyChange = false;
      for (int pass = 0; pass < 8; pass++) {
        bool passChanged = false;
        for (int i = 0; i < ordered.Count - 1; i++) {
          var current = ordered[i];
          var next = ordered[i + 1];
          int curOn = getTickOn(current);
          int curOff = getTickOff(current);
          int nextOn = getTickOn(next);
          if (curOff > nextOn) {
            setTickOff(current, nextOn);
            passChanged = true;
            curOff = nextOn;
          }
          if (curOff - curOn < MinNoteDurationTicks) {
            int newOff = Math.Min(curOn + MinNoteDurationTicks, nextOn);
            if (newOff != curOff) {
              setTickOff(current, newOff);
              passChanged = true;
            }
          }
        }
        for (int i = 1; i < ordered.Count; i++) {
          var prev = ordered[i - 1];
          var current = ordered[i];
          if (getTickOn(current) < getTickOff(prev)) {
            setTickOn(current, getTickOff(prev));
            passChanged = true;
          }
          if (getTickOn(current) == getTickOn(prev)) {
            setTickOn(current, getTickOn(prev) + MinNoteDurationTicks);
            passChanged = true;
          }
        }
        anyChange |= passChanged;
        if (!passChanged) {
          break;
        }
      }
      int removed = ordered.RemoveAll(n => getTickOff(n) - getTickOn(n) < MinNoteDurationTicks);
      if (removed > 0) {
        Log.Warning("SVP import: removed {Count} notes shorter than {Min} ticks", removed, MinNoteDurationTicks);
      }
      notes.Clear();
      foreach (var n in ordered) {
        notes.Add(n);
      }
      if (anyChange) {
        Log.Information("SVP import: resolved overlapping notes");
      }
    }
  }
}
