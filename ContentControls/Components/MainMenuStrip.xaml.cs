using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VideoCompressorGUI.ContentControls.Settingspages;
using VideoCompressorGUI.Utils.Github;

namespace VideoCompressorGUI.ContentControls.Components
{
    public partial class MainMenuStrip : UserControl
    {
        public MainMenuStrip()
        {
            InitializeComponent();


            Dispatcher.Invoke(async () =>
            {
                var ghReleaseChecker = new GithubReleaseCheck();
                await ghReleaseChecker.FetchData();
            
                hasUpdateNotification.Visibility = ghReleaseChecker.Check() ? Visibility.Visible : Visibility.Collapsed;
            });
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Settings_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PushContentControl(new Settings());
        }

        private void PresetsEdit_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow) Application.Current.MainWindow).PushContentControl(new PresetsEditor());
        }

        private void OnUpdateNotification_OnClick(object sender, MouseButtonEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).PushContentControl(new Settings(SettingsPage.About));
        }
    }
}

