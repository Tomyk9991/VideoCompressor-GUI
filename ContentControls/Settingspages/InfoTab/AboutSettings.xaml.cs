using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VideoCompressorGUI.Utils.Github;

namespace VideoCompressorGUI.ContentControls.Settingspages.InfoTab
{
    public partial class AboutSettings : UserControl
    {
        private bool updateNext = false;
        public AboutSettings()
        {
            InitializeComponent();
            versionTextBlock.Text = "Version: " + typeof(AboutSettings).Assembly.GetName().Version;
        }
        
        private void OpenURL(string url)
        {
            System.Diagnostics.Process.Start("explorer", url);
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            OpenURL("https://github.com/Tomyk9991");
        }

        private void SourceCode_Click(object sender, RoutedEventArgs e)
        {
            OpenURL("https://github.com/Tomyk9991/VideoCompressor-GUI");
        }

        private async void OnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!updateNext)
            {
                // GithubReleaseCheck checker = new GithubReleaseCheck();
                // GithubResponse response = await checker.FetchData();
                //
                // if (checker.Check())
                // {
                //     checker.OnDownloadFinished += () =>
                //     {
                //         this.newUpdateAvailableTextBlock.Text = response.ChangeLogs;
                //     };
                //     
                //     await checker.DownloadNewest();

                    updateButton.Content = "Update durchführen (Neustart erforderlich)";
                    updateNext = true;
                // }
            }
            else
            {
                // Code, für den Fall, dass das Update durchgeführt werden soll
                // https://stackoverflow.com/questions/22891580/update-c-sharp-application-replace-exe-file
            }
        }
    }
}
