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
using Microsoft.Win32;

namespace ThemeEditor
{
    public class MainViewModel : ViewModelBase
    {
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

            EditThemeName = "(none)";
            EditFile = Properties.Settings.Default.CurrentEditFile;
            if (!string.IsNullOrEmpty(EditFile) && File.Exists(EditFile))
            {
                EditThemeName = Path.GetFileNameWithoutExtension(EditFile);
                LoadXaml();
            }
        }

        public string EditFile { get; private set; }

        public BrushResourceViewModel BrushResourceVM { get; private set; }
        public ColorEditorViewModel ColorEditorVM { get; private set; }

        private void BrushResourceVM_SaveBrush(object sender, EventArgs e)
        {
            if (BrushResourceVM.SelectedBrush != null)
                BrushResourceVM.Save(ColorEditorVM.Color);
        }

        private void BrushResourceVM_RevertBrush(object sender, EventArgs e)
        {
            if (BrushResourceVM.SelectedBrush != null)
                ColorEditorVM.Color = BrushResourceVM.SelectedBrush.Color;
        }

        private void BrushResourceViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedResource" && BrushResourceVM.SelectedResource != null)
            {
                ColorEditorVM.Color = BrushResourceVM.SelectedResource.Color;
            }
        }

        private void ColorEditorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColorEditorVM.ColorBrush))
            {
                if (Application.Current.Resources[BrushResourceVM.SelectedResource.Key] is Brush)
                {
                    Application.Current.Resources[BrushResourceVM.SelectedResource.Key] = ColorEditorVM.ColorBrush;
                }
            }
            else if (e.PropertyName == nameof(ColorEditorVM.Color))
            {
                BrushResourceVM.CurrentColor = ColorEditorVM.Color;
            }
        }



        public ObservableCollection<KeyValuePair<string, int>> Encodings { get; } = new ObservableCollection<KeyValuePair<string, int>>();

        private int codePage = -1;
        public int CodePage
        {
            get { return codePage; }
            set
            {
                if (value == codePage)
                    return;

                codePage = value;

                OnPropertyChanged(() => CodePage);
            }
        }

        private void PopulateEncodings()
        {
            KeyValuePair<string, int> defaultValue = new KeyValuePair<string, int>("Auto detection (default)", -1);

            List<KeyValuePair<string, int>> tempEnc = new List<KeyValuePair<string, int>>();
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

        private string editThemeName = string.Empty;
        public string EditThemeName
        {
            get { return editThemeName; }
            set
            {
                if (value == editThemeName)
                    return;

                editThemeName = value;
                WindowTitle = string.IsNullOrEmpty(editThemeName) ?
                    "Theme Editor" : $"Theme Editor - {editThemeName}";

                OnPropertyChanged(() => EditThemeName);
            }
        }

        private string windowTitle = string.Empty;
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                if (value == windowTitle)
                    return;

                windowTitle = value;

                OnPropertyChanged(() => WindowTitle);
            }
        }

        private bool multiline;
        public bool Multiline
        {
            get { return multiline; }
            set
            {
                if (value == multiline)
                    return;

                multiline = value;

                OnPropertyChanged(() => Multiline);
            }
        }

        private string searchFor = "string";
        public string SearchFor
        {
            get { return searchFor; }
            set
            {
                if (value == searchFor)
                    return;

                searchFor = value;

                OnPropertyChanged(() => SearchFor);
            }
        }

        public ObservableCollection<string> FastSearchBookmarks { get; } = new ObservableCollection<string>
        {
            "text", "unique", "watch", "xeon", "yesterday", "zoom"
        };


        RelayCommand editThemeCommand;
        public ICommand EditThemeCommand
        {
            get
            {
                if (editThemeCommand == null)
                {
                    editThemeCommand = new RelayCommand(
                        p => LoadXaml(),
                        q => !string.IsNullOrEmpty(EditFile) && File.Exists(EditFile));
                }
                return editThemeCommand;
            }
        }

        RelayCommand darkThemeCommand;
        public ICommand DarkThemeCommand
        {
            get
            {
                if (darkThemeCommand == null)
                {
                    darkThemeCommand = new RelayCommand(
                        p => LoadResource("Dark"));
                }
                return darkThemeCommand;
            }
        }

        RelayCommand lightThemeCommand;
        public ICommand LightThemeCommand
        {
            get
            {
                if (lightThemeCommand == null)
                {
                    lightThemeCommand = new RelayCommand(
                        p => LoadResource("Light"));
                }
                return lightThemeCommand;
            }
        }

        RelayCommand openXamlCommand;
        public ICommand OpenXamlCommand
        {
            get
            {
                if (openXamlCommand == null)
                {
                    openXamlCommand = new RelayCommand(
                        p => OpenXaml());
                }
                return openXamlCommand;
            }
        }

        RelayCommand saveXamlCommand;
        public ICommand SaveXamlCommand
        {
            get
            {
                if (saveXamlCommand == null)
                {
                    saveXamlCommand = new RelayCommand(
                        p => SaveXaml(),
                        q => BrushResourceVM.ResourceColors.Any(r => r.IsModified)
                        );
                }
                return saveXamlCommand;
            }
        }

        private void OpenXaml()
        {
            OpenFileDialog dlg = new OpenFileDialog
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
                BrushResourceVM.InitializeColors(false);
                BrushResourceVM.CanEdit = false;

                WindowTitle = $"Theme Editor - {name}";

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

                BrushResourceVM.ResetEditColors();
                BrushResourceVM.CanEdit = true;
                WindowTitle = $"Theme Editor - {EditThemeName}";
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
            List<string> lines = File.ReadAllLines(EditFile).ToList();

            foreach (var namedColor in BrushResourceVM.ResourceColors.Where(nc => nc.IsModified))
            {
                string key = $"x:Key=\"{namedColor.Name}\"";
                Color c = namedColor.Color;
                string color = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                string update = $"    <SolidColorBrush x:Key=\"{namedColor.Name}\" po:Freeze=\"true\" Color=\"{color}\"/>";

                string line = lines.FirstOrDefault(l => l.IndexOf(key, StringComparison.OrdinalIgnoreCase) > -1);
                if (!string.IsNullOrEmpty(line))
                {
                    int index = lines.IndexOf(line);
                    lines.RemoveAt(index);
                    lines.Insert(index, update);
                }
            }

            File.WriteAllLines(EditFile, lines);
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
}