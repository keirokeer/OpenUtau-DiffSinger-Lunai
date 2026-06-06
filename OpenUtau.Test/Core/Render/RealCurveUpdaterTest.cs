using System;
using System.Collections.Generic;
using System.Linq;
using OpenUtau.Core.Render;
using OpenUtau.Core.Ustx;
using Xunit;

namespace OpenUtau.Core {
    public class RealCurveUpdaterTest {
        const string Ene = "ene";

        [Fact]
        public void RenderPhraseEventsReportsNonEmptyRealCurvesOnly() {
            int reports = 0;
            var events = new RenderPhraseEvents(_ => reports++);

            events.ReportRealCurves(new List<RenderRealCurveResult>());
            events.ReportRealCurves(new[] {
                new RenderRealCurveResult {
                    abbr = Ene,
                    ticks = new[] { 0f },
                    values = new[] { 0.1f },
                },
            });

            Assert.Equal(1, reports);
        }

        [Fact]
        public void BuildUpdatesConvertsTicksToPartLocalCoordinates() {
            var result = new RenderRealCurveResult {
                abbr = Ene,
                ticks = new[] { 0f, 5f, 10f },
                values = new[] { 0.1f, 0.2f, 0.3f },
            };

            var update = Assert.Single(RealCurveUpdater.BuildUpdates(
                partPosition: 100,
                phrasePosition: 250,
                phraseHash: 42,
                results: new[] { result }));

            Assert.Equal(Ene, update.abbr);
            Assert.Equal((ulong)42, update.phraseHash);
            Assert.Equal(150, update.startTick);
            Assert.Equal(160, update.endTick);
            Assert.Equal(new[] { 150, 150, 155, 160 }, update.xs);
            Assert.Equal(new[] { -1, 100, 200, 300 }, update.ys);
        }

        [Fact]
        public void ApplyReplacesMatchingPhraseRangeOnly() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            curve.realXs.AddRange(new[] { 0, 0, 10, 100, 100, 110 });
            curve.realYs.AddRange(new[] { -1, 10, 20, -1, 30, 40 });
            var update = new RealCurveUpdate(
                Ene,
                phraseHash: 42,
                startTick: 100,
                endTick: 110,
                xs: new[] { 100, 100, 105, 110 },
                ys: new[] { -1, 300, 400, 500 });

            bool changed = RealCurveUpdater.Apply(
                project,
                part,
                new[] { update },
                new HashSet<ulong> { 42 });

            Assert.True(changed);
            Assert.Equal(new[] { 0, 0, 10, 100, 100, 105, 110 }, curve.realXs);
            Assert.Equal(new[] { -1, 10, 20, -1, 300, 400, 500 }, curve.realYs);
        }

        [Fact]
        public void ApplySkipsStalePhraseHash() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            curve.realXs.AddRange(new[] { 0, 0, 10 });
            curve.realYs.AddRange(new[] { -1, 10, 20 });
            var update = new RealCurveUpdate(
                Ene,
                phraseHash: 42,
                startTick: 0,
                endTick: 10,
                xs: new[] { 0, 0, 5 },
                ys: new[] { -1, 300, 400 });

            bool changed = RealCurveUpdater.Apply(
                project,
                part,
                new[] { update },
                new HashSet<ulong> { 43 });

