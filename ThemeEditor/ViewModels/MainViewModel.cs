using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;

namespace ThemeEditor
{
    public partial class MainViewModel : ObservableObject
    {
        public EventHandler? ThemeColorChanged;

        public MainViewModel()
        {
            BrushResourceVM = new BrushResourceViewModel();
            ColorEditorVM = new ColorEditorViewModel();

            if (BrushResourceVM.SelectedBrush != null)
                ColorEditorVM.Color = BrushResourceVM.SelectedBrush.Color;

            BrushResourceVM.PropertyChanged += BrushResourceViewModel_PropertyChanged;
            BrushResourceVM.SaveBrush += BrushResourceVM_SaveBrush;
            BrushResourceVM.RevertBrush += BrushResourceVM_RevertBrush;
            ColorEditorVM.PropertyChanged += ColorEditorViewModel_PropertyChanged;

            PopulateEncodings();

            //AddMarker(3, 80, 40, MarkerType.Global);
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

        public BrushResourceViewModel BrushResourceVM { get; private set; }
        public ColorEditorViewModel ColorEditorVM { get; private set; }

        private void BrushResourceVM_SaveBrush(object? sender, EventArgs e)
        {
            if (BrushResourceVM.SelectedBrush != null)
            {
                BrushResourceVM.Save(ColorEditorVM.Color);
            }
        }

        private void BrushResourceVM_RevertBrush(object? sender, EventArgs e)
        {
            if (BrushResourceVM.SelectedBrush != null)
            {
                ColorEditorVM.Color = BrushResourceVM.SelectedBrush.Color;
                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BrushResourceViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedResource" && BrushResourceVM.SelectedResource != null)
            {
                ColorEditorVM.Color = BrushResourceVM.SelectedResource.Color;
                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void ColorEditorViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (BrushResourceVM.CanEdit)
            {
                if (e.PropertyName == nameof(ColorEditorVM.ColorBrush))
                {
                    if (Application.Current.Resources[BrushResourceVM.SelectedResource?.Key] is Brush)
                    {
                        Application.Current.Resources[BrushResourceVM.SelectedResource?.Key] = ColorEditorVM.ColorBrush;
                    }

                    if (BrushResourceVM.SyncColors)
                    {
                        foreach (var item in BrushResourceVM.ColorGroup)
                        {
                            if (Application.Current.Resources[item.Key] is Brush)
                            {
                                Application.Current.Resources[item.Key] = ColorEditorVM.ColorBrush;
                            }
                        }
                    }

                    ThemeColorChanged?.Invoke(this, EventArgs.Empty);
                }
                else if (e.PropertyName == nameof(ColorEditorVM.Color))
                {
                    BrushResourceVM.CurrentColor = ColorEditorVM.Color;
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
        private string searchFor = "string";

        public ObservableCollection<string> FastSearchBookmarks { get; } =
        [
            "text", "unique", "watch", "xeon", "yesterday", "zoom"
        ];

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

        public ICommand OpenXamlCommand => new RelayCommand(
            p => OpenXaml());

        public ICommand SaveXamlCommand => new RelayCommand(
            p => SaveXaml(),
            q => BrushResourceVM.ResourceColors.Any(r => r.IsModified));

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

        private void LoadResource(string name)
        {
            if (BrushResourceVM.ResourceColors.Any(r => r.IsModified))
            {
                var ans = MessageBox.Show("Data has been changed - save changes?", "Theme Editor",
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (ans == MessageBoxResult.Yes)
                {
                    SaveXaml();
                }
            }

            if (name.Equals("Dark") || name.Equals("Light"))
            {
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries[0].Source = new Uri($"/Themes/{name}Brushes.xaml", UriKind.Relative);

                BrushResourceVM.InitializeColors();
                BrushResourceVM.CanEdit = false;
                ColorEditorVM.InitializeThemeColors();

                WindowTitle = $"Theme Editor - {name}";
                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void LoadXaml()
        {
            if (BrushResourceVM.ResourceColors.Any(r => r.IsModified))
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
                Application.Current.Resources.Clear();
                Application.Current.Resources.MergedDictionaries[0].Source =
                    new Uri(EditFile, UriKind.Absolute);

                BrushResourceVM.InitializeColors();
                BrushResourceVM.CanEdit = true;
                ColorEditorVM.InitializeThemeColors();

                WindowTitle = $"Theme Editor - {EditThemeName}";
                ThemeColorChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LoadResource("Dark");
                MessageBox.Show($"Error loading xaml file {EditFile} : {ex.Message}",
                    "Theme Editor", MessageBoxButton.OK, MessageBoxImage.Error);

                BrushResourceVM.CanEdit = false;
            }
        }

        private void SaveXaml()
        {
            List<string> lines = [.. File.ReadAllLines(EditFile)];

            foreach (var namedColor in BrushResourceVM.ResourceColors.Where(nc => nc.IsModified))
            {
                string key = $"x:Key=\"{namedColor.Name}\"";
                Color c = namedColor.Color;
                string color = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                string update = $"    <SolidColorBrush x:Key=\"{namedColor.Name}\" po:Freeze=\"true\" Color=\"{color}\"/>";

                string? line = lines.FirstOrDefault(l => l.IndexOf(key, StringComparison.OrdinalIgnoreCase) > -1);
                if (!string.IsNullOrEmpty(line))
                {
                    int index = lines.IndexOf(line);
                    lines.RemoveAt(index);
                    lines.Insert(index, update);
                }
            }

            File.WriteAllLines(EditFile, lines);

            var list = BrushResourceVM.ResourceColors.Where(nc => nc.IsModified).ToList();
            foreach (var item in list)
            {
                item.IsModified = false;
            }
        }

        internal bool Closing()
        {
            if (BrushResourceVM.ResourceColors.Any(r => r.IsModified))
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
}