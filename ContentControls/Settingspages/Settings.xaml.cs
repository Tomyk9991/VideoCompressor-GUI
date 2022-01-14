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
        // lower settings pages are responsible for their saves
        ((MainWindow) Application.Current.MainWindow).PopContentControl();
    }
}