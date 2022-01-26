using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Unosquare.FFME;
using VideoCompressorGUI.Utils.Logger;

namespace VideoCompressorGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Log logger = new();

        public App()
        {
            Library.FFmpegDirectory = Path.GetDirectoryName(typeof(App).Assembly.Location) + @"\libs\bin";
            Task.Run(async () => {await Library.LoadFFmpegAsync();}).GetAwaiter().GetResult();
        }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
#if DEBUG
            logger.LogToFile = false;
#endif
            SetupExceptionHandling();
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
