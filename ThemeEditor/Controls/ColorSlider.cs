using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ThemeEditor
{
    public class ColorSlider : Slider
    {
        static ColorSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSlider), new FrameworkPropertyMetadata(typeof(ColorSlider)));
        }

        public static readonly DependencyProperty MaxColorProperty =
            DependencyProperty.Register("MaxColor", typeof(Color), typeof(ColorSlider),
                new UIPropertyMetadata(Colors.Red, new PropertyChangedCallback(ColorChangedCallback)));

        public Color MaxColor
        {
            get { return (Color)this.GetValue(MaxColorProperty); }
            set { this.SetValue(MaxColorProperty, value); }
        }

        public static readonly DependencyProperty MinColorProperty =
            DependencyProperty.Register("MinColor", typeof(Color), typeof(ColorSlider),
                new FrameworkPropertyMetadata(Color.FromArgb(255, 76, 76, 76), new PropertyChangedCallback(ColorChangedCallback)));

        public Color MinColor
        {
            get { return (Color)this.GetValue(MinColorProperty); }
            set { this.SetValue(MinColorProperty, value); }
        }

        public static readonly DependencyProperty IsHueProperty =
            DependencyProperty.Register("IsHue", typeof(bool), typeof(ColorSlider),
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(ColorChangedCallback)));

        public bool IsHue
        {
            get { return (bool)this.GetValue(IsHueProperty); }
            set { this.SetValue(IsHueProperty, value); }
        }



        public ColorSlider()
        {
            UpdateBackground();
        }

        private void UpdateBackground()
        {
            if (IsHue)
            {
                Background = new LinearGradientBrush(new GradientStopCollection()
                {
                    new GradientStop(Color.FromArgb(255, 255, 0, 0), 0.0),
                    new GradientStop(Color.FromArgb(255, 255, 255, 0), 1.0 / 6.0),
                    new GradientStop(Color.FromArgb(255, 0, 255, 0), 2.0 / 6.0),
                    new GradientStop(Color.FromArgb(255, 0, 255, 255), 3.0 / 6.0),
                    new GradientStop(Color.FromArgb(255, 0, 0, 255), 4.0/ 6.0),
                    new GradientStop(Color.FromArgb(255, 255, 0, 255), 5.0 / 6.0),
                    new GradientStop(Color.FromArgb(255, 255, 0, 0), 1.0),

                }, 0.0);
            }
            else
            {
                Background = new LinearGradientBrush(MinColor, MaxColor, 0.0);
            }
        }

        private static void ColorChangedCallback(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            if (property is ColorSlider colorSlider)
            {
                colorSlider.UpdateBackground();
            }
        }
    }
}
