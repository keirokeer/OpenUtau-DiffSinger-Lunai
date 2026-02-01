using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using OpenUtau.App;
using OpenUtau.App.ViewModels;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
using OpenUtau.Core.Util;
using ReactiveUI;

namespace OpenUtau.App.Controls {
    class PhonemeCanvas : Control {
        public static readonly DirectProperty<PhonemeCanvas, IBrush> BackgroundProperty =
            AvaloniaProperty.RegisterDirect<PhonemeCanvas, IBrush>(
                nameof(Background),
                o => o.Background,
                (o, v) => o.Background = v);
        public static readonly DirectProperty<PhonemeCanvas, double> TickWidthProperty =
            AvaloniaProperty.RegisterDirect<PhonemeCanvas, double>(
                nameof(TickWidth),
                o => o.TickWidth,
                (o, v) => o.TickWidth = v);
        public static readonly DirectProperty<PhonemeCanvas, double> TickOffsetProperty =
            AvaloniaProperty.RegisterDirect<PhonemeCanvas, double>(
                nameof(TickOffset),
                o => o.TickOffset,
                (o, v) => o.TickOffset = v);
        public static readonly DirectProperty<PhonemeCanvas, UVoicePart?> PartProperty =
            AvaloniaProperty.RegisterDirect<PhonemeCanvas, UVoicePart?>(
                nameof(Part),
                o => o.Part,
                (o, v) => o.Part = v);
        public static readonly DirectProperty<PhonemeCanvas, bool> ShowPhonemeProperty =
            AvaloniaProperty.RegisterDirect<PhonemeCanvas, bool>(
                nameof(ShowPhoneme),
                o => o.ShowPhoneme,
                (o, v) => o.ShowPhoneme = v);

        public IBrush Background {
            get => background;
            private set => SetAndRaise(BackgroundProperty, ref background, value);
        }
        public double TickWidth {
            get => tickWidth;
            private set => SetAndRaise(TickWidthProperty, ref tickWidth, value);
        }
        public double TickOffset {
            get => tickOffset;
            private set => SetAndRaise(TickOffsetProperty, ref tickOffset, value);
        }
        public UVoicePart? Part {
            get => part;
            set => SetAndRaise(PartProperty, ref part, value);
        }
        public bool ShowPhoneme {
            get => showPhoneme;
            private set => SetAndRaise(ShowPhonemeProperty, ref showPhoneme, value);
        }

        private IBrush background = Brushes.White;
        private double tickWidth;
        private double tickOffset;
        private UVoicePart? part;
        private bool showPhoneme = true;

        private HashSet<UNote> selectedNotes = new HashSet<UNote>();
        private Geometry pointGeometry;
        private UPhoneme? mouseoverPhoneme;

        public PhonemeCanvas() {
            ClipToBounds = true;
            pointGeometry = new EllipseGeometry(new Rect(0, 0, 0, 0));
            MessageBus.Current.Listen<NotesRefreshEvent>()
                .Subscribe(_ => InvalidateVisual());
            MessageBus.Current.Listen<NotesSelectionEvent>()
                .Subscribe(e => {
                    selectedNotes.Clear();
                    selectedNotes.UnionWith(e.selectedNotes);
                    selectedNotes.UnionWith(e.tempSelectedNotes);
                    InvalidateVisual();
                });
            MessageBus.Current.Listen<PhonemeMouseoverEvent>()
                .Subscribe(e => {
                    if (mouseoverPhoneme != e.mouseoverPhoneme) {
                        mouseoverPhoneme = e.mouseoverPhoneme;
                        InvalidateVisual();
                    }
                });
            MessageBus.Current.Listen<ThemeChangedEvent>()
                .Subscribe(_ => InvalidateVisual());
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change) {
            base.OnPropertyChanged(change);
            InvalidateVisual();
        }

