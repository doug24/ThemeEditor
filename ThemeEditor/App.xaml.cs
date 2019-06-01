using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            string appTheme = "Light";// appThemeSvc.CurrentTheme == WindowsTheme.Dark ? "Dark" : "Light";
            Resources.MergedDictionaries[0].Source = new Uri($"/Themes/{appTheme}Brushes.xaml", UriKind.Relative);

            //Invert(Colors.Navy);
            //Invert(Colors.Yellow);
            //Invert(Colors.White);
            //Invert(Colors.Black);
            //Color iBlue = Invert(Colors.Blue);
            //Color iRoyalBlue = Invert(Colors.RoyalBlue);

            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        public static Color Invert(Color c)
        {
            double white_bias = .08;
            double m = 1.0 + white_bias;
            double shift = white_bias + (byte.MaxValue - Math.Min(c.R, Math.Min(c.G, c.B)) - Math.Max(c.R, Math.Max(c.G, c.B)));
            Color result = new Color
            {
                A = c.A,
                R = (byte)((shift + c.R) / m),
                G = (byte)((shift + c.G) / m),
                B = (byte)((shift + c.B) / m),
            };
            return result;
        }

        //public static Color Invert(Color c)
        //{
        //    byte shift = (byte)(byte.MaxValue - Math.Min(c.R, Math.Min(c.G, c.B)) - Math.Max(c.R, Math.Max(c.G, c.B)));
        //    Color result = new Color
        //    {
        //        A = c.A,
        //        R = (byte)(shift + c.R),
        //        G = (byte)(shift + c.G),
        //        B = (byte)(shift + c.B),
        //    };
        //    return result;
        //}

    }
}