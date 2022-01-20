using System;
using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls;

public partial class Settings : UserControl
{
    public static event Func<bool> OnClosingSettings;
    public Settings()
    {
        InitializeComponent();
    }

    private void CloseSettings_OnClick(object sender, RoutedEventArgs e)
    {
        bool b = OnClosingSettings.Invoke();

        if (!b)
        {
            // lower settings pages are responsible for their saves
            ((MainWindow) Application.Current.MainWindow).PopContentControl();
        }
    }
}