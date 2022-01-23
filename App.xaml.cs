using System;
using System.Threading.Tasks;
using System.Windows;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Log logger = new();
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupExceptionHandling();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                logger.Error((Exception)e.ExceptionObject);

            DispatcherUnhandledException += (s, e) =>
                logger.Error(e.Exception);

            TaskScheduler.UnobservedTaskException += (s, e) =>
                logger.Error(e.Exception);
        }
    }
}