        public override void Render(DrawingContext context) {
            base.Render(context);
            if (Part == null || !ShowPhoneme) {
                return;
            }
            string langCode = PhonemeUIRender.getLangCode(Part);
            var viewModel = ((PianoRollViewModel?)DataContext)?.NotesViewModel;
            if (viewModel == null) {
                return;
            }
            bool diffSingerMode = Preferences.Default.DiffSingerPhonemePanelMode;
            // Use same brushes as expressions panel: base = BackgroundBrush (SystemRegionBrush), alt = TrackBackgroundAltBrush
            var bgBrush = diffSingerMode ? ThemeManager.BackgroundBrush : Background;
            context.DrawRectangle(bgBrush, null, Bounds.WithX(0).WithY(0));
            double leftTick = TickOffset - 480;
            double rightTick = TickOffset + Bounds.Width / TickWidth + 480;
            bool raiseText = false;
            double lastTextEndX = double.NegativeInfinity;

            // In DiffSinger mode: tag strip at top (hidden when DiffSingerLangCodeHide), bars below; in UTAU mode: use original layout
            double tagStripHeight = 0, barY, barHeight;
            if (diffSingerMode) {
                tagStripHeight = Preferences.Default.DiffSingerLangCodeHide ? 0 : ViewConstants.PhonemeTagStripHeight;
                barY = tagStripHeight;
                barHeight = Math.Max(0, Bounds.Height - tagStripHeight);
            } else {
                const double refHeight = 60;
                double scale = Bounds.Height > 0 ? Bounds.Height / refHeight : 1;
                barY = 35.5 * scale;
                barHeight = 28 * scale;
            }
            foreach (var phoneme in Part.phonemes) {
                double leftBound = viewModel.Project.timeAxis.MsPosToTickPos(phoneme.PositionMs - phoneme.preutter) - Part.position;
                double rightBound = phoneme.End;
                if (leftBound > rightTick || rightBound < leftTick || phoneme.Parent.OverlapError) {
                    continue;
                }
                var timeAxis = viewModel.Project.timeAxis;
                double x = Math.Round(viewModel.TickToneToPoint(phoneme.position, 0).X) + 0.5;
                double posMs = phoneme.PositionMs;
                if (!phoneme.Error) {
                    var pen = selectedNotes.Contains(phoneme.Parent) ? ThemeManager.NoteBorderPenPressed : ThemeManager.NoteBorderPen;
                    IBrush brush;

                    if (diffSingerMode) {
                        brush = selectedNotes.Contains(phoneme.Parent) ? ThemeManager.NoteBrush : ThemeManager.NoteEmptyBrush;
                        double xLeft = viewModel.TickToneToPoint(phoneme.position, 0).X;
                        double xRight = viewModel.TickToneToPoint(phoneme.End, 0).X;
                        // Draw delta region (original → new position) when timing was moved, using NoteColor
                        if (phoneme.rawPosition != phoneme.position) {
                            double xRaw = viewModel.TickToneToPoint(phoneme.rawPosition, 0).X;
                            double xMin = Math.Min(xRaw, xLeft);
                            double xMax = Math.Max(xRaw, xLeft);
                            var deltaRect = new Rect(xMin, barY, xMax - xMin, barHeight);
                            context.DrawRectangle(ThemeManager.NoteBrush, null, deltaRect);
                        }
                        var rect = new Rect(xLeft, barY, xRight - xLeft, barHeight);
                        context.DrawRectangle(brush, null, rect);
                        // Right border only for last phoneme (left border drawn below with penPos for timing indicator)
                        if (phoneme == Part.phonemes[Part.phonemes.Count - 1]) {
                            var penBar = selectedNotes.Contains(phoneme.Parent) ? ThemeManager.NoteBorderPenPressed : ThemeManager.NoteBorderPen;
                            context.DrawLine(penBar, new Point(xRight, barY), new Point(xRight, barY + barHeight));
                        }
                    } else {
                        brush = selectedNotes.Contains(phoneme.Parent) ? ThemeManager.NoteBrushPressed : ThemeManager.NoteBrush;
                        // Standard UTAU mode: draw envelope shape with preutter/overlap points
                        double x0 = viewModel.TickToneToPoint(timeAxis.MsPosToTickPos(posMs + phoneme.envelope.data[0].X) - Part.position, 0).X;
                        double y0 = (1 - phoneme.envelope.data[0].Y / 100) * barHeight;
                        double x1 = viewModel.TickToneToPoint(timeAxis.MsPosToTickPos(posMs + phoneme.envelope.data[1].X) - Part.position, 0).X;
                        double y1 = (1 - phoneme.envelope.data[1].Y / 100) * barHeight;
                        double x2 = viewModel.TickToneToPoint(timeAxis.MsPosToTickPos(posMs + phoneme.envelope.data[2].X) - Part.position, 0).X;
                        double y2 = (1 - phoneme.envelope.data[2].Y / 100) * barHeight;
                        double x3 = viewModel.TickToneToPoint(timeAxis.MsPosToTickPos(posMs + phoneme.envelope.data[3].X) - Part.position, 0).X;
                        double y3 = (1 - phoneme.envelope.data[3].Y / 100) * barHeight;
                        double x4 = viewModel.TickToneToPoint(timeAxis.MsPosToTickPos(posMs + phoneme.envelope.data[4].X) - Part.position, 0).X;
                        double y4 = (1 - phoneme.envelope.data[4].Y / 100) * barHeight;

                        var point0 = new Point(x0, barY + y0);
                        var point1 = new Point(x1, barY + y1);
                        var point2 = new Point(x2, barY + y2);
                        var point3 = new Point(x3, barY + y3);
                        var point4 = new Point(x4, barY + y4);
                        var polyline = new PolylineGeometry(new Point[] { point0, point1, point2, point3, point4 }, true);
                        context.DrawGeometry(brush, pen, polyline);

                        brush = phoneme.preutterDelta.HasValue ? pen!.Brush! : ThemeManager.BackgroundBrush!;
                        using (var state = context.PushTransform(Matrix.CreateTranslation(x0, barY + y0 - 1))) {
                            context.DrawGeometry(brush, pen, pointGeometry);
                        }
                        brush = phoneme.overlapDelta.HasValue ? pen!.Brush! : ThemeManager.BackgroundBrush!;
                        using (var state = context.PushTransform(Matrix.CreateTranslation(point1))) {
                            context.DrawGeometry(brush, pen, pointGeometry);
                        }
                    }
                }

                var penPos = ThemeManager.NoteBorderPen;
                if (phoneme.rawPosition != phoneme.position) {
                    penPos = ThemeManager.NoteBorderPenThickness3;
                }
                if (diffSingerMode) {
                    // Left border already drawn in diffSingerMode block; use penPos for timing indicator (thicker when moved)
                    var xLeft = viewModel.TickToneToPoint(phoneme.position, 0).X;
                    context.DrawLine(penPos, new Point(xLeft, barY), new Point(xLeft, barY + barHeight));
                } else {
                    context.DrawLine(penPos, new Point(x, barY), new Point(x, barY + barHeight));
                }

                // FIXME: Changing code below may break `HitTestAlias`.
                if (viewModel.TickWidth > ViewConstants.PianoRollTickWidthShowDetails) {
                    string phonemeText = !string.IsNullOrEmpty(phoneme.phonemeMapped) ? phoneme.phonemeMapped : phoneme.phoneme;
                    if (!string.IsNullOrEmpty(phonemeText)) {
                        if (diffSingerMode) {
                            (string tagText, string phonemeOnlyText) = PhonemeUIRender.SplitTagAndPhoneme(phonemeText, langCode);
                            double xLeft = viewModel.TickToneToPoint(phoneme.position, 0).X;
                            double xRight = viewModel.TickToneToPoint(phoneme.End, 0).X;
                            double barWidth = Math.Max(0, xRight - xLeft);
                            var brush = ThemeManager.ForegroundBrush!;
                            var bold = phoneme.phoneme != phoneme.rawPhoneme;
                            const int fontSize = 14;
                            // Tag above bars (in tag strip)
                            if (!string.IsNullOrEmpty(tagText)) {
                                var tagRect = new Rect(xLeft, 0, barWidth, tagStripHeight);
                                using (context.PushClip(tagRect)) {
                                    var tagLayout = TextLayoutCache.Get(tagText, brush, fontSize, false);
                                    double tagY = (tagStripHeight - tagLayout.Height) / 2;
                                    using (context.PushTransform(Matrix.CreateTranslation(xLeft + 2, tagY))) {
                                        tagLayout.Draw(context, new Point());
                                    }
                                }
                            }
                            // Phoneme only inside bar, vertically centered
                            if (!string.IsNullOrEmpty(phonemeOnlyText)) {
                                var barRect = new Rect(xLeft, barY, barWidth, barHeight);
                                using (context.PushClip(barRect)) {
                                    var phonemeLayout = TextLayoutCache.Get(phonemeOnlyText, brush, fontSize, bold);
                                    double phonemeY = barY + (barHeight - phonemeLayout.Height) / 2;
                                    using (context.PushTransform(Matrix.CreateTranslation(xLeft + 2, phonemeY))) {
                                        phonemeLayout.Draw(context, new Point());
                                    }
                                }
                            }
                        } else {
                            (double textX, double textY, Size size, TextLayout textLayout)
                                = PhonemeUIRender.AliasPosition(viewModel, phoneme, langCode, ref lastTextEndX, ref raiseText);
                            using (var state = context.PushTransform(Matrix.CreateTranslation(textX + 2, textY))) {
                                var pen = mouseoverPhoneme == phoneme ? ThemeManager.AccentPen1Thickness2 : ThemeManager.NeutralAccentPenSemi;
                                context.DrawRectangle(ThemeManager.BackgroundBrush, pen, new Rect(new Point(-2, 1.5), size), 4, 4);
                                textLayout.Draw(context, new Point());
                            }
                        }
                    }
                }
            }
        }
    }
}
