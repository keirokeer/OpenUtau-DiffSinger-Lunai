using System.Collections.Generic;
using System.Linq;
using OpenUtau.Core.DiffSinger;
using Xunit;

namespace OpenUtau.Core {
    public class DiffSingerRetakeTest {
        [Fact]
        public void MapSelectedPositionsToNoteIndexes_PicksMatchingNotes() {
            var noteRel = new[] { 0, 480, 960, 1440 };
            var selected = new HashSet<int> { 100 + 480, 100 + 1440 };

            var result = DiffSingerRetake.MapSelectedPositionsToNoteIndexes(100, noteRel, selected);

            Assert.Equal(new HashSet<int> { 1, 3 }, result);
        }

        [Fact]
        public void MapSelectedPositionsToNoteIndexes_ReturnsEmptyWhenNoneSelected() {
            var noteRel = new[] { 0, 480 };
            var result = DiffSingerRetake.MapSelectedPositionsToNoteIndexes(0, noteRel, new HashSet<int>());
            Assert.Empty(result);
        }

        [Fact]
        public void MapSelectedPositionsToNoteIndexes_HandlesNullSelected() {
            var noteRel = new[] { 0, 480 };
            var result = DiffSingerRetake.MapSelectedPositionsToNoteIndexes(0, noteRel, null);
            Assert.Empty(result);
        }

        [Fact]
        public void BuildRetakeFrameMask_AllSelected_AllTrue() {
            var paddedDurations = new[] { 2, 5, 5, 2 };
            var totalFrames = paddedDurations.Sum();
            var indexes = new HashSet<int> { 0, 1 };

            var mask = DiffSingerRetake.BuildRetakeFrameMask(paddedDurations, 2, indexes, totalFrames);

            Assert.Equal(totalFrames, mask.Length);
            Assert.All(mask, b => Assert.True(b));
        }

        [Fact]
        public void BuildRetakeFrameMask_NoneSelected_AllFalse() {
            var paddedDurations = new[] { 2, 5, 5, 2 };
            var totalFrames = paddedDurations.Sum();
            var indexes = new HashSet<int>();

            var mask = DiffSingerRetake.BuildRetakeFrameMask(paddedDurations, 2, indexes, totalFrames);

            Assert.Equal(totalFrames, mask.Length);
            Assert.All(mask, b => Assert.False(b));
        }

        [Fact]
        public void BuildRetakeFrameMask_PartialSelected_RespectsHeadTailPaddingShift() {
            // 3 real notes, padded with head + tail → 5 padded "note durations".
            // Mapping: padded[0] → real 0 (head), padded[1] → real 0, padded[2] → real 1,
            //          padded[3] → real 2, padded[4] → real 2 (tail).
            var paddedDurations = new[] { 2, 3, 4, 3, 2 };  // 14 frames total
            int totalFrames = paddedDurations.Sum();
            var indexes = new HashSet<int> { 1 };  // retake only middle real note

            var mask = DiffSingerRetake.BuildRetakeFrameMask(paddedDurations, 3, indexes, totalFrames);

            // padded[0] (frames 0-1, head→real 0) → false
            Assert.False(mask[0]);
            Assert.False(mask[1]);
            // padded[1] (frames 2-4, real 0) → false
            Assert.False(mask[2]);
            Assert.False(mask[4]);
            // padded[2] (frames 5-8, real 1) → true
            Assert.True(mask[5]);
            Assert.True(mask[8]);
            // padded[3] (frames 9-11, real 2) → false
            Assert.False(mask[9]);
            Assert.False(mask[11]);
            // padded[4] (frames 12-13, tail→real 2) → false
            Assert.False(mask[12]);
            Assert.False(mask[13]);
        }

        [Fact]
        public void BuildRetakeFrameMask_FirstRealNoteSelected_HeadPadIncluded() {
            // Selecting real note 0 should mark both head (padded[0]) and padded[1] frames.
            var paddedDurations = new[] { 2, 3, 4, 3, 2 };
            int totalFrames = paddedDurations.Sum();
            var indexes = new HashSet<int> { 0 };

            var mask = DiffSingerRetake.BuildRetakeFrameMask(paddedDurations, 3, indexes, totalFrames);

            Assert.True(mask[0]);
            Assert.True(mask[1]);  // head padded → real 0
            Assert.True(mask[2]);
            Assert.True(mask[4]);  // padded[1] → real 0
            Assert.False(mask[5]); // padded[2] → real 1, not selected
        }

        [Fact]
        public void BuildRetakeFrameMask_LastRealNoteSelected_TailPadIncluded() {
            var paddedDurations = new[] { 2, 3, 4, 3, 2 };
            int totalFrames = paddedDurations.Sum();
            var indexes = new HashSet<int> { 2 };  // last real note

            var mask = DiffSingerRetake.BuildRetakeFrameMask(paddedDurations, 3, indexes, totalFrames);

            Assert.False(mask[8]);
            Assert.True(mask[9]);   // padded[3] → real 2
            Assert.True(mask[11]);
            Assert.True(mask[12]);  // padded[4] tail → real 2
            Assert.True(mask[13]);
        }

        [Fact]
        public void BuildRetakeFrameMask_ClampsFramesPastTotal() {
            // paddedDurations sum to 10 but totalFrames is 8 (simulating FitDurationSum trim).
            var paddedDurations = new[] { 2, 4, 4 };
            var indexes = new HashSet<int> { 0 };

            var mask = DiffSingerRetake.BuildRetakeFrameMask(paddedDurations, 1, indexes, 8);

            Assert.Equal(8, mask.Length);
            // Should not throw; frames past totalFrames silently dropped.
            Assert.True(mask[0]);
            Assert.True(mask[5]);
        }

        [Fact]
        public void BuildRetakeFrameMask_EmptyDurations_ReturnsAllFalse() {
            var mask = DiffSingerRetake.BuildRetakeFrameMask(new int[0], 0, new HashSet<int> { 0 }, 4);
            Assert.Equal(4, mask.Length);
            Assert.All(mask, b => Assert.False(b));
        }
    }
}
