using System;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
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
        private readonly PreviewLineNumberMargin lineNumberMargin;

        internal MainViewModel? ViewModel { get; set; }

        public TestControls()
        {
            InitializeComponent();

            textEditor.ShowLineNumbers = false; // using custom line numbers

            lineNumberMargin = new PreviewLineNumberMargin();
            Line line = (Line)DottedLineMargin.Create();
            textEditor.TextArea.LeftMargins.Insert(0, lineNumberMargin);
            textEditor.TextArea.LeftMargins.Insert(1, line);
            var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = textEditor };
            line.SetBinding(Line.StrokeProperty, lineNumbersForeground);
            lineNumberMargin.SetBinding(Control.ForegroundProperty, lineNumbersForeground);

            textEditor.TextArea.TextView.LineTransformers.Add(new BigEllipsisColorizer());

            Loaded += (s, e) => SetText();

            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as MainViewModel;
                if (ViewModel != null)
                {
                    ViewModel.ThemeColorChanged += OnThemeColorChanged;
                }
            };
        }

        private void OnThemeColorChanged(object? sender, EventArgs e)
        {
            textEditor.TextArea.TextView.LinkTextForegroundBrush = Application.Current.Resources["AvalonEdit.Link"] as Brush;
            UpdatePositionMarkers();
        }

        private void SetText()
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
    }
}
