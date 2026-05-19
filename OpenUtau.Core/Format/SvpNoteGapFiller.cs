using System.Collections.Generic;

namespace OpenUtau.Core.Format {
  /// <summary>
  /// Extends notes to close short gaps before the next note (UtaFormatix slight rests filling).
  /// </summary>
  internal static class SvpNoteGapFiller {
    internal const int TicksPerFullNote = 480 * 4;

    /// <param name="maxLengthDenominator">
    /// Gap threshold as 1/denominator of a whole note (exclusive). E.g. 64 → gaps shorter than 1/64 note are filled.
    /// </param>
    internal static void FillShortGaps<T>(IList<T> notes, int maxLengthDenominator, System.Func<T, int> getTickOn,
        System.Func<T, int> getTickOff, System.Action<T, int> setTickOff) {
      if (notes.Count < 2 || maxLengthDenominator <= 0) {
        return;
      }
      int maxGapExclusive = TicksPerFullNote / maxLengthDenominator;
      for (int i = 0; i < notes.Count - 1; i++) {
        var current = notes[i];
        var next = notes[i + 1];
        int gap = getTickOn(next) - getTickOff(current);
        if (gap > 0 && gap < maxGapExclusive) {
          setTickOff(current, getTickOn(next));
        }
      }
    }
  }
}
