using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace ThemeEditor
{
    public class ColorEditorViewModel : ViewModelBase
    {
        private bool lockUpdates = false;

        public ColorEditorViewModel()
        {
            var webColors = GetWebColors();
            webColors.Sort(new HueComparer());

            WebColors = new ObservableCollection<NamedColor>(webColors);

            var systemColors = GetSystemColors().OrderBy(nc => nc.Name);
            SystemColors = new ObservableCollection<NamedColor>(systemColors);
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


        private Color color = Colors.White;
        public Color Color
        {
            get { return color; }
            set
            {
                if (color == value || lockUpdates)
                    return;

                color = value;
                UpdateElements(color);
                ColorBrush = new SolidColorBrush(color);
                OnPropertyChanged("Color");
            }
        }


        private SolidColorBrush colorBrush = Brushes.White;
        public SolidColorBrush ColorBrush
        {
            get { return colorBrush; }
            set
            {
                if (colorBrush == value)
                    return;

                colorBrush = value;
                OnPropertyChanged("ColorBrush");
            }
        }

        private byte alphaElem = 255;
        public byte AlphaElem
        {
            get { return alphaElem; }
            set
            {
                if (alphaElem == value)
                    return;

                alphaElem = value;
                OnPropertyChanged("AlphaElem");

                Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
            }
        }

        private byte redElem = 255;
        public byte RedElem
        {
            get { return redElem; }
            set
            {
                if (redElem == value)
                    return;

                redElem = value;
                OnPropertyChanged("RedElem");

                Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
            }
        }

        private byte greenElem = 255;
        public byte GreenElem
        {
            get { return greenElem; }
            set
            {
                if (greenElem == value)
                    return;

                greenElem = value;
                OnPropertyChanged("GreenElem");

                Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
            }
        }

        private byte blueElem = 255;
        public byte BlueElem
        {
            get { return blueElem; }
            set
            {
                if (blueElem == value)
                    return;

                blueElem = value;
                OnPropertyChanged("BlueElem");

                Color = Color.FromArgb(AlphaElem, RedElem, GreenElem, BlueElem);
            }
        }

        private int hueElem = 0;
        public int HueElem
        {
            get { return hueElem; }
            set
            {
                if (hueElem == value)
                    return;

                if (value == -1)
                    hueElem = 359;
                else if (value == 360)
                    hueElem = 0;
                else
                    hueElem = value;

                OnPropertyChanged("HueElem");

                Color = ColorHSV.ConvertToColor(
                    HueElem / 360d,
                    SaturationElem / 100d,
                    ValueElem / 100d);
            }
        }

        private int saturationElem = 0;
        public int SaturationElem
        {
            get { return saturationElem; }
            set
            {
                if (saturationElem == value)
                    return;

                saturationElem = value;
                OnPropertyChanged("SaturationElem");

                Color = ColorHSV.ConvertToColor(
                    HueElem / 360d,
                    SaturationElem / 100d,
                    ValueElem / 100d);
            }
        }

        private int valueElem = 100;
        public int ValueElem
        {
            get { return valueElem; }
            set
            {
                if (valueElem == value)
                    return;

                valueElem = value;
                OnPropertyChanged("ValueElem");

                Color = ColorHSV.ConvertToColor(
                    HueElem / 360d,
                    SaturationElem / 100d,
                    ValueElem / 100d);
            }
        }


        private Color minSaturationColor = Colors.White;
        public Color MinSaturationColor
        {
            get { return minSaturationColor; }
            set
            {
                if (minSaturationColor == value)
                    return;

                minSaturationColor = value;
                OnPropertyChanged("MinSaturationColor");
            }
        }


        private Color maxSaturationColor = Colors.Black;
        public Color MaxSaturationColor
        {
            get { return maxSaturationColor; }
            set
            {
                if (maxSaturationColor == value)
                    return;

                maxSaturationColor = value;
                OnPropertyChanged("MaxSaturationColor");
            }
        }

        private Color minValueColor = Colors.Black;
        public Color MinValueColor
        {
            get { return minValueColor; }
            set
            {
                if (minValueColor == value)
                    return;

                minValueColor = value;
                OnPropertyChanged("MinValueColor");
            }
        }

        private Color maxValueColor = Colors.White;
        public Color MaxValueColor
        {
            get { return maxValueColor; }
            set
            {
                if (maxValueColor == value)
                    return;

                maxValueColor = value;
                OnPropertyChanged("MaxValueColor");
            }
        }


        public ObservableCollection<NamedColor> WebColors { get; }
        public ObservableCollection<NamedColor> SystemColors { get; }


        private NamedColor selectedWebColor = null;
        public NamedColor SelectedWebColor
        {
            get { return selectedWebColor; }
            set
            {
                if (selectedWebColor == value)
                    return;

                selectedWebColor = value;
                OnPropertyChanged("SelectedWebColor");

                if (selectedWebColor != null)
                    Color = selectedWebColor.Color;
            }
        }

        private NamedColor selectedSystemColor = null;
        public NamedColor SelectedSystemColor
        {
            get { return selectedSystemColor; }
            set
            {
                if (selectedSystemColor == value)
                    return;

                selectedSystemColor = value;
                OnPropertyChanged("SelectedSystemColor");

                if (selectedSystemColor != null)
                    Color = selectedSystemColor.Color;
            }
        }


        private List<NamedColor> GetWebColors()
        {
            List<NamedColor> colors = new List<NamedColor>();
            Type type = typeof(System.Windows.Media.Colors);
            foreach (var p in type.GetProperties())
            {
                if (p.GetValue(null, null) is System.Windows.Media.Color color)
                {
                    Color c = Color.FromArgb(color.A, color.R, color.G, color.B);
                    colors.Add(new NamedColor(p.Name, c));
                }
            }
            return colors;
        }

        private List<NamedColor> GetSystemColors()
        {
            List<NamedColor> colors = new List<NamedColor>();
            Type type = typeof(System.Windows.SystemColors);
            foreach (var p in type.GetProperties())
            {
                if (p.GetValue(null, null) is System.Windows.Media.Color color)
                {
                    Color c = Color.FromArgb(color.A, color.R, color.G, color.B);
                    colors.Add(new NamedColor(p.Name.Replace("Color", ""), c));
                }
            }
            return colors;
        }

        class HueComparer : IComparer<NamedColor>
        {
            public int Compare(NamedColor x, NamedColor y)
            {
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

    public class NamedColor
    {
        public NamedColor(string name, Color color)
        {
            Name = name;
            Color = color;
            Brush = new SolidColorBrush(color);
        }

        public string Name { get; private set; }
        public Color Color { get; private set; }
        public SolidColorBrush Brush { get; private set; }

        public override bool Equals(object obj)
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
