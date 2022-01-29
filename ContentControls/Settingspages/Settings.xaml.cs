using System;
using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls
{
    public enum SettingsPage
    {
        General = 0,
        Pathrules = 1,
        VideoBrowser = 2,
        About = 3
    }
    
    public partial class Settings : UserControl
    {
        public static event Func<bool> OnClosingSettings;

        public Settings(SettingsPage page = SettingsPage.General)
        {
            InitializeComponent();
            tabControl.SelectedIndex = (int)page;
        }

        private void CloseSettings_OnClick(object sender, RoutedEventArgs e)
        {
            bool b = OnClosingSettings.Invoke();

            if (!b)
            {
                // lower settings pages are responsible for their saves
                ((MainWindow)Application.Current.MainWindow).PopContentControl();
            }
        }
    }
}