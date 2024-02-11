using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeEditor
{
    public partial class ColorEditorViewModel : ObservableObject
    {
        private bool lockUpdates = false;

        public ColorEditorViewModel()
        {
            var webColors = GetWebColors();
            webColors.Sort(new HueComparer());
            WebColors = new ObservableCollection<NamedColor>(webColors);

            var systemColors = GetSystemColors().OrderBy(nc => nc.Name);
            SystemColors = new ObservableCollection<NamedColor>(systemColors);

            ThemeColors = [];
            SortedColors = [];
            InitializeThemeColors();
        }

        internal void InitializeThemeColors()
        {
            ThemeColors.Clear();
            var themeColors = GetThemeColors().OrderBy(nc => nc.Name);
            foreach (var color in themeColors)
            {
                ThemeColors.Add(color);
            }

            SortedColors.Clear();
            var sortedColors = GetThemeColors();
            sortedColors.Sort(new HueComparer());
            foreach (var color in sortedColors)
            {
                SortedColors.Add(color);
            }
        }

        private void UpdateElements(Color color)
        {
            if (!lockUpdates)
            {
                lockUpdates = true;

                AlphaElem = color.A;
                RedElem = color.R;
                GreenElem = color.G;
                BlueElem = color.B;

                ColorHSV hsv = ColorHSV.ConvertFrom(color);

                HueElem = Convert.ToInt32(hsv.Hue * 360);
                SaturationElem = Convert.ToInt32(hsv.Saturation * 100);
                ValueElem = Convert.ToInt32(hsv.Value * 100);

                if (hsv.Saturation == 0) // gray scale
                {
                    MinSaturationColor = Colors.White;
                    MaxSaturationColor = Colors.Black;
                    MinValueColor = Colors.Black;
                    MaxValueColor = Colors.White;
                }
                else
                {
                    MinSaturationColor = ColorHSV.ConvertToColor(hsv.Hue, 0d, 1d);
                    MaxSaturationColor = ColorHSV.ConvertToColor(hsv.Hue, 1d, 1d);
                    MinValueColor = ColorHSV.ConvertToColor(hsv.Hue, 1d, 0d);
                    MaxValueColor = ColorHSV.ConvertToColor(hsv.Hue, 1d, 1d);
                }

                SelectedWebColor = WebColors.FirstOrDefault(nc => nc.Color.R == color.R && nc.Color.G == color.G && nc.Color.B == color.B);

                if (SelectedSystemColor != null && !SelectedSystemColor.Color.Equals(color))
                    SelectedSystemColor = null;

                lockUpdates = false;
            }
        }

        [ObservableProperty]
        private Color color = Colors.White;
        partial void OnColorChanging(Color value)
        {
            UpdateElements(value);
            ColorBrush = new SolidColorBrush(value);
            ColorARGB = string.Join(", ", value.A, value.R, value.G, value.B);
            ColorHex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", value.A, value.R, value.G, value.B);
            ColorBrightness = Brightness(value).ToString();
        }

        [ObservableProperty]
        private SolidColorBrush colorBrush = Brushes.White;

        [ObservableProperty]
        private string colorInput = string.Empty;

        [ObservableProperty]
        private string colorARGB = string.Empty;

        [ObservableProperty]
        private string colorHex = string.Empty;

        [ObservableProperty]
        private string colorBrightness = string.Empty;

        [ObservableProperty]
        private byte alphaElem = 255;
        partial void OnAlphaElemChanged(byte value)
        {
            Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
        }

        [ObservableProperty]
        private byte redElem = 255;
        partial void OnRedElemChanged(byte value)
        {
            Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
        }

        [ObservableProperty]
        private byte greenElem = 255;
        partial void OnGreenElemChanged(byte value)
        {
            Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
        }

        [ObservableProperty]
        private byte blueElem = 255;
        partial void OnBlueElemChanged(byte value)
        {
            Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
        }

        private int hueElem = 0;
        public int HueElem
        { 
            get { return hueElem; } 
            set 
            {
                int newValue = value;
                if (value == -1)
                    newValue = 359;
                else if (value == 360)
                    newValue = 0;

                SetProperty(ref hueElem, newValue);

                Color = ColorHSV.ConvertToColor(
                    HueElem / 360d,
                    SaturationElem / 100d,
                    ValueElem / 100d);
            }
        }

        [ObservableProperty]
        private int saturationElem = 0;
        partial void OnSaturationElemChanged(int value)
        {
            Color = ColorHSV.ConvertToColor(
                HueElem / 360d,
                SaturationElem / 100d,
                ValueElem / 100d);
        }

        [ObservableProperty]
        private int valueElem = 100;
        partial void OnValueElemChanged(int value)
        {
            Color = ColorHSV.ConvertToColor(
                HueElem / 360d,
                SaturationElem / 100d,
                ValueElem / 100d);
        }

        [ObservableProperty]
        private Color minSaturationColor = Colors.White;

        [ObservableProperty]
        private Color maxSaturationColor = Colors.Black;

        [ObservableProperty]
        private Color minValueColor = Colors.Black;

        [ObservableProperty]
        private Color maxValueColor = Colors.White;

        public ObservableCollection<NamedColor> WebColors { get; }
        public ObservableCollection<NamedColor> ThemeColors { get; }
        public ObservableCollection<NamedColor> SystemColors { get; }
        public ObservableCollection<NamedColor> SortedColors { get; }


        [ObservableProperty]
        private NamedColor? selectedWebColor = null;
        partial void OnSelectedWebColorChanged(NamedColor? value)
        {
            if (value != null)
                Color = value.Color;
        }

        [ObservableProperty]
        private NamedColor? selectedSystemColor = null;
        partial void OnSelectedSystemColorChanged(NamedColor? value)
        {
            if (value != null)
                Color = value.Color;
        }

        [ObservableProperty]
        private NamedColor? selectedSortedColor = null;
        partial void OnSelectedSortedColorChanged(NamedColor? value)
        {
            if (value != null)
                Color = value.Color;
        }

        private static List<NamedColor> GetWebColors()
        {
            List<NamedColor> colors = [];
            Type type = typeof(Colors);
            foreach (var p in type.GetProperties())
            {
                if (p.GetValue(null, null) is Color color)
                {
                    Color c = Color.FromArgb(color.A, color.R, color.G, color.B);
                    colors.Add(new NamedColor(p.Name, c));
                }
            }
            return colors;
        }

        private static List<NamedColor> GetThemeColors()
        {
            List<NamedColor> colors = [];
            var dictionary = Application.Current.Resources.MergedDictionaries[0];
            foreach (object key in dictionary.Keys)
            {
                if (dictionary[key] is SolidColorBrush br)
                    colors.Add(new NamedColor(key.ToString() ?? string.Empty, br.Color));
            }

            return colors;
        }

        private static List<NamedColor> GetSystemColors()
        {
            List<NamedColor> colors = [];
            Type type = typeof(SystemColors);
            foreach (var p in type.GetProperties())
            {
                if (p.GetValue(null, null) is Color color)
                {
                    Color c = Color.FromArgb(color.A, color.R, color.G, color.B);
                    colors.Add(new NamedColor(p.Name.Replace("Color", ""), c));
                }
            }
            return colors;
        }

        public ICommand ParseCommand => new RelayCommand(
            p => ParseText(ColorInput),
            q => !string.IsNullOrWhiteSpace(ColorInput));

        private static int Brightness(Color c)
        {
            return (int)Math.Sqrt(
               c.R * c.R * .241 +
               c.G * c.G * .691 +
               c.B * c.B * .068);
        }

        private void ParseText(string clr)
        {
            Color? color = null;

            if (!string.IsNullOrEmpty(clr))
            {
                if (clr.StartsWith("#", StringComparison.OrdinalIgnoreCase) &&
                    TryConvert(clr, out Color c1))
                {
                    color = c1;
                }
                //else if (clr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                //{
                //    clr = clr.Substring(2);
                //    uint argb = uint.Parse(clr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                //    if (argb < 0x01000000)
                //        argb = 0xff000000 | argb; // add the alpha channel
                //    color = Color.FromArgb();
                //}
                else if (TryConvert(clr, out Color c2))
                {
                    color = c2;
                }
                else if (!clr.Contains(','))
                {
                    color = (Color)ColorConverter.ConvertFromString("#" + clr);
                }
                else if (char.IsDigit(clr[0]))
                {
                    if (clr.Contains(','))
                    {
                        string[] splt = clr.Split(',');
                        if (splt.Length > 3)
                        {
                            byte a = Convert.ToByte(splt[0], CultureInfo.InvariantCulture);
                            byte r = Convert.ToByte(splt[1], CultureInfo.InvariantCulture);
                            byte g = Convert.ToByte(splt[2], CultureInfo.InvariantCulture);
                            byte b = Convert.ToByte(splt[3], CultureInfo.InvariantCulture);
                            color = Color.FromArgb(a, r, g, b);
                        }
                        else if (splt.Length > 2)
                        {
                            byte r = Convert.ToByte(splt[0], CultureInfo.InvariantCulture);
                            byte g = Convert.ToByte(splt[1], CultureInfo.InvariantCulture);
                            byte b = Convert.ToByte(splt[2], CultureInfo.InvariantCulture);
                            color = Color.FromArgb(255, r, g, b);
                        }
                    }
                }
            }

            if (color.HasValue)
                Color = color.Value;
        }

        private static bool TryConvert(string value, out Color color)
        {
            color = Colors.Transparent;
            try
            {
                color = (Color)ColorConverter.ConvertFromString(value);
                return true;
            }
            catch { }
            return false;
        }

        class HueComparer : IComparer<NamedColor>
        {
            public int Compare(NamedColor? x, NamedColor? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                if (x.Color.A == 0 && y.Color.A == 0) return 0;
                if (x.Color.A == 0) return -1;
                if (y.Color.A == 0) return 1;

                ColorHSV x1 = ColorHSV.ConvertFrom(x.Color);
                ColorHSV y1 = ColorHSV.ConvertFrom(y.Color);

                if (x1.Saturation == 0 && y1.Saturation == 0) return x1.Value.CompareTo(y1.Value);
                if (x1.Saturation == 0) return -1;
                if (y1.Saturation == 0) return 1;

                if (x1.Hue == y1.Hue)
                {
                    return x1.Value.CompareTo(y1.Value);
                }

                return x1.Hue.CompareTo(y1.Hue);
            }
        }
    }

    public partial class NamedColor : ObservableObject
    {
        public NamedColor(object key, string name, Color color)
        {
            Key = key;
            Name = name;
            Color = color;
            Brush = new SolidColorBrush(color);
            Hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        public NamedColor(string name, Color color)
        {
            Key = name;
            Name = name;
            Color = color;
            Brush = new SolidColorBrush(color);
            Hex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);
        }

        public string Name { get; private set; }
        public object Key { get; private set; }
        public string Hex { get; private set; }
        public bool IsModified { get; set; }

        [ObservableProperty]
        private Color color = Colors.Black;
        partial void OnColorChanged(Color value)
        {
            Brush = new SolidColorBrush(value);
        }

        [ObservableProperty]
        private SolidColorBrush brush = Brushes.Black;

        public override bool Equals(object? obj)
        {
            if (obj is NamedColor other)
            {
                return Name.Equals(other.Name);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
