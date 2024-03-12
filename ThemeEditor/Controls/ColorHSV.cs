using System;
using System.Windows.Media;

namespace ThemeEditor
{
    public class ColorHSV(byte a, double h, double s, double v)
    {
        private double _hue = h;
        private double _saturation = s;
        private double _value = v;

        public byte A { get; set; } = a;

        public double Hue
        {
            get { return _hue; }
            set { if (value >= 0 && value <= 1.0) _hue = value; }
        }

        public double Saturation
        {
            get { return _saturation; }
            set { if (value >= 0 && value <= 1.0) _saturation = value; }
        }

        public double Value
        {
            get { return _value; }
            set { if (value >= 0 && value <= 1.0) _value = value; }
        }

        public override bool Equals(object? obj)
        {
            if (obj is ColorHSV x)
            {
                return
                    Hue == x.Hue &&
                    Saturation == x.Saturation &&
                    Value == x.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_hue, _saturation, _value);
        }

        public static bool operator ==(ColorHSV left, ColorHSV right)
        {
            return left.Hue == right.Hue &&
                left.Saturation == right.Saturation &&
                left.Value == right.Value;
        }

        public static bool operator !=(ColorHSV left, ColorHSV right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return string.Format("{0}°, {1}%, {2}%",
                (_hue * 360).ToString("f0"),
                (_saturation * 100).ToString("f0"),
                (_value * 100).ToString("f0"));
        }

        public Color ToColor()
        {
            return ConvertToColor(A, _hue, _saturation, _value);
        }

        public static Color ConvertToColor(byte a, double h, double s, double v)
        {
            double r, g, b;

            r = v;   // default to gray
            g = v;
            b = v;

            int hi = (int)(h * 6.0);
            double f = (h * 6.0) - hi;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);

            switch (hi)
            {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;

                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;

                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;

                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;

                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;

                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            }

            Color rgb = Color.FromArgb(a,
                Convert.ToByte(r * 255.0f),
                Convert.ToByte(g * 255.0f),
                Convert.ToByte(b * 255.0f));

            return rgb;
        }

        public static ColorHSV ConvertFrom(Color c)
        {
            double r = c.R / 255.0;
            double g = c.G / 255.0;
            double b = c.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue, sat, val;
            if (max == min)
                hue = 0.0;
            else if (max == r)
                hue = ((60 * ((g - b) / delta)) + 360) % 360;
            else if (max == g)
                hue = (60 * ((b - r) / delta)) + 120;
            else
                hue = (60 * ((r - g) / delta)) + 240;

            hue /= 360;

            if (max == 0.0)
                sat = 0.0;
            else
                sat = delta / max;

            val = max;

            return new ColorHSV(c.A, hue, sat, val);
        }
    }
}