            Assert.False(changed);
            Assert.Equal(new[] { 0, 0, 10 }, curve.realXs);
            Assert.Equal(new[] { -1, 10, 20 }, curve.realYs);
        }

        [Fact]
        public void TrimToCoverageNoOpWhenPartNotInProject() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            curve.realXs.AddRange(new[] { 0 });
            curve.realYs.AddRange(new[] { 1 });
            project.parts.Remove(part);

            bool changed = RealCurveUpdater.TrimToCoverage(project, part, new[] {
                new RealCurveUpdate(Ene, 1, 0, 10, new[] { 0 }, new[] { 1 }),
            });

            Assert.False(changed);
            Assert.Equal(new[] { 0 }, curve.realXs);
            Assert.Equal(new[] { 1 }, curve.realYs);
        }

        [Fact]
        public void TrimToCoverageRemovesEntriesOutsideUnionForAbbrsWithUpdates() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            // Stale tail data past 110 that an earlier render with a wider phrase left behind.
            curve.realXs.AddRange(new[] { 0, 0, 10, 100, 100, 110, 150, 200 });
            curve.realYs.AddRange(new[] { -1, 10, 20, -1, 30, 40, 50, 60 });

            // Two updates whose union is [0,10] U [100,110]. Entries at 150 and 200 fall outside.
            bool changed = RealCurveUpdater.TrimToCoverage(project, part, new[] {
                new RealCurveUpdate(Ene, 1, 0, 10, new[] { 0, 0, 5, 10 }, new[] { -1, 11, 12, 13 }),
                new RealCurveUpdate(Ene, 2, 100, 110, new[] { 100, 100, 105, 110 }, new[] { -1, 31, 32, 33 }),
            });

            Assert.True(changed);
            Assert.Equal(new[] { 0, 0, 10, 100, 100, 110 }, curve.realXs);
            Assert.Equal(new[] { -1, 10, 20, -1, 30, 40 }, curve.realYs);
        }

        [Fact]
        public void TrimToCoverageMergesOverlappingRanges() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            curve.realXs.AddRange(new[] { 0, 50, 80, 150 });
            curve.realYs.AddRange(new[] { 1, 2, 3, 4 });

            // Two overlapping ranges [0,60] and [40,100] merge into [0,100]; 150 falls outside.
            bool changed = RealCurveUpdater.TrimToCoverage(project, part, new[] {
                new RealCurveUpdate(Ene, 1, 0, 60, new[] { 0, 60 }, new[] { 1, 2 }),
                new RealCurveUpdate(Ene, 2, 40, 100, new[] { 40, 100 }, new[] { 3, 4 }),
            });

            Assert.True(changed);
            Assert.Equal(new[] { 0, 50, 80 }, curve.realXs);
            Assert.Equal(new[] { 1, 2, 3 }, curve.realYs);
        }

        [Fact]
        public void TrimToCoverageLeavesAbbrsWithoutUpdatesUntouched() {
            var project = new UProject();
            var eneDescriptor = new UExpressionDescriptor("energy", Ene, 0, 100, 0) {
                type = UExpressionType.Curve,
            };
            var breDescriptor = new UExpressionDescriptor("breath", "bre", 0, 100, 0) {
                type = UExpressionType.Curve,
            };
            project.RegisterExpression(eneDescriptor);
            project.RegisterExpression(breDescriptor);
            var part = new UVoicePart { trackNo = 0, position = 0, Duration = 480 };
            project.parts.Add(part);
            var eneCurve = new UCurve(eneDescriptor);
            var breCurve = new UCurve(breDescriptor);
            part.curves.Add(eneCurve);
            part.curves.Add(breCurve);
            eneCurve.realXs.AddRange(new[] { 0, 200 });
            eneCurve.realYs.AddRange(new[] { 1, 2 });
            breCurve.realXs.AddRange(new[] { 0, 200 });
            breCurve.realYs.AddRange(new[] { 3, 4 });

            // Only ene has an update; bre should be untouched even though its data extends past
            // ene's covered range.
            bool changed = RealCurveUpdater.TrimToCoverage(project, part, new[] {
                new RealCurveUpdate(Ene, 1, 0, 100, new[] { 0, 100 }, new[] { 1, 2 }),
            });

            Assert.True(changed);
            Assert.Equal(new[] { 0 }, eneCurve.realXs);
            Assert.Equal(new[] { 1 }, eneCurve.realYs);
            Assert.Equal(new[] { 0, 200 }, breCurve.realXs);
            Assert.Equal(new[] { 3, 4 }, breCurve.realYs);
        }

        [Fact]
        public void TrimToCoverageNoOpWhenUpdatesEmpty() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            curve.realXs.AddRange(new[] { 0, 100 });
            curve.realYs.AddRange(new[] { 1, 2 });

            bool changed = RealCurveUpdater.TrimToCoverage(project, part, Array.Empty<RealCurveUpdate>());

            Assert.False(changed);
            Assert.Equal(new[] { 0, 100 }, curve.realXs);
            Assert.Equal(new[] { 1, 2 }, curve.realYs);
        }

        static UProject CreateProjectWithCurve(out UVoicePart part, out UCurve curve) {
            var project = new UProject();
            var descriptor = new UExpressionDescriptor("energy", Ene, 0, 100, 0) {
                type = UExpressionType.Curve,
            };
            project.RegisterExpression(descriptor);
            part = new UVoicePart {
                trackNo = 0,
                position = 0,
                Duration = 480,
            };
            project.parts.Add(part);
            curve = new UCurve(descriptor);
            part.curves.Add(curve);
            return project;
        }
    }
}
