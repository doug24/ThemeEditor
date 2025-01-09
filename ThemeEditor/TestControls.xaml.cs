using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DiffPlex.DiffBuilder.Model;
using dnGREP.WPF;
using ICSharpCode.AvalonEdit.Editing;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for TestControls.xaml
    /// </summary>
    [SupportedOSPlatform("windows")]
    public partial class TestControls : UserControl
    {
        private readonly PreviewLineNumberMargin previewLineNumberMargin = new PreviewLineNumberMargin();
        private ReplaceViewLineNumberMargin? replaceLineNumberMargin;
        private ReplaceViewHighlighter? replaceHighlighter;

        internal MainViewModel? ViewModel { get; set; }

        public TestControls()
        {
            InitializeComponent();

            InitializePreviewControl();

            InitializeReplaceControl();

            Loaded += (s, e) =>
            {
                SetPreviewText();
                SetReplaceText();
            };

            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as MainViewModel;
                if (ViewModel != null)
                {
                    ViewModel.ThemeColorChanged += OnThemeColorChanged;
                    OnThemeColorChanged(this, EventArgs.Empty);
                }
            };
        }

        private void InitializePreviewControl()
        {
            textEditor.ShowLineNumbers = false; // using custom line numbers

            Line line = (Line)DottedLineMargin.Create();
            textEditor.TextArea.LeftMargins.Insert(0, previewLineNumberMargin);
            textEditor.TextArea.LeftMargins.Insert(1, line);
            var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = textEditor };
            line.SetBinding(Line.StrokeProperty, lineNumbersForeground);
            previewLineNumberMargin.SetBinding(Control.ForegroundProperty, lineNumbersForeground);

            textEditor.TextArea.TextView.LineTransformers.Add(new BigEllipsisColorizer());

            textEditor.TextArea.SelectionForeground = Application.Current.Resources["PreviewText.Selection.Foreground"] as Brush;
            textEditor.TextArea.SelectionBrush = Application.Current.Resources["PreviewText.Selection.Background"] as Brush;
            Pen selectionBorder = new(Application.Current.Resources["PreviewText.Selection.Border"] as Brush, 1.0);
            selectionBorder.Freeze();
            textEditor.TextArea.SelectionBorder = selectionBorder;

            textEditor.TextArea.TextView.Options.HighlightCurrentLine = true;
            textEditor.TextArea.TextView.CurrentLineBackground = Application.Current.Resources["PreviewText.CurrentLine.Background"] as Brush;
            Pen border = new(Application.Current.Resources["PreviewText.CurrentLine.Border"] as Brush, 1.0);
            border.Freeze();
            textEditor.TextArea.TextView.CurrentLineBorder = border;
        }

        private void OnThemeColorChanged(object? sender, EventArgs e)
        {
            if (Application.Current is App app)
            {
                textEditor.TextArea.TextView.LinkTextForegroundBrush = app.ThemeResources["PreviewText.Link"] as Brush;

                textEditor.TextArea.SelectionForeground = Application.Current.Resources["PreviewText.Selection.Foreground"] as Brush;
                textEditor.TextArea.SelectionBrush = Application.Current.Resources["PreviewText.Selection.Background"] as Brush;
                Pen selectionBorder = new(Application.Current.Resources["PreviewText.Selection.Border"] as Brush, 1.0);
                selectionBorder.Freeze();
                textEditor.TextArea.SelectionBorder = selectionBorder;

                textEditor.TextArea.TextView.CurrentLineBackground = Application.Current.Resources["PreviewText.CurrentLine.Background"] as Brush;
                Pen border = new(Application.Current.Resources["PreviewText.CurrentLine.Border"] as Brush, 1.0);
                border.Freeze();
                textEditor.TextArea.TextView.CurrentLineBorder = border;

                UpdatePositionMarkers();
                textEditor.TextArea.TextView.Redraw();// redraw needed for big ellipsis


                replaceView.TextArea.TextView.LinkTextForegroundBrush = app.ThemeResources["PreviewText.Link"] as Brush;

                replaceView.TextArea.SelectionForeground = Application.Current.Resources["PreviewText.Selection.Foreground"] as Brush;
                replaceView.TextArea.SelectionBrush = Application.Current.Resources["PreviewText.Selection.Background"] as Brush;
                replaceView.TextArea.SelectionBorder = selectionBorder;

                replaceView.TextArea.TextView.CurrentLineBackground = Application.Current.Resources["PreviewText.CurrentLine.Background"] as Brush;
                replaceView.TextArea.TextView.CurrentLineBorder = border;

                SetReplaceText();
                replaceView.TextArea.TextView.Redraw();
            }
        }

        private void SetPreviewText()
        {
            textEditor.Text = "Truncated" + BigEllipsisColorizer.ellipsis + "\r\n\r\nUsers, see the **[main web site](http://dngrep.github.io/)** to download and install dnGrep.\r\n\r\nThis is the source code for dnGrep, a great Windows search utility that allows you to search across text files, Word, Excel and PowerPoint documents, PDFs, and archives using text, regular expression, XPath, and phonetic queries.\r\n\r\nDevelopers, see the [development documentation](https://github.com/dnGrep/dnGrep/wiki/Developer-Documentation) for info, including about the continuous build and release process.\r\n";

            UpdatePositionMarkers();
        }

        private void UpdatePositionMarkers()
        {
            if (textEditor != null && textEditor.ViewportHeight > 0 && !string.IsNullOrEmpty(textEditor.Text))
            {
                textEditor.TextArea.TextView.EnsureVisualLines();
                double trackHeight = textEditor.TextArea.TextView.ActualHeight - 2 * SystemParameters.VerticalScrollBarButtonHeight;
                double documentHeight = textEditor.TextArea.TextView.DocumentHeight;

                ViewModel?.BeginUpdateMarkers();

                foreach (int lineNumber in new int[] { 3, 5 })
                {
                    var linePosition = textEditor.TextArea.TextView.GetVisualTopByDocumentLine(lineNumber);

                    ViewModel?.AddMarker(linePosition, documentHeight, trackHeight, MarkerType.Global);
                }

                foreach (int lineNumber in new int[] { 6, 8 })
                {
                    var linePosition = textEditor.TextArea.TextView.GetVisualTopByDocumentLine(lineNumber);

                    ViewModel?.AddMarker(linePosition, documentHeight, trackHeight, MarkerType.Local);
                }

                ViewModel?.EndUpdateMarkers();
            }
        }

        private void InitializeReplaceControl()
        {
            replaceView.ShowLineNumbers = false; // using custom line numbers

            replaceView.TextArea.SelectionForeground = Application.Current.Resources["PreviewText.Selection.Foreground"] as Brush;
            replaceView.TextArea.SelectionBrush = Application.Current.Resources["PreviewText.Selection.Background"] as Brush;
            Pen selectionBorder = new(Application.Current.Resources["PreviewText.Selection.Border"] as Brush, 1.0);
            selectionBorder.Freeze();
            replaceView.TextArea.SelectionBorder = selectionBorder;
        }

        private void SetReplaceText()
        {
            replaceView.TextArea.LeftMargins.Clear();
            replaceLineNumberMargin = new ReplaceViewLineNumberMargin();

            Line line = (Line)DottedLineMargin.Create();
            replaceView.TextArea.LeftMargins.Insert(0, replaceLineNumberMargin);
            replaceView.TextArea.LeftMargins.Insert(1, line);
            var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = replaceView };
            line.SetBinding(Line.StrokeProperty, lineNumbersForeground);
            replaceLineNumberMargin.SetBinding(Control.ForegroundProperty, lineNumbersForeground);

            List<DiffPiece> diffPieces =
            [
                new DiffPiece("This line is unchanged.", ChangeType.Unchanged, 1),
                new DiffPiece("This line has been deleted.", ChangeType.Deleted, null),
                new DiffPiece("This line has been inserted.", ChangeType.Inserted, 2),
                new DiffPiece("This line is unchanged.", ChangeType.Unchanged, 3),
            ];

            diffPieces[1].SubPieces.Add(new DiffPiece("This line has been ", ChangeType.Unchanged));
            diffPieces[1].SubPieces.Add(new DiffPiece("deleted", ChangeType.Deleted));
            diffPieces[1].SubPieces.Add(new DiffPiece(".", ChangeType.Unchanged));

            diffPieces[2].SubPieces.Add(new DiffPiece("This line has been ", ChangeType.Unchanged));
            diffPieces[2].SubPieces.Add(new DiffPiece("inserted", ChangeType.Inserted));
            diffPieces[2].SubPieces.Add(new DiffPiece(".", ChangeType.Unchanged));

            var lineNumbers = Enumerable.Range(0, diffPieces.Count);

            replaceView.Text = string.Join("\n", diffPieces.Select(x => x.Text));

            replaceLineNumberMargin.DiffLines.Clear();
            replaceLineNumberMargin.DiffLines.AddRange(diffPieces);
            replaceLineNumberMargin.LineNumbers.AddRange(lineNumbers);

            replaceView.TextArea.TextView.BackgroundRenderers.Clear();
            replaceHighlighter = new ReplaceViewHighlighter();
            replaceView.TextArea.TextView.BackgroundRenderers.Add(replaceHighlighter);

            replaceHighlighter.DiffLines.AddRange(diffPieces);
            replaceHighlighter.LineNumbers.AddRange(lineNumbers);

            // recalculate the width of the line number margin
            replaceLineNumberMargin.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        }
    }
}
