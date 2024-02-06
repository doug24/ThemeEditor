using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeEditor
{
    public partial class BrushResourceViewModel : ObservableObject
    {
        public event EventHandler? SaveBrush;
        public event EventHandler? RevertBrush;

        public BrushResourceViewModel()
        {
            InitializeColors();
        }

        internal void InitializeColors()
        {
            string selectionName = string.Empty;
            if (SelectedResource != null)
            {
                selectionName = SelectedResource.Name;
            }

            List<NamedColor> names = [];
            var dictionary = Application.Current.Resources.MergedDictionaries[0];
            foreach (object key in dictionary.Keys)
            {
                if (dictionary[key] is SolidColorBrush br)
                    names.Add(new NamedColor(key, key.ToString() ?? string.Empty, br.Color));
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

        private void UpdateColorGroup(NamedColor value)
        {
            ColorGroup.Clear();

            if (value != null)
            {
                var group = ResourceColors.Where(r => r.Color == value.Color).ToList();

                foreach (NamedColor item in group)
                {
                    ColorGroup.Add(item);
                }
            }
        }

        internal void Save(Color color)
        {
            if (SelectedResource != null)
            {
                SelectedResource.Color = color;
                SelectedResource.IsModified = true;

                if (SyncColors)
                {
                    foreach (var item in ColorGroup)
                    {
                        item.Color = color;
                        item.IsModified = true;
                    }
                }
                else
                {
                    // revert changes that may have been visible if the
                    // checkbox was unchecked just before save
                    foreach (var item in ColorGroup)
                    {
                        if (Application.Current.Resources[item.Key] is Brush)
                        {
                            Application.Current.Resources[item.Key] = new SolidColorBrush(item.Color);
                        }
                    }
                }

                UpdateColorGroup(SelectedResource);
            }
        }

        /// <summary>
        /// Gets the color currently set by the color selector
        /// </summary>
        public Color CurrentColor { get; internal set; }

        public bool IsColorChanged => SelectedResource != null &&
            SelectedResource.Color != CurrentColor;

        public ObservableCollection<NamedColor> ResourceColors { get; private set; } = [];

        public ObservableCollection<NamedColor> ColorGroup { get; } = [];

        [ObservableProperty]
        private NamedColor? selectedResource = null;

        partial void OnSelectedResourceChanged(NamedColor? value)
        {
            if (value != null)
            {
                UpdateColorGroup(value);
                SelectedBrush = value.Brush;
            }
            else
            {
                SelectedBrush = null;
            }
        }

        [ObservableProperty]
        private SolidColorBrush? selectedBrush = null;

        [ObservableProperty]
        private bool canEdit;

        [ObservableProperty]
        private bool syncColors;

        public ICommand SaveCommand => new RelayCommand(
            p => SaveBrush?.Invoke(this, EventArgs.Empty),
            q => CanEdit && IsColorChanged);

        public ICommand RevertCommand => new RelayCommand(
            p => RevertBrush?.Invoke(this, EventArgs.Empty),
            q => CanEdit && IsColorChanged);
    }
}
