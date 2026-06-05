using System.Collections.Generic;

namespace OpenUtau.Core.DiffSinger {
    public static class DiffSingerRetake {
        public static HashSet<int> MapSelectedPositionsToNoteIndexes(
            int phrasePosition,
            IReadOnlyList<int> noteRelativePositions,
            IReadOnlyCollection<int> selectedAbsolutePositions) {
            var result = new HashSet<int>();
            if (selectedAbsolutePositions == null || selectedAbsolutePositions.Count == 0) {
                return result;
            }
            var lookup = selectedAbsolutePositions as ISet<int> ?? new HashSet<int>(selectedAbsolutePositions);
            for (int i = 0; i < noteRelativePositions.Count; i++) {
                if (lookup.Contains(phrasePosition + noteRelativePositions[i])) {
                    result.Add(i);
                }
            }
            return result;
        }

        public static bool[] BuildRetakeFrameMask(
            IReadOnlyList<int> paddedNoteDurations,
            int realNoteCount,
            IReadOnlyCollection<int> retakeNoteIndexes,
            int totalFrames) {
            var mask = new bool[totalFrames];
            if (retakeNoteIndexes == null || retakeNoteIndexes.Count == 0 || paddedNoteDurations.Count == 0) {
                return mask;
            }
            var lookup = retakeNoteIndexes as ISet<int> ?? new HashSet<int>(retakeNoteIndexes);
            int padded = paddedNoteDurations.Count;
            int frameOffset = 0;
            for (int noteIdx = 0; noteIdx < padded; noteIdx++) {
                int realIdx;
                if (noteIdx == 0) {
                    realIdx = 0;
                } else if (noteIdx == padded - 1) {
                    realIdx = realNoteCount - 1;
                } else {
                    realIdx = noteIdx - 1;
                }
                bool shouldRetake = lookup.Contains(realIdx);
                int dur = paddedNoteDurations[noteIdx];
                for (int f = 0; f < dur; f++) {
                    int fi = frameOffset + f;
                    if (fi < totalFrames) {
                        mask[fi] = shouldRetake;
                    }
                }
                frameOffset += dur;
            }
            return mask;
        }
    }
}

