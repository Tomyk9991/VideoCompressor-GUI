using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls;

public partial class Settings : UserControl
{
    public Settings()
    {
        InitializeComponent();
    }

    private void CloseSettings_OnClick(object sender, RoutedEventArgs e)
    {
        // Save settings()
        ((MainWindow) Application.Current.MainWindow).PopContentControl();
    }
}