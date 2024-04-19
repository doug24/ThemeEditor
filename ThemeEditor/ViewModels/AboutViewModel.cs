using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using ThemeEditor.Properties;

namespace ThemeEditor
{
    public partial class AboutViewModel : ObservableObject
    {
        public AboutViewModel()
        {
            Version = $"Version {AssemblyVersion}";
            BuildDate = $"Built on {AssemblyBuildDate?.ToString(CultureInfo.CurrentCulture)}";
            Copyright = AssemblyCopyright;
            Description = AssemblyDescription;
        }

        [ObservableProperty]
        private string version = string.Empty;

        [ObservableProperty]
        private string buildDate = string.Empty;

        [ObservableProperty]
        private string copyright = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;


        public static string AssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;

        public static string AssemblyDescription
        {
            get
            {
                // Get all Description attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                // If there aren't any Description attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;
                // If there is a Description attribute, return its value
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public static string AssemblyCopyright
        {
            get
            {
                // Get all Copyright attributes on this assembly
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                // If there aren't any Copyright attributes, return an empty string
                if (attributes.Length == 0)
                    return string.Empty;
                // If there is a Copyright attribute, return its value
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public static DateTime? AssemblyBuildDate => GetAssemblyBuildDateTime(Assembly.GetExecutingAssembly());

        // https://stackoverflow.com/questions/1600962/displaying-the-build-date
        private static DateTime? GetAssemblyBuildDateTime(Assembly assembly)
        {
            var attr = Attribute.GetCustomAttribute(assembly, typeof(BuildDateTimeAttribute)) as BuildDateTimeAttribute;
            if (DateTime.TryParse(attr?.Date, out DateTime dt))
                return dt.ToLocalTime();
            else
                return null;
        }
    }
}
