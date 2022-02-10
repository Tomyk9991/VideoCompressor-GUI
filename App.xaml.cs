using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME;
using VideoCompressorGUI.ContentControls.Settingspages.GeneralSettingsTab;
using VideoCompressorGUI.SettingsLoadables;
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
#if DEBUG
            logger.LogToFile = false;
#endif
            SetupExceptionHandling();

            var data = SettingsFolder.Load<GeneralSettingsData>();
            
            if (string.IsNullOrEmpty(data.Language))
                data.Language = new GeneralSettingsData().CreateDefault().Language;
            
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(data.Language);
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                logger.Error((Exception)e.ExceptionObject);

            DispatcherUnhandledException += (s, e) =>
            {
                logger.Error(e.Exception);
                e.Handled = true;
                Application.Current.Shutdown();
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
                logger.Error(e.Exception);
        }
    }
}
