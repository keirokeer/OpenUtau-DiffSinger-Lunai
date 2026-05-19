using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;

namespace OpenUtau.App.Controls {
    partial class TrackHeaderCanvas {
        const double DragThreshold = 4;

        UTrack? dragTrack;
        TrackHeader? dragHeader;
        double dragStartY;
        double dragGrabOffsetY;
        double dragHeaderBaseTop;
        int dragInsertIndex = -1;
        bool dragActive;
        bool suppressCaptureLostCancel;
        Border? insertIndicator;

        public void BeginTrackReorder(UTrack track, TrackHeader header, PointerPressedEventArgs e) {
            if (dragActive || track == null || !trackHeaders.TryGetValue(track, out var existing) || existing != header) {
                return;
            }
            dragTrack = track;
            dragHeader = header;
            dragStartY = e.GetPosition(this).Y;
            dragGrabOffsetY = e.GetPosition(header).Y;
            dragHeaderBaseTop = Canvas.GetTop(header);
            dragInsertIndex = track.TrackNo;
            dragActive = false;
            e.Pointer.Capture(this);
            e.Handled = true;
        }

        protected override void OnPointerMoved(PointerEventArgs e) {
            base.OnPointerMoved(e);
            if (dragHeader != null) {
                UpdateTrackReorder(dragHeader, e);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e) {
            base.OnPointerReleased(e);
            if (dragHeader != null) {
                EndTrackReorder(dragHeader, e);
            }
        }

        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e) {
            base.OnPointerCaptureLost(e);
            if (!suppressCaptureLostCancel && dragHeader != null) {
                FinishTrackReorder(commit: false);
            }
        }

        public void UpdateTrackReorder(TrackHeader header, PointerEventArgs e) {
            if (dragTrack == null || dragHeader != header) {
                return;
            }
            var point = e.GetPosition(this);
            if (!dragActive) {
                if (Math.Abs(point.Y - dragStartY) < DragThreshold) {
                    return;
                }
                dragActive = true;
                dragHeader.Opacity = 0.85;
                dragHeader.ZIndex = 100;
                EnsureInsertIndicator();
            }
            Canvas.SetTop(dragHeader, point.Y - dragGrabOffsetY);
            dragInsertIndex = GetInsertIndex(point.Y);
            UpdateInsertIndicator();
            e.Handled = true;
        }

        public void EndTrackReorder(TrackHeader header, PointerReleasedEventArgs e) {
            if (dragTrack == null || dragHeader != header) {
                return;
            }
            suppressCaptureLostCancel = true;
            FinishTrackReorder(commit: true);
            suppressCaptureLostCancel = false;
            e.Handled = true;
        }

        void FinishTrackReorder(bool commit) {
            if (dragHeader != null) {
                dragHeader.Opacity = 1;
                dragHeader.ZIndex = 0;
                Canvas.SetTop(dragHeader, dragHeaderBaseTop);
            }
            RemoveInsertIndicator();

            if (commit && dragActive && dragTrack != null) {
                int oldIndex = dragTrack.TrackNo;
                int targetIndex = dragInsertIndex;
                if (targetIndex > oldIndex) {
                    targetIndex--;
                }
                if (targetIndex != oldIndex && targetIndex >= 0 && targetIndex < _items.Count) {
                    DocManager.Inst.StartUndoGroup("command.track.order");
                    DocManager.Inst.ExecuteCmd(new ReorderTrackCommand(DocManager.Inst.Project, dragTrack, targetIndex));
                    DocManager.Inst.EndUndoGroup();
                }
            }

            dragTrack = null;
            dragHeader = null;
            dragActive = false;
            dragInsertIndex = -1;
        }

        int GetInsertIndex(double y) {
            if (TrackHeight <= 0 || _items.Count == 0) {
                return 0;
            }
            double relativeY = y + TrackOffset * TrackHeight;
            int index = (int)Math.Floor(relativeY / TrackHeight);
            return Math.Clamp(index, 0, _items.Count);
        }

        void EnsureInsertIndicator() {
            if (insertIndicator != null) {
                return;
            }
            insertIndicator = new Border {
                Height = 2,
                Background = Brushes.White,
                Opacity = 0.75,
                IsHitTestVisible = false,
            };
            Children.Add(insertIndicator);
        }

        void UpdateInsertIndicator() {
            if (insertIndicator == null || TrackHeight <= 0) {
                return;
            }
            double top = dragInsertIndex * TrackHeight - TrackOffset * TrackHeight - 1;
            Canvas.SetTop(insertIndicator, top);
            insertIndicator.Width = Bounds.Width > 0 ? Bounds.Width : 300;
            Canvas.SetLeft(insertIndicator, 0);
        }

        void RemoveInsertIndicator() {
            if (insertIndicator == null) {
                return;
            }
            Children.Remove(insertIndicator);
            insertIndicator = null;
        }
    }
}
