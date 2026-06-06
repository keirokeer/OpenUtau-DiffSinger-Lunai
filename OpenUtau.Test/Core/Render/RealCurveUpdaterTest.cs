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
        public void ApplyFullRefreshClearsEveryRealCurveInPart() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            // Stale data from a previous render that covered a wider tick range than the
            // restored phrase will after undo.
            curve.realXs.AddRange(new[] { 0, 0, 10, 50, 50, 80 });
            curve.realYs.AddRange(new[] { -1, 10, 20, -1, 90, 95 });

            bool changed = RealCurveUpdater.ApplyFullRefresh(project, part, Array.Empty<RealCurveUpdate>());

            Assert.True(changed);
            Assert.Empty(curve.realXs);
            Assert.Empty(curve.realYs);
        }

        [Fact]
        public void ApplyFullRefreshNoOpWhenAlreadyEmpty() {
            var project = CreateProjectWithCurve(out var part, out _);

            bool changed = RealCurveUpdater.ApplyFullRefresh(project, part, Array.Empty<RealCurveUpdate>());

            Assert.False(changed);
        }

        [Fact]
        public void ApplyFullRefreshNoOpWhenPartNotInProject() {
            var project = CreateProjectWithCurve(out var part, out var curve);
            curve.realXs.AddRange(new[] { 0 });
            curve.realYs.AddRange(new[] { 1 });
            project.parts.Remove(part);

            bool changed = RealCurveUpdater.ApplyFullRefresh(project, part, Array.Empty<RealCurveUpdate>());

            Assert.False(changed);
            Assert.Equal(new[] { 0 }, curve.realXs);
            Assert.Equal(new[] { 1 }, curve.realYs);
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
