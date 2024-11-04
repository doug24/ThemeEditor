using System;
using System.Threading.Tasks;
using System.Windows;
using NLog;

namespace ThemeEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            logger.Info("Theme Editor started");
            SetupExceptionHandling();

            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        public ResourceDictionary ThemeResources => Resources.MergedDictionaries[0];


        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                logger.Error(exception, message);
            }
        }
    }
}