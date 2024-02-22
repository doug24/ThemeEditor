﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeEditor
{
    public partial class ThemeResourceViewModel : ObservableObject
    {
        public event EventHandler? SaveBrush;
        public event EventHandler? RevertBrush;

        public ThemeResourceViewModel()
        {
            InitializeColors();
        }

        public bool IsModified => ResourceBrushes.Any(r => r.IsModified) ||
            ButtonImageFlagChanged || SyntaxColorFlagChanged;

        internal void InitializeColors()
        {
            string selectionName = string.Empty;
            if (SelectedResource != null)
            {
                selectionName = SelectedResource.Name;
            }

            List<ThemeBrush> brushes = [];
            var dictionary = Application.Current.Resources.MergedDictionaries[0];
            foreach (object key in dictionary.Keys)
            {
                if (dictionary[key] is SolidColorBrush solidBrush)
                    brushes.Add(new ThemeBrush(key.ToString() ?? string.Empty, solidBrush));
                else if (dictionary[key] is LinearGradientBrush gradientBrush)
                    brushes.Add(new ThemeBrush(key.ToString() ?? string.Empty, gradientBrush));
                else if (dictionary[key] is DropShadowEffect dropShadowEffect)
                    brushes.Add(new ThemeBrush(key.ToString() ?? string.Empty, dropShadowEffect));
            }

            ResourceBrushes = new ObservableCollection<ThemeBrush>(brushes.OrderBy(nc => nc.Name));
            OnPropertyChanged(nameof(ResourceBrushes));


            bool set = false;
            if (!string.IsNullOrEmpty(selectionName))
            {
                var item = ResourceBrushes.FirstOrDefault(r => r.Name == selectionName);

                if (item != null)
                {
                    SelectedResource = item;
                    set = true;
                }
            }

            if (!set && ResourceBrushes.Count > 0)
            {
                SelectedResource = ResourceBrushes[0];
            }

            if (Application.Current.Resources[ButtonImageFlagKey] is bool flag1)
            {
                ButtonImageFlag = flag1;
            }

            if (Application.Current.Resources[SyntaxColorFlagKey] is bool flag2)
            {
                SyntaxColorFlag = flag2;
            }
        }

        private void UpdateColorGroup(ThemeBrush value)
        {
            if (value.BrushType == BrushType.SolidColorBrush)
            {
                ColorGroup.Clear();

                if (value != null)
                {
                    var group = ResourceBrushes.Where(r => r.BrushType == BrushType.SolidColorBrush &&
                        r.ColorBrush.Color == value.ColorBrush.Color).ToList();

                    foreach (ThemeBrush item in group)
                    {
                        ColorGroup.Add(item);
                    }
                }
            }
        }

        internal void Save(Brush brush)
        {
            if (SelectedResource != null)
            {
                if (brush is SolidColorBrush solidBrush)
                {
                    SelectedResource.ColorBrush = solidBrush;
                    SelectedResource.IsModified = true;
                    SelectedResource.HasPendingChange = false;
                }
                else if (brush is LinearGradientBrush gradientBrush)
                {
                    SelectedResource.GradientBrush = gradientBrush;
                    SelectedResource.IsModified = true;
                    SelectedResource.HasPendingChange = false;
                }

                if (SyncColors)
                {
                    if (brush is SolidColorBrush solidBrush2)
                    {
                        foreach (var item in ColorGroup)
                        {
                            item.ColorBrush = solidBrush2;
                            item.IsModified = true;
                            item.HasPendingChange = false;
                        }
                    }
                }
                else
                {
                    // revert changes that may have been visible if the
                    // checkbox was unchecked just before save
                    foreach (var item in ColorGroup)
                    {
                        if (Application.Current.Resources[item.Name] is SolidColorBrush)
                        {
                            Application.Current.Resources[item.Name] = item.ColorBrush;
                            item.HasPendingChange = false;
                        }
                    }
                }

                UpdateColorGroup(SelectedResource);
            }
        }

        public void Save(DropShadowEffect dropShadowEffect)
        {
            if (SelectedResource != null)
            {
                SelectedResource.DropShadowEffect = dropShadowEffect;
                SelectedResource.IsModified = true;
                SelectedResource.HasPendingChange = false;
            }
        }

        public bool IsColorChanged => SelectedResource?.HasPendingChange ?? false;

        public ObservableCollection<ThemeBrush> ResourceBrushes { get; private set; } = [];

        public ObservableCollection<ThemeBrush> ColorGroup { get; } = [];

        [ObservableProperty]
        private ThemeBrush? selectedResource = null;
        partial void OnSelectedResourceChanged(ThemeBrush? value)
        {
            if (value != null && value.BrushType == BrushType.SolidColorBrush)
            {
                UpdateColorGroup(value);
            }
        }

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

        [ObservableProperty]
        private bool buttonImageFlag;

        public static string ButtonImageFlagKey => "ToggleButton.DarkImages";
        public bool ButtonImageFlagChanged
        {
            get
            {
                if (Application.Current.Resources[ButtonImageFlagKey] is bool flag)
                        return flag != ButtonImageFlag;
                return false;
            }
        }

        [ObservableProperty]
        private bool syntaxColorFlag;

        public static string SyntaxColorFlagKey => "AvalonEdit.SyntaxColor.Invert";

        public bool SyntaxColorFlagChanged
        {
            get
            {
                if (Application.Current.Resources[SyntaxColorFlagKey] is bool flag)
                    return flag != SyntaxColorFlag;
                return false;
            }
        }
    }
}
