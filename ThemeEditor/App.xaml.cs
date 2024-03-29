﻿using System.Windows;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        public ResourceDictionary ThemeResources => Resources.MergedDictionaries[0];
    }
}