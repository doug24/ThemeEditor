using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

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
        }

        public BrushResourceViewModel BrushResourceVM { get; private set; }
        public ColorEditorViewModel ColorEditorVM { get; private set; }

        private void BrushResourceVM_SaveBrush(object sender, System.EventArgs e)
        {
            if (BrushResourceVM.SelectedBrush != null)
                BrushResourceVM.Save(ColorEditorVM.Color);
        }

        private void BrushResourceVM_RevertBrush(object sender, System.EventArgs e)
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
            if (e.PropertyName == "ColorBrush")
            {
                if (Application.Current.Resources[BrushResourceVM.SelectedResource.Key] is Brush)
                {
                    Application.Current.Resources[BrushResourceVM.SelectedResource.Key] = ColorEditorVM.ColorBrush;
                }
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

    }
}