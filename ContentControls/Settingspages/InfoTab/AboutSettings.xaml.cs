using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VideoCompressorGUI.ContentControls.Settingspages.InfoTab
{
    public partial class AboutSettings : UserControl
    {
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

        private void OnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}