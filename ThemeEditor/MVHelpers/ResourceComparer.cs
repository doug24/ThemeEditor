using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ThemeEditor
{
    public class SolidColorBrushComparer : EqualityComparer<SolidColorBrush>
    {
        public override bool Equals(SolidColorBrush? x, SolidColorBrush? y)
        {
            return x?.Color == y?.Color &&
                x?.Opacity == y?.Opacity;
        }

        public override int GetHashCode([DisallowNull] SolidColorBrush obj)
        {
            return HashCode.Combine(obj.Color, obj.Opacity);
        }
    }

    public class LinearGradientBrushComparer : EqualityComparer<LinearGradientBrush>
    {
        public override bool Equals(LinearGradientBrush? x, LinearGradientBrush? y)
        {
            if (x == null && y == null) return true;
            else if (x != null && y == null) return false;
            else if (x == null && y != null) return false;
            else if (x != null && y != null)
            {
                GradientStopComparer comp = new();

                if (x.GradientStops.Count != y.GradientStops.Count)
                    return false;

                for (int i = 0; i < x.GradientStops.Count; i++)
                {
                    if (!comp.Equals(x.GradientStops[i], y.GradientStops[i]))
                        return false;
                }

                return x.StartPoint == y.StartPoint &&
                    x.EndPoint == y.EndPoint &&
                    x.Opacity == y.Opacity;
            }
            return false;
        }

        public override int GetHashCode([DisallowNull] LinearGradientBrush obj)
        {
            return HashCode.Combine(obj.StartPoint, obj.EndPoint, obj.Opacity, obj.GradientStops);
        }
    }

    public class GradientStopComparer : EqualityComparer<GradientStop>
    {
        public override bool Equals(GradientStop? x, GradientStop? y)
        {
            return x?.Color == y?.Color &&
                x?.Offset == y?.Offset;
        }

        public override int GetHashCode([DisallowNull] GradientStop obj)
        {
            return HashCode.Combine(obj.Color, obj.Offset);
        }
    }

    public class DropShadowEffectComparer : EqualityComparer<DropShadowEffect>
    {
        public override bool Equals(DropShadowEffect? x, DropShadowEffect? y)
        {
            return x?.BlurRadius == y?.BlurRadius &&
                x?.Color == y?.Color &&
                x?.Direction == y?.Direction &&
                x?.Opacity == y?.Opacity &&
                x?.ShadowDepth == y?.ShadowDepth;
        }

        public override int GetHashCode([DisallowNull] DropShadowEffect obj)
        {
            return HashCode.Combine(obj.BlurRadius, obj.Color, obj.Direction, obj.Opacity, obj.ShadowDepth);
        }
    }
}
