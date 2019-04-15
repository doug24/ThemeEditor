using System;
using System.Windows.Media;

namespace ThemeEditor
{
    internal static class ColorExtensions
    {
        public static double Distance(this Color source, Color target)
        {
            ColorHSV c1 = ColorHSV.ConvertFrom(source);
            ColorHSV c2 = ColorHSV.ConvertFrom(target);

            double hue = c1.Hue - c2.Hue;
            double saturation = c1.Saturation - c2.Saturation;
            double brightness = c1.Value - c2.Value;

            return (hue * hue) + (saturation * saturation) + (brightness * brightness);
        }

    }
}
