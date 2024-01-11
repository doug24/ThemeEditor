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
            InitializeColors();
        }

        internal void InitializeColors()
        {
            string selectionName = string.Empty;
            if (selectedResource != null)
            {
                selectionName = selectedResource.Name;
            }

                List<NamedColor> names = new List<NamedColor>();
                var dictionary = Application.Current.Resources.MergedDictionaries[0];
                foreach (object key in dictionary.Keys)
                {
                    if (dictionary[key] is SolidColorBrush br)
                        names.Add(new NamedColor(key, key.ToString(), br.Color));
                }

                ResourceColors = new ObservableCollection<NamedColor>(names.OrderBy(nc => nc.Name));
                OnPropertyChanged(nameof(ResourceColors));


            bool set = false;
            if (!string.IsNullOrEmpty(selectionName))
            {
                var item = ResourceColors.FirstOrDefault(r => r.Name == selectionName);

                if (item != null)
                {
                    SelectedResource = item;
                    set = true;
                }
            }
            
            if (!set && ResourceColors.Count > 0)
            {
                SelectedResource = ResourceColors[0];
            }
        }

        internal void Save(Color color)
        {
            if (SelectedResource != null)
            {
                SelectedResource.Color = color;
                SelectedResource.IsModified = true;
            }
        }

        /// <summary>
        /// Gets the color currently set by the color selector
        /// </summary>
        public Color CurrentColor { get; internal set; }

        public bool IsColorChanged => SelectedResource != null &&
            SelectedResource.Color != CurrentColor;

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

        private bool canEdit;
        public bool CanEdit
        {
            get { return canEdit; }
            set
            {
                if (value == canEdit)
                    return;

                canEdit = value;

                OnPropertyChanged(() => CanEdit);
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
                        q => CanEdit && IsColorChanged
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
                        p => RevertBrush?.Invoke(this, EventArgs.Empty),
                        q => CanEdit && IsColorChanged
                        );
                }
                return revertCommand;
            }
        }
    }
}
