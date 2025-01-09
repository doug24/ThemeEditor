using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DiffPlex.DiffBuilder.Model;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace ThemeEditor
{
    /// <summary>
    /// The ReplaceViewHighlighter is used to color the background of match occurrences to
    /// show which are selected for replacement; also outlines the currently selected match
    /// </summary>
    public class ReplaceViewHighlighter : IBackgroundRenderer
    {
        public ReplaceViewHighlighter()
        {
            outlinePen = new Pen(penBrush, 1);
            outlinePen.Freeze();

            transparentPen = new Pen(Brushes.Transparent, 0.0);
            transparentPen.Freeze();
        }

        public List<DiffPiece> DiffLines { get; } = [];

        /// <summary>
        /// The ordered list of line numbers from the original source file
        /// </summary>
        public List<int> LineNumbers { get; } = [];

        private readonly Brush? insertedLineBackground = Application.Current.Resources["DiffText.Inserted.Line.Background"] as Brush;
        private readonly Brush? deletedLineBackground = Application.Current.Resources["DiffText.Deleted.Line.Background"] as Brush;
        private readonly Brush? insertedWordBackground = Application.Current.Resources["DiffText.Inserted.Word.Background"] as Brush;
        private readonly Brush? deletedWordBackground = Application.Current.Resources["DiffText.Deleted.Word.Background"] as Brush;
        private readonly Brush? penBrush = Application.Current.Resources["Match.Skip.Foreground"] as Brush;
        private readonly Pen outlinePen;
        private readonly Pen transparentPen;

        /// <summary>Gets the layer on which this background renderer should draw.</summary>
        public KnownLayer Layer => KnownLayer.Selection; // draw behind selection

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (!textView.VisualLinesValid)
                return;

            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0)
                return;

            foreach (VisualLine visLine in textView.VisualLines)
            {
                var lineNum = visLine.FirstDocumentLine.LineNumber - 1;

                if (DiffLines.Count > 0 && lineNum < DiffLines.Count)
                {
                    var diffLine = DiffLines[lineNum];
                    if (diffLine.Type == ChangeType.Inserted || diffLine.Type == ChangeType.Deleted)
                    {
                        DrawDiffBackground(diffLine, visLine, textView, drawingContext);
                    }
                }
            }
        }

        private void DrawDiffBackground(DiffPiece diffLine, VisualLine visLine,
            TextView textView, DrawingContext drawingContext)
        {
            var brush = diffLine.Type == ChangeType.Inserted ? insertedLineBackground : deletedLineBackground;

            foreach (var rc in BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visLine, 0, 1000))
            {
                drawingContext.DrawRectangle(brush, transparentPen,
                    new Rect(0, rc.Top, textView.ActualWidth, rc.Height));
            }

            int startOffset = 0;
            int endOffset = 0;
            foreach (var piece in diffLine.SubPieces)
            {
                endOffset += string.IsNullOrEmpty(piece.Text) ? 0 : piece.Text.Length;

                if (piece.Type != ChangeType.Inserted && piece.Type != ChangeType.Deleted)
                {
                    startOffset = endOffset;
                    continue;
                }

                brush = piece.Type == ChangeType.Inserted ? insertedWordBackground : deletedWordBackground;

                var rects = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visLine, startOffset, endOffset);
                if (rects.Any())
                {
                    BackgroundGeometryBuilder geoBuilder = new()
                    {
                        AlignToWholePixels = true,
                    };
                    foreach (var rect in rects)
                    {
                        geoBuilder.AddRectangle(textView, rect);
                    }
                    Geometry geometry = geoBuilder.CreateGeometry();
                    if (geometry != null)
                    {
                        drawingContext.DrawGeometry(brush, transparentPen, geometry);
                    }
                }

                startOffset = endOffset;
            }
        }
    }
}
