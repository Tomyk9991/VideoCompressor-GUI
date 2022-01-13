using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class MainMenuStrip : UserControl
{
    public MainMenuStrip()
    {
        InitializeComponent();
    }

    private void Exit_OnClick(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Settings_OnClick(object sender, RoutedEventArgs e)
    {
        ((MainWindow)Application.Current.MainWindow).PushContentControl(new Settings());
    }
}