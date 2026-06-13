using System.Collections.Generic;
using OpenUtau.Core.DiffSinger;
using OpenUtau.Core.Util;
using Xunit;

namespace OpenUtau.Core {
    public class DiffSingerUnvoicedConsonantPatchTest {
        static readonly HashSet<string> RuUnvoiced = new(System.StringComparer.Ordinal) {
            "ru/s", "ru/f", "ru/p", "ru/t", "ru/k", "ru/sh",
        };

        static readonly HashSet<string> EnUnvoiced = new(System.StringComparer.Ordinal) {
            "en/s", "en/k", "en/p", "en/t",
        };

        [Fact]
        public void InterpolateF0SmoothsDipAcrossPhone() {
            Preferences.Default.DiffSingerUnvoicedConsonantAcousticF0Interpolate = true;
            var phonemes = new[] { "ru/s" };
            var durations = new List<int> { 2, 8, 2 };
            var f0 = new float[] { 200, 200, 50, 50, 50, 50, 50, 50, 50, 50, 200, 200 };

            DiffSingerUnvoicedConsonantPatch.ApplyAcousticF0(phonemes, durations, 50f, f0, RuUnvoiced);

            Assert.Equal(200f, f0[5]);
        }

        [Fact]
        public void SkipsNonTargetPhonemes() {
            Preferences.Default.DiffSingerUnvoicedConsonantAcousticF0Interpolate = true;
            var phonemes = new[] { "ru/a" };
            var durations = new List<int> { 2, 8, 2 };
            var f0 = new float[] { 200, 200, 50, 50, 50, 50, 50, 50, 50, 50, 200, 200 };

            DiffSingerUnvoicedConsonantPatch.ApplyAcousticF0(phonemes, durations, 50f, f0, RuUnvoiced);

            Assert.Equal(50f, f0[5]);
        }

        [Fact]
        public void MergesConsecutiveUnvoicedConsonantsIntoOneRange() {
            var phonemes = new[] { "en/s", "en/k" };
            var durations = new List<int> { 2, 4, 4, 2 };

            var ranges = DiffSingerUnvoicedConsonantPatch.FindTargetPhoneRanges(phonemes, durations, EnUnvoiced);

            Assert.Single(ranges);
            Assert.Equal(2, ranges[0].start);
            Assert.Equal(10, ranges[0].end);
        }

        [Fact]
        public void ConsecutiveUnvoicedUsesSingleInterpolationSpan() {
            Preferences.Default.DiffSingerUnvoicedConsonantAcousticF0Interpolate = true;
            var phonemes = new[] { "en/s", "en/k" };
            var durations = new List<int> { 2, 4, 4, 2 };
            var f0 = new float[] { 200, 200, 50, 50, 50, 50, 50, 50, 50, 50, 200, 200 };

            DiffSingerUnvoicedConsonantPatch.ApplyAcousticF0(phonemes, durations, 50f, f0, EnUnvoiced);

            Assert.Equal(200f, f0[6]);
        }

        [Fact]
        public void SplitsRangesWhenVoicedPhoneIsBetweenUnvoicedOnes() {
            var phonemes = new[] { "en/s", "en/a", "en/k" };
            var durations = new List<int> { 2, 3, 4, 3, 2 };

            var ranges = DiffSingerUnvoicedConsonantPatch.FindTargetPhoneRanges(phonemes, durations, EnUnvoiced);

            Assert.Equal(2, ranges.Count);
            Assert.Equal(2, ranges[0].start);
            Assert.Equal(5, ranges[0].end);
            Assert.Equal(9, ranges[1].start);
            Assert.Equal(12, ranges[1].end);
        }
    }
}
