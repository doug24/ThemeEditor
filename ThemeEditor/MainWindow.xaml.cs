using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        private readonly MainViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainViewModel();
            DataContext = vm;

            Rect windowBounds = Rect.Empty;
            var settings = Properties.Settings.Default;
            if (settings.Left != -1 && settings.Top != -1 &&
                settings.Width != -1 && settings.Height != -1)
            {
                // fix for placements on monitors with different DPIs:
                // initial placement on the primary monitor, then move it
                // to the saved location when the window is loaded
                Left = 0;
                Top = 0;
                Width = settings.Width;
                Height = settings.Height;
                WindowState = WindowState.Normal;

                windowBounds = new(
                    settings.Left,
                    settings.Top,
                    settings.Width,
                    settings.Height);
            }

            Loaded += (s, e) =>
            {
                double sz = FontSize;

                if (windowBounds.IsOnScreen())
                {
                    // setting Left and Top does not work when
                    // moving to a monitor with a different DPI
                    // than the primary monitor
                    this.MoveWindow(settings.Left, settings.Top);
                    WindowState = (WindowState)settings.WindowState;

                    TextBoxCommands.BindCommandsToWindow(this);
                }
                else
                {
                    this.CenterWindow();
                }

                this.ConstrainToScreen();
            };

            Closing += (s, e) =>
            {
                e.Cancel = vm.Closing();

                if (!e.Cancel)
                {
                    if (WindowState == WindowState.Normal)
                    {
                        Properties.Settings.Default.Left = Left;
                        Properties.Settings.Default.Top = Top;
                        Properties.Settings.Default.Width = Width;
                        Properties.Settings.Default.Height = Height;
                    }
                    else
                    {
                        Properties.Settings.Default.Left = RestoreBounds.Left;
                        Properties.Settings.Default.Top = RestoreBounds.Top;
                        Properties.Settings.Default.Width = RestoreBounds.Width;
                        Properties.Settings.Default.Height = RestoreBounds.Height;
                    }
                    Properties.Settings.Default.WindowState = (int)WindowState.Normal;
                    if (WindowState == WindowState.Maximized)
                        Properties.Settings.Default.WindowState = (int)WindowState.Maximized;

                    Properties.Settings.Default.Save();
                }
            };
        }

        private void ColorShift_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                DefaultExt = ".xaml",
                CheckFileExists = true,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dnGrep"),
            };

            if (dlg.ShowDialog(this) ?? false)
            {
                XDocument? doc = null;
                using (FileStream fs = File.OpenRead(dlg.FileName))
                {
                    doc = XDocument.Load(fs);
                }

                if (doc != null && doc.Root != null)
                {
                    foreach (XElement elem in doc.Root.Elements())
                    {
                        XAttribute? attr = elem.Attribute("Color");
                        if (attr != null)
                        {
                            string hex = attr.Value;
                            if (ColorConverter.ConvertFromString(hex) is Color color)
                            {
                                ColorHSV hsv = ColorHSV.ConvertFrom(color);

                                if (hsv.Hue >= 0.5 && hsv.Hue <= 0.67)
                                {
                                    double val = hsv.Hue + 0.4167;
                                    if (val < 0) val += 1.0;
                                    if (val > 1) val -= 1.0;
                                    hsv.Hue = val;

                                    Color mod = hsv.ToColor();
                                    attr.Value = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", mod.A, mod.R, mod.G, mod.B);
                                }
                            }
                        }
                    }

                    string outFile = Path.Combine(Path.GetDirectoryName(dlg.FileName) ?? string.Empty,
                        Path.GetFileNameWithoutExtension(dlg.FileName) + "_out.xaml");

                    using FileStream fs = File.OpenWrite(outFile);
                    XmlWriterSettings settings = new()
                    {
                        Indent = true,
                        IndentChars = "    ",
                        NewLineChars = "\n",
                        Encoding = Encoding.UTF8
                    };

                    using XmlWriter writer = XmlWriter.Create(fs, settings);
                    doc.WriteTo(writer);
                }
            }
        }

        private void Close_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
