using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace ThemeEditor
{
    public partial class MainViewModel : ObservableObject
    {
        private const string Indent = "    ";
        private const string Newline = "\r\n";
        public EventHandler? ThemeColorChanged;

        public MainViewModel()
        {
            ThemeResourceVM = new ThemeResourceViewModel();
            BrushEditorVM = new BrushEditorViewModel();

            ThemeResourceVM.PropertyChanged += ThemeResourceViewModel_PropertyChanged;
            ThemeResourceVM.SaveBrush += ThemeResourceVM_SaveBrush;
            ThemeResourceVM.RevertBrush += ThemeResourceVM_RevertBrush;
            BrushEditorVM.PropertyChanged += BrushEditorViewModel_PropertyChanged;

            PopulateEncodings();
            PopulateTreeData();

            AddMarker(10, 40, 40, MarkerType.Local);

            EditThemeName = "(none)";
            EditFile = Properties.Settings.Default.CurrentEditFile;
            if (!string.IsNullOrEmpty(EditFile) && File.Exists(EditFile))
            {
                EditThemeName = Path.GetFileNameWithoutExtension(EditFile);
                LoadXaml();
            }
            else
            {
                EditFile = string.Empty;
                Properties.Settings.Default.CurrentEditFile = string.Empty;
                Properties.Settings.Default.Save();
            }
        }

        public string EditFile { get; private set; }

        public ThemeResourceViewModel ThemeResourceVM { get; private set; }
        public BrushEditorViewModel BrushEditorVM { get; private set; }

        private void ThemeResourceVM_SaveBrush(object? sender, EventArgs e)
        {
            if (ThemeResourceVM.SelectedResource != null)
            {
                if (BrushEditorVM.BrushType == BrushType.SolidColorBrush)
                {
                    ThemeResourceVM.Save(BrushEditorVM.ColorBrush);
                }
                else if (BrushEditorVM.BrushType == BrushType.LinearGradientBrush)
                {
                    ThemeResourceVM.Save(BrushEditorVM.GradientBrush);
                }
                else if (BrushEditorVM.BrushType == BrushType.DropShadowEffect)
                {
                    ThemeResourceVM.Save(BrushEditorVM.DropShadowEffect);
                }

                BrushEditorVM.InitializeThemeBrushes();
            }
        }

        private void ThemeResourceVM_RevertBrush(object? sender, EventArgs e)
        {
            if (ThemeResourceVM.SelectedResource != null)
            {
                if (ThemeResourceVM.SelectedResource.BrushType == BrushType.SolidColorBrush)
                {
                    BrushEditorVM.ColorBrush = ThemeResourceVM.SelectedResource.ColorBrush;
                    ThemeResourceVM.SelectedResource.HasPendingChange = false;
                }
                else if (ThemeResourceVM.SelectedResource.BrushType == BrushType.LinearGradientBrush)
                {
                    BrushEditorVM.GradientBrush = ThemeResourceVM.SelectedResource.GradientBrush;
                    ThemeResourceVM.SelectedResource.HasPendingChange = false;
                }
                else if (ThemeResourceVM.SelectedResource.BrushType == BrushType.DropShadowEffect)
                {
                    BrushEditorVM.DropShadowEffect = ThemeResourceVM.SelectedResource.DropShadowEffect;
                    ThemeResourceVM.SelectedResource.HasPendingChange = false;
                }

                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ThemeResourceViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeResourceViewModel.SelectedResource) &&
                ThemeResourceVM.SelectedResource != null)
            {
                // disconnect handler to break feedback loop from resource selection
                BrushEditorVM.PropertyChanged -= BrushEditorViewModel_PropertyChanged;

                BrushEditorVM.SelectedName = ThemeResourceVM.SelectedResource.Name;
                if (ThemeResourceVM.SelectedResource.BrushType == BrushType.SolidColorBrush)
                {
                    BrushEditorVM.ColorBrush = ThemeResourceVM.SelectedResource.ColorBrush;
                }
                else if (ThemeResourceVM.SelectedResource.BrushType == BrushType.LinearGradientBrush)
                {
                    BrushEditorVM.GradientBrush = ThemeResourceVM.SelectedResource.GradientBrush;
                }
                else if (ThemeResourceVM.SelectedResource.BrushType == BrushType.DropShadowEffect)
                {
                    BrushEditorVM.DropShadowEffect = ThemeResourceVM.SelectedResource.DropShadowEffect;
                }

                BrushEditorVM.PropertyChanged += BrushEditorViewModel_PropertyChanged;
                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BrushEditorViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (ThemeResourceVM.SelectedResource != null &&
                Application.Current is App app)
            {
                if (e.PropertyName == nameof(BrushEditorVM.ColorBrush))
                {
                    if (app.ThemeResources[ThemeResourceVM.SelectedResource.Name] is Brush)
                    {
                        ThemeResourceVM.SelectedResource.HasPendingChange = true;
                        app.ThemeResources[ThemeResourceVM.SelectedResource.Name] = BrushEditorVM.ColorBrush;
                    }

                    if (ThemeResourceVM.SyncColors)
                    {
                        foreach (var item in ThemeResourceVM.ColorGroup)
                        {
                            if (app.ThemeResources[item.Name] is Brush)
                            {
                                item.HasPendingChange = true;
                                app.ThemeResources[item.Name] = BrushEditorVM.ColorBrush;
                            }
                        }
                    }

                    ThemeColorChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (e.PropertyName == nameof(BrushEditorVM.GradientBrush))
                {
                    if (app.ThemeResources[ThemeResourceVM.SelectedResource.Name] is Brush)
                    {
                        ThemeResourceVM.SelectedResource.HasPendingChange = true;
                        app.ThemeResources[ThemeResourceVM.SelectedResource.Name] = BrushEditorVM.GradientBrush;
                    }

                    ThemeColorChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (e.PropertyName == nameof(BrushEditorVM.DropShadowEffect))
                {
                    if (app.ThemeResources[ThemeResourceVM.SelectedResource.Name] is DropShadowEffect)
                    {
                        ThemeResourceVM.SelectedResource.HasPendingChange = true;
                        app.ThemeResources[ThemeResourceVM.SelectedResource.Name] = BrushEditorVM.DropShadowEffect;
                    }
                }
            }
        }



        public ObservableCollection<KeyValuePair<string, int>> Encodings { get; } = [];

        [ObservableProperty]
        private int codePage = -1;

        private void PopulateEncodings()
        {
            KeyValuePair<string, int> defaultValue = new("Auto detection (default)", -1);

            List<KeyValuePair<string, int>> tempEnc = [];
            foreach (EncodingInfo ei in Encoding.GetEncodings())
            {
                Encoding e = ei.GetEncoding();
                tempEnc.Add(new KeyValuePair<string, int>(e.EncodingName, e.CodePage));
            }

            tempEnc.Insert(0, defaultValue);
            Encodings.Clear();
            foreach (var enc in tempEnc)
                Encodings.Add(enc);
        }

        public ObservableCollection<FileNode> TreeData { get; } = [];

        private void PopulateTreeData()
        {
            TreeData.Add(new FileNode("Star Trek (3 matches on 2 lines)", true));
            TreeData[0].Children.Add(new LineNode("Space, the final frontier.", "1"));
            TreeData[0].Children.Add(new LineNode("These are the voyages of the Starship Enterprise.", "2"));
            TreeData[0].Children.Add(new LineNode("to boldly go where no one has gone before!", "5", true));
        }

        [ObservableProperty]
        private string editThemeName = string.Empty;
        partial void OnEditThemeNameChanged(string value)
        {
            WindowTitle = string.IsNullOrEmpty(value) ?
                "Theme Editor" : $"Theme Editor - {value}";
        }

        [ObservableProperty]
        private string windowTitle = string.Empty;

        [ObservableProperty]
        private bool multiline;

        [ObservableProperty]
        private string searchFor = "vanilla";

        public ObservableCollection<MRUViewModel> FastSearchBookmarks { get; } =
        [
            new MRUViewModel(MRUType.SearchFor, "text"),
            new MRUViewModel(MRUType.SearchFor, "unique"),
            new MRUViewModel(MRUType.SearchFor, "vanilla"),
            new MRUViewModel(MRUType.SearchFor, "watch"),
            new MRUViewModel(MRUType.SearchFor, "xenon"),
            new MRUViewModel(MRUType.SearchFor, "yesterday"),
            new MRUViewModel(MRUType.SearchFor, "zoom"),
            new MRUViewModel(MRUType.SearchFor, "disabled"),
        ];

        [ObservableProperty]
        private string fileOrFolderPath = @"C:\files\to\search";
        public ObservableCollection<MRUViewModel> FastPathBookmarks { get; } =
        [
            new MRUViewModel(MRUType.SearchPath, @"C:\files\to\search"),
            new MRUViewModel(MRUType.SearchPath, @"C:\Users\user\Documents"),
        ];

        [ObservableProperty]
        private string imDisabled = "disabled";

        public ObservableCollection<string> DisabledList { get; } = ["disabled"];

        public static ICommand DeleteMRUItemCommand => new RelayCommand(
            p => { },
            q => true);

        // dummy data for DataGrid
        public record Student(string FirstName, string LastName, string Email) { }

        public ObservableCollection<Student> Students { get; } =
        [
            new Student("Holly", "Perrinchief", "hperrinchief4@prlog.org"),
            new Student("Booth", "Hamil", "bhamil9@squidoo.com"),
            new Student("Livvyy", "Cornall", "lcornall0@google.com.au")
        ];

        public ObservableCollection<Marker> Markers { get; } = [];

        internal void BeginUpdateMarkers()
        {
            Markers.Clear();
        }

        internal void AddMarker(double linePosition, double documentHeight, double trackHeight, MarkerType markerType)
        {
            double position = (documentHeight < trackHeight) ? linePosition : linePosition * trackHeight / documentHeight;
            Markers.Add(new Marker(position, markerType));
        }

        internal void EndUpdateMarkers()
        {
            OnPropertyChanged(nameof(Markers));
        }


        public ICommand EditThemeCommand => new RelayCommand(
            p => LoadXaml(),
            q => !string.IsNullOrEmpty(EditFile) && File.Exists(EditFile));

        public ICommand DarkThemeCommand => new RelayCommand(
            p => LoadResource("Dark"));

        public ICommand LightThemeCommand => new RelayCommand(
            p => LoadResource("Light"));

        public ICommand ImportVSCodeCommand => new RelayCommand(
            p => ImportVSCode(p as string));

        public ICommand OpenXamlCommand => new RelayCommand(
            p => OpenXaml());

        public ICommand SaveXamlCommand => new RelayCommand(
            p => SaveXaml(),
            q => ThemeResourceVM.IsModified);

        private void OpenXaml()
        {
            OpenFileDialog dlg = new()
            {
                Filter = "Xaml files|*.xaml",
                DefaultExt = ".xaml",
                AddExtension = true,
                CheckFileExists = true,
            };

            if (dlg.ShowDialog() ?? false)
            {
                EditFile = dlg.FileName;
                EditThemeName = Path.GetFileNameWithoutExtension(EditFile);

                LoadXaml();
                Properties.Settings.Default.CurrentEditFile = EditFile;
                Properties.Settings.Default.Save();
            }
        }

        private void ImportVSCode(string? input)
        {
            if (ThemeResourceVM.IsModified)
            {
                var ans = MessageBox.Show("Data has been changed - save changes?", "Theme Editor",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ans == MessageBoxResult.Yes)
                {
                    SaveXaml();
                }
            }

            if (!string.IsNullOrEmpty(input))
            {
                string name = VSCodeTheme.LoadFile(input);
                if (!string.IsNullOrEmpty(name))
                {
                    ThemeResourceVM.InitializeColors();
                    ThemeResourceVM.CanEdit = true;
                    BrushEditorVM.InitializeThemeBrushes();

                    WindowTitle = $"Theme Editor - {name}";
                    ThemeColorChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                OpenFileDialog dlg = new()
                {
                    Filter = "Jsonc files|*.jsonc",
                    DefaultExt = "jsonc",
                    CheckFileExists = true,
                };

                var result = dlg.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    string themeFile = VSCodeTheme.NewTheme(dlg.FileName);
                    if (!string.IsNullOrEmpty(themeFile))
                    {
                        EditFile = themeFile;
                        EditThemeName = Path.GetFileNameWithoutExtension(EditFile);

                        LoadXaml(false);
                        Properties.Settings.Default.CurrentEditFile = EditFile;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private void LoadResource(string name)
        {
            if (ThemeResourceVM.IsModified)
            {
                var ans = MessageBox.Show("Data has been changed - save changes?", "Theme Editor",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ans == MessageBoxResult.Yes)
                {
                    SaveXaml();
                }
            }

            if ((name.Equals("Dark") || name.Equals("Light")) &&
                Application.Current is App app)
            {
                app.ThemeResources.Clear();
                app.ThemeResources.Source = new Uri($"/Themes/{name}Brushes.xaml", UriKind.Relative);

                ThemeResourceVM.InitializeColors();
                ThemeResourceVM.CanEdit = false;
                BrushEditorVM.InitializeThemeBrushes();

                WindowTitle = $"Theme Editor - {name}";
                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void LoadXaml(bool checkModified = true)
        {
            if (checkModified && ThemeResourceVM.IsModified)
            {
                var ans = MessageBox.Show("Data has been changed - save changes?", "Theme Editor",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ans == MessageBoxResult.Yes)
                {
                    SaveXaml();
                }
            }

            try
            {
                if (Application.Current is App app)
                {
                    app.ThemeResources.Clear();
                    app.ThemeResources.Source = new Uri(EditFile, UriKind.Absolute);

                    ThemeResourceVM.InitializeColors();
                    ThemeResourceVM.CanEdit = true;
                    BrushEditorVM.InitializeThemeBrushes();

                    WindowTitle = $"Theme Editor - {EditThemeName}";
                    ThemeColorChanged?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                LoadResource("Dark");
                MessageBox.Show($"Error loading xaml file {EditFile} : {ex.Message}",
                    "Theme Editor", MessageBoxButton.OK, MessageBoxImage.Error);

                ThemeResourceVM.CanEdit = false;
            }
        }

        private void SaveXaml()
        {
            XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
            XNamespace sys = "clr-namespace:System;assembly=mscorlib";
            XNamespace mc = "http://schemas.openxmlformats.org/markup-compatibility/2006";
            XNamespace po = "http://schemas.microsoft.com/winfx/2006/xaml/presentation/options";

            XDocument doc = XDocument.Load(EditFile, LoadOptions.PreserveWhitespace);

            if (doc != null && doc.Root != null)
            {
                foreach (ThemeBrush themeBrush in ThemeResourceVM.ResourceBrushes.Where(nc => nc.IsModified))
                {
                    XElement? original = doc.Root.Elements()
                        .FirstOrDefault(el => el.Attribute(x + "Key")?.Value == themeBrush.Name);

                    if (original != null)
                    {
                        if (themeBrush.BrushType == BrushType.SolidColorBrush)
                        {
                            Color c = themeBrush.ColorBrush.Color;
                            string color = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);

                            XElement elem = new(ns + "SolidColorBrush");
                            elem.Add(new XAttribute(x + "Key", themeBrush.Name));
                            elem.Add(new XAttribute(po + "Freeze", true));
                            elem.Add(new XAttribute("Color", color));

                            original.ReplaceWith(elem);
                        }
                        else if (themeBrush.BrushType == BrushType.LinearGradientBrush)
                        {
                            string startPoint = themeBrush.GradientBrush.StartPoint.ToString(CultureInfo.InvariantCulture);
                            string endPoint = themeBrush.GradientBrush.EndPoint.ToString(CultureInfo.InvariantCulture);
                            string opacity = themeBrush.GradientBrush.Opacity.ToString(CultureInfo.InvariantCulture);

                            XElement elem = new(ns + "LinearGradientBrush");
                            elem.Add(new XAttribute(x + "Key", themeBrush.Name));
                            elem.Add(new XAttribute(po + "Freeze", true));
                            elem.Add(new XAttribute("StartPoint", startPoint));
                            elem.Add(new XAttribute("EndPoint", endPoint));
                            if (themeBrush.GradientBrush.Opacity != 1.0)
                                elem.Add(new XAttribute("Opacity", opacity));

                            foreach (GradientStop stop in themeBrush.GradientBrush.GradientStops)
                            {
                                Color c = stop.Color;
                                string color = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                                string offset = stop.Offset.ToString(CultureInfo.InvariantCulture);

                                XElement stopElem = new(ns + "GradientStop");
                                stopElem.Add(new XAttribute(po + "Freeze", true));
                                stopElem.Add(new XAttribute("Color", color));
                                stopElem.Add(new XAttribute("Offset", offset));

                                elem.Add(Newline + Indent + Indent);
                                elem.Add(stopElem);
                            }
                            elem.Add(Newline + Indent);

                            original.ReplaceWith(elem);
                        }
                        else if (themeBrush.BrushType == BrushType.DropShadowEffect)
                        {
                            Color c = themeBrush.DropShadowEffect.Color;
                            string color = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                            string blur = themeBrush.DropShadowEffect.BlurRadius.ToString(CultureInfo.InvariantCulture);
                            string depth = themeBrush.DropShadowEffect.ShadowDepth.ToString(CultureInfo.InvariantCulture);
                            string opacity = themeBrush.DropShadowEffect.Opacity.ToString(CultureInfo.InvariantCulture);
                            string direction = themeBrush.DropShadowEffect.Direction.ToString(CultureInfo.InvariantCulture);

                            XElement elem = new(ns + "DropShadowEffect");
                            elem.Add(new XAttribute(x + "Key", themeBrush.Name));
                            elem.Add(new XAttribute(po + "Freeze", true));
                            if (themeBrush.DropShadowEffect.ShadowDepth != 5.0)
                                elem.Add(new XAttribute("ShadowDepth", depth));
                            if (themeBrush.DropShadowEffect.BlurRadius != 5.0)
                                elem.Add(new XAttribute("BlurRadius", blur));
                            if (themeBrush.DropShadowEffect.Opacity != 1.0)
                                elem.Add(new XAttribute("Opacity", opacity));
                            if (themeBrush.DropShadowEffect.Direction != 315.0)
                                elem.Add(new XAttribute("Direction", direction));
                            elem.Add(new XAttribute("Color", color));

                            original.ReplaceWith(elem);
                        }
                    }

                    themeBrush.IsModified = false;
                    themeBrush.HasPendingChange = false;
                }

                if (Application.Current is App app)
                {
                    if (ThemeResourceVM.ButtonImageFlagChanged)
                    {
                        XElement? original = doc.Root.Elements().FirstOrDefault(
                            el => el.Attribute(x + "Key")?.Value == ThemeResourceViewModel.ButtonImageFlagKey);
                        if (original != null)
                        {
                            original.Value = ThemeResourceVM.ButtonImageFlag.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
                            app.ThemeResources[ThemeResourceViewModel.ButtonImageFlagKey] = ThemeResourceVM.ButtonImageFlag;
                        }
                    }
                    if (ThemeResourceVM.DateCalendarFlagChanged)
                    {
                        XElement? original = doc.Root.Elements().FirstOrDefault(
                            el => el.Attribute(x + "Key")?.Value == ThemeResourceViewModel.DateCalendarFlagKey);
                        if (original != null)
                        {
                            original.Value = ThemeResourceVM.DateCalendarFlag.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
                            app.ThemeResources[ThemeResourceViewModel.DateCalendarFlagKey] = ThemeResourceVM.DateCalendarFlag;
                        }
                    }
                    if (ThemeResourceVM.SyntaxColorFlagChanged)
                    {
                        XElement? original = doc.Root.Elements().FirstOrDefault(
                            el => el.Attribute(x + "Key")?.Value == ThemeResourceViewModel.SyntaxColorFlagKey);
                        if (original != null)
                        {
                            original.Value = ThemeResourceVM.SyntaxColorFlag.ToString(CultureInfo.InvariantCulture).ToLowerInvariant();
                            app.ThemeResources[ThemeResourceViewModel.SyntaxColorFlagKey] = ThemeResourceVM.SyntaxColorFlag;
                        }
                    }
                }

                XmlWriterSettings settings = new()
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    IndentChars = Indent,
                    NewLineChars = Newline,
                    OmitXmlDeclaration = true,
                };

                using XmlWriter writer = XmlWriter.Create(EditFile, settings);
                doc.Save(writer);
            }
        }

        internal bool Closing()
        {
            if (ThemeResourceVM.IsModified)
            {
                var ans = MessageBox.Show("Data has been changed - save changes?", "Theme Editor",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Yes);

                if (ans == MessageBoxResult.Yes)
                {
                    SaveXaml();
                }

                if (ans == MessageBoxResult.Cancel)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public enum MarkerType { Global, Local }

    public class Marker(double position, MarkerType markerType)
    {
        public double Position { get; private set; } = position;
        public MarkerType MarkerType { get; private set; } = markerType;
    }

    public class FileNode(string text, bool isExpanded)
    {
        public ObservableCollection<LineNode> Children { get; } = [];
        public string Text { get; } = text;
        public bool IsExpanded { get; set; } = isExpanded;
    }

    public class LineNode(string text, string lineNumber, bool isSectionBreak = false)
    {
        public ObservableCollection<LineNode> Children { get; } = [];
        public string Text { get; } = text;
        public string LineNumber { get; } = lineNumber;
        public bool IsExpanded { get; set; } = false;
        public bool IsSectionBreak { get; set; } = isSectionBreak;
    }


}