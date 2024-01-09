using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ThemeEditor
{
    public class BrushResourceViewModel : ViewModelBase
    {
        public event EventHandler SaveBrush;
        public event EventHandler RevertBrush;

        public BrushResourceViewModel()
        {
            List<NamedColor> names = new List<NamedColor>();
            var dictionary = Application.Current.Resources.MergedDictionaries[0];
            foreach (object key in dictionary.Keys)
            {
                if (dictionary[key] is SolidColorBrush br)
                    names.Add(new NamedColor(key, key.ToString(), br.Color));
            }

            ResourceColors = new ObservableCollection<NamedColor>(names.OrderBy(nc => nc.Name));

            if (ResourceColors.Count > 0)
                SelectedResource = ResourceColors[0];
        }

        internal void Save(Color color)
        {
            if (SelectedResource != null)
            {
                SelectedResource.Color = color;
                SelectedResource.IsModified = true;
            }
        }

        public ObservableCollection<NamedColor> ResourceColors { get; private set; }


        private NamedColor selectedResource = null;
        public NamedColor SelectedResource
        {
            get { return selectedResource; }
            set
            {
                if (selectedResource == value)
                    return;

                selectedResource = value;

                if (selectedResource != null)
                    SelectedBrush = selectedResource.Brush;
                else
                    SelectedBrush = null;

                OnPropertyChanged("SelectedResource");
            }
        }

        private SolidColorBrush selectedBrush = null;
        public SolidColorBrush SelectedBrush
        {
            get { return selectedBrush; }
            set
            {
                if (selectedBrush == value)
                    return;

                selectedBrush = value;
                OnPropertyChanged("SelectedBrush");
            }
        }

        RelayCommand saveCommand;
        public ICommand SaveCommand
        {
            get
            {
                if (saveCommand == null)
                {
                    saveCommand = new RelayCommand(
                        p => SaveBrush?.Invoke(this, EventArgs.Empty),
                        q => SelectedResource != null
                        );
                }
                return saveCommand;
            }
        }

        RelayCommand revertCommand;
        public ICommand RevertCommand
        {
            get
            {
                if (revertCommand == null)
                {
                    revertCommand = new RelayCommand(
                        p => RevertBrush?.Invoke(this, EventArgs.Empty)
                        );
                }
                return revertCommand;
            }
        }

        RelayCommand exportCommand;
        public ICommand ExportCommand
        {
            get
            {
                if (exportCommand == null)
                {
                    exportCommand = new RelayCommand(
                        p => Export(),
                        q => ResourceColors.Any(nc => nc.IsModified)
                        );
                }
                return exportCommand;
            }
        }

        private void Export()
        {
            List<string> changes = new List<string>();
            foreach (var namedColor in ResourceColors.Where(nc => nc.IsModified))
            {
                Color c = namedColor.Color;
                string color = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", c.A, c.R, c.G, c.B);
                changes.Add($"<SolidColorBrush x:Key=\"{namedColor.Name}\" po:Freeze=\"true\" Color=\"{color}\"/>");
            }
            Clipboard.SetText(string.Join(Environment.NewLine, changes));
        }
    }
}
