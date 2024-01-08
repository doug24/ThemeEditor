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

            Loaded += (s, e) =>
            {
                double sz = FontSize;
            };
        }

        private void ColorShift_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                DefaultExt = ".xaml",
                CheckFileExists = true,
                InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "dnGrep"),
            };

            if (dlg.ShowDialog(this) ?? false)
            {
                XDocument doc = null;
                using (FileStream fs = File.OpenRead(dlg.FileName))
                {
                    doc = XDocument.Load(fs);
                }

                if (doc != null)
                {
                    foreach (XElement elem in doc.Root.Elements())
                    {
                        XAttribute attr = elem.Attribute("Color");
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

                    string outFile = Path.Combine(Path.GetDirectoryName(dlg.FileName),
                        Path.GetFileNameWithoutExtension(dlg.FileName) + "_out.xaml");

                    using (FileStream fs = File.OpenWrite(outFile))
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            IndentChars = "    ",
                            NewLineChars = Environment.NewLine,
                            Encoding = Encoding.UTF8
                        };

                        using (XmlWriter writer = XmlWriter.Create(fs, settings))
                        {
                            doc.WriteTo(writer);
                        }
                    }
                }
            }
        }
    }
}
