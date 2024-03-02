using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ThemeEditor
{
    public enum BrushType { SolidColorBrush, LinearGradientBrush, DropShadowEffect }

    public partial class BrushEditorViewModel : ObservableObject
    {
        public static readonly Color EmptyColor = new() { A = 0, R = 0, G = 0, B = 0 };
        public static readonly SolidColorBrush EmptySolidBrush = new() { Color = EmptyColor };
        public static readonly LinearGradientBrush EmptyGradientBrush = new();
        public static readonly DropShadowEffect EmptyDropShadow = new() { Color = EmptyColor };

        private static readonly SolidColorBrushComparer solidColorBrushComparer = new();
        private static readonly LinearGradientBrushComparer linearGradientBrushComparer = new();
        private static readonly DropShadowEffectComparer dropShadowEffectComparer = new();

        // reentrant flags
        private bool inUpdateElements = false;
        private bool inColorChanging = false;
        private bool inColorInitializing = false;
        private bool inBrushChange = false;

        public LinearGradientBrushViewModel LinearGradientBrushVM { get; }
        public DropShadowEffectViewModel DropShadowEffectVM { get; }

        public BrushEditorViewModel()
        {
            var webColors = GetWebColors();
            webColors.Sort(new HueComparer());
            WebColors = new ObservableCollection<NamedColor>(webColors);

            var systemColors = GetSystemColors().OrderBy(nc => nc.Name);
            SystemColors = new ObservableCollection<NamedColor>(systemColors);

            ThemeColors = [];
            themeColorsView = CollectionViewSource.GetDefaultView(ThemeColors);
            themeColorsView.Filter = (o) => string.IsNullOrEmpty(ThemeFilter) || ((NamedColor)o).Name.Contains(ThemeFilter, StringComparison.OrdinalIgnoreCase);

            SortedColors = [];
            sortedColorsView = CollectionViewSource.GetDefaultView(SortedColors);
            sortedColorsView.Filter = (o) => string.IsNullOrEmpty(SortedFilter) || ((NamedColor)o).Name.Contains(SortedFilter, StringComparison.OrdinalIgnoreCase);

            InitializeThemeBrushes();

            LinearGradientBrushVM = new(this);
            LinearGradientBrushVM.PropertyChanged += LinearGradientBrushVM_PropertyChanged;

            DropShadowEffectVM = new();
            DropShadowEffectVM.PropertyChanged += DropShadowEffectVM_PropertyChanged;
        }

        internal void InitializeThemeBrushes()
        {
            ThemeColors.Clear();
            var themeBrushes = GetThemeColors().OrderBy(nc => nc.Name);
            foreach (var color in themeBrushes)
            {
                ThemeColors.Add(color);
            }

            SortedColors.Clear();
            var sortedBrushes = GetThemeColors();
            sortedBrushes.Sort(new HueComparer());
            foreach (var color in sortedBrushes)
            {
                SortedColors.Add(color);
            }
        }

        private void LinearGradientBrushVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LinearGradientBrushVM.SelectedStop) &&
                LinearGradientBrushVM.SelectedStop != null)
            {
                inColorInitializing = true;
                Color = LinearGradientBrushVM.SelectedStop.Color;
                inColorInitializing = false;
            }
            else if (e.PropertyName == nameof(LinearGradientBrushVM.Brush))
            {
                if (!linearGradientBrushComparer.Equals(GradientBrush, LinearGradientBrushVM.Brush))
                {
                    GradientBrush = LinearGradientBrushVM.Brush;
                }
            }
        }

        private void DropShadowEffectVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DropShadowEffectVM.DropShadowEffect))
            {
                if (!dropShadowEffectComparer.Equals(DropShadowEffect, DropShadowEffectVM.DropShadowEffect))
                {
                    DropShadowEffect = DropShadowEffectVM.DropShadowEffect;
                }
            }
        }

        private void UpdateElements(Color color)
        {
            if (inUpdateElements) return;
            inUpdateElements = true;

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

            inUpdateElements = false;
        }

        [ObservableProperty]
        private string selectedName = string.Empty;

        [ObservableProperty]
        private Color color = Colors.Transparent;
        partial void OnColorChanging(Color value)
        {
            if (inColorChanging) return;
            inColorChanging = true;

            UpdateElements(value);

            EditColor = new SolidColorBrush(value);
            ColorARGB = string.Join(", ", value.A, value.R, value.G, value.B);
            ColorHex = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", value.A, value.R, value.G, value.B);
            ColorBrightness = Brightness(value).ToString();

            if (!inColorInitializing)
            {
                if (BrushType == BrushType.SolidColorBrush)
                {
                    ColorBrush = new SolidColorBrush(value);
                }
                else if (BrushType == BrushType.LinearGradientBrush)
                {
                    LinearGradientBrushVM.SetColor(value);
                }
                else if (BrushType == BrushType.DropShadowEffect)
                {
                    DropShadowEffectVM.Color = value;
                }
            }

            inColorChanging = false;
        }


        [ObservableProperty]
        private BrushType brushType = BrushType.SolidColorBrush;
        partial void OnBrushTypeChanged(BrushType value)
        {
            if (inBrushChange) return;

            if (value == BrushType.SolidColorBrush)
            {
                ColorBrush = new SolidColorBrush(Color);
            }
            else if (value == BrushType.LinearGradientBrush &&
                linearGradientBrushComparer.Equals(GradientBrush, EmptyGradientBrush))
            {
                LinearGradientBrush brush = new()
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops =
                    [
                        new GradientStop(Colors.LightGray, 0.0),
                        new GradientStop(Colors.DimGray, 1.0)
                    ],
                };
                LinearGradientBrushVM.InitializeFromBrush(brush);
            }
        }

        [ObservableProperty]
        private SolidColorBrush editColor = Brushes.Transparent;

        private SolidColorBrush colorBrush = EmptySolidBrush;
        public SolidColorBrush ColorBrush
        {
            get => colorBrush;
            set
            {
                if (SetProperty(ref colorBrush, value, solidColorBrushComparer, nameof(ColorBrush)))
                {
                    OnColorBrushChanged(value);
                }
            }
        }

        private void OnColorBrushChanged(SolidColorBrush value)
        {
            if (inBrushChange) return;
            inBrushChange = true;

            // reset these without raising property change events
            gradientBrush = EmptyGradientBrush;
            LinearGradientBrushVM.Clear();
            dropShadowEffect = EmptyDropShadow;
            DropShadowEffectVM.Clear();

            BrushType = BrushType.SolidColorBrush;

            inColorInitializing = true;
            Color = value.Color;
            inColorInitializing = false;

            inBrushChange = false;
        }

        private LinearGradientBrush gradientBrush = EmptyGradientBrush;
        public LinearGradientBrush GradientBrush
        {
            get => gradientBrush;
            set
            {
                if (SetProperty(ref gradientBrush, value, linearGradientBrushComparer, nameof(GradientBrush)))
                {
                    OnGradientBrushChanged(value);
                }
            }
        }
        private void OnGradientBrushChanged(LinearGradientBrush value)
        {
            if (inBrushChange) return;
            inBrushChange = true;

            // reset these without raising property change events
            colorBrush = EmptySolidBrush;
            dropShadowEffect = EmptyDropShadow;
            DropShadowEffectVM.Clear();

            BrushType = BrushType.LinearGradientBrush;

            if (!linearGradientBrushComparer.Equals(value, LinearGradientBrushVM.Brush))
            {
                LinearGradientBrushVM.InitializeFromBrush(value);
                if (LinearGradientBrushVM.SelectedStop != null)
                {
                    inColorInitializing = true;
                    Color = LinearGradientBrushVM.SelectedStop.Color;
                    inColorInitializing = false;
                }
            }

            inBrushChange = false;
        }

        private DropShadowEffect dropShadowEffect = EmptyDropShadow;
        public DropShadowEffect DropShadowEffect
        {
            get => dropShadowEffect;
            set
            {
                if (SetProperty(ref dropShadowEffect, value, dropShadowEffectComparer, nameof(DropShadowEffect)))
                {
                    OnDropShadowEffectChanged(value);
                }
            }
        }
        private void OnDropShadowEffectChanged(DropShadowEffect value)
        {
            if (inBrushChange) return;
            inBrushChange = true;

            // reset these without raising property change events
            colorBrush = EmptySolidBrush;
            gradientBrush = EmptyGradientBrush;
            LinearGradientBrushVM.Clear();

            BrushType = BrushType.DropShadowEffect;
            DropShadowEffectVM.InitializeFromDropShadowEffect(SelectedName, value);

            inColorInitializing = true;
            Color = value.Color;
            inColorInitializing = false;

            inBrushChange = false;
        }

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
        public ObservableCollection<NamedColor> SystemColors { get; }
        public ObservableCollection<NamedColor> ThemeColors { get; }

        private readonly ICollectionView themeColorsView;
        public ObservableCollection<NamedColor> SortedColors { get; }

        private readonly ICollectionView sortedColorsView;

        [ObservableProperty]
        private string themeFilter = string.Empty;
        partial void OnThemeFilterChanged(string value)
        {
            themeColorsView.Refresh();
        }

        [ObservableProperty]
        private string sortedFilter = string.Empty;
        partial void OnSortedFilterChanged(string value)
        {
            sortedColorsView.Refresh();
        }

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
        private NamedColor? selectedThemeColor = null;
        partial void OnSelectedThemeColorChanged(NamedColor? value)
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

            if (VSCodeTheme.ThemeColors.Count > 0)
            {
                foreach (string key in VSCodeTheme.ThemeColors.Keys)
                {
                    if (VSCodeTheme.ThemeColors.TryGetValue(key, out string? value) &&
                        !string.IsNullOrEmpty(value) &&
                        TryConvert(value, out Color color))
                    {
                        colors.Add(new NamedColor(key, color));
                    }
                }
            }
            else if (Application.Current is App app)
            {
                foreach (object key in app.ThemeResources.Keys)
                {
                    if (app.ThemeResources[key] is SolidColorBrush solidBrush)
                    {
                        colors.Add(new NamedColor(key.ToString() ?? string.Empty, solidBrush.Color));
                    }
                    else if (app.ThemeResources[key] is LinearGradientBrush gradientBrush)
                    {
                        int idx = 0;
                        foreach (var stop in gradientBrush.GradientStops)
                        {
                            string name = (key.ToString() ?? string.Empty) + ".Stop" + idx++;
                            colors.Add(new NamedColor(name, stop.Color));
                        }
                    }
                    else if (app.ThemeResources[key] is DropShadowEffect dropShadowEffect)
                    {
                        colors.Add(new NamedColor(key.ToString() ?? string.Empty, dropShadowEffect.Color));
                    }
                }
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
                        string[] split = clr.Split(',');
                        if (split.Length > 3)
                        {
                            byte a = Convert.ToByte(split[0], CultureInfo.InvariantCulture);
                            byte r = Convert.ToByte(split[1], CultureInfo.InvariantCulture);
                            byte g = Convert.ToByte(split[2], CultureInfo.InvariantCulture);
                            byte b = Convert.ToByte(split[3], CultureInfo.InvariantCulture);
                            color = Color.FromArgb(a, r, g, b);
                        }
                        else if (split.Length > 2)
                        {
                            byte r = Convert.ToByte(split[0], CultureInfo.InvariantCulture);
                            byte g = Convert.ToByte(split[1], CultureInfo.InvariantCulture);
                            byte b = Convert.ToByte(split[2], CultureInfo.InvariantCulture);
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

    public partial class ThemeBrush : ObservableObject
    {
        public ThemeBrush(string name, SolidColorBrush brush)
        {
            Name = name;
            ColorBrush = brush;
            BrushType = BrushType.SolidColorBrush;
        }

        public ThemeBrush(string name, LinearGradientBrush gradientBrush)
        {
            Name = name;
            GradientBrush = gradientBrush;
            BrushType = BrushType.LinearGradientBrush;
        }

        public ThemeBrush(string name, DropShadowEffect dropShadow)
        {
            Name = name;
            DropShadowEffect = dropShadow;
            BrushType = BrushType.DropShadowEffect;
        }

        public string Name { get; private set; }

        public BrushType BrushType { get; private set; }

        /// <summary>
        /// Gets or sets if the brush as a committed change
        /// </summary>
        public bool IsModified { get; set; }

        /// <summary>
        /// Gets or sets if the brush has a color or gradient change, but not committed
        /// </summary>
        [ObservableProperty]
        private bool hasPendingChange;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Brush))]
        private SolidColorBrush colorBrush = Brushes.Transparent;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Brush))]
        private LinearGradientBrush gradientBrush = BrushEditorViewModel.EmptyGradientBrush;

        [ObservableProperty]
        private DropShadowEffect dropShadowEffect = BrushEditorViewModel.EmptyDropShadow;

        public Brush Brush => BrushType switch
        {
            BrushType.LinearGradientBrush => GradientBrush,
            _ => ColorBrush,
        };
    }

    public class NamedColor(string name, Color color)
    {
        public string Name { get; private set; } = name;

        public Color Color { get; private set; } = color;

        public SolidColorBrush Brush { get; private set; } = new SolidColorBrush(color);

        public string Hex { get; private set; } = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", color.A, color.R, color.G, color.B);


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
