using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class HeaderedSeperator : UserControl
{
    public static DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(VisualStyleElement.Header), typeof(string), typeof(HeaderedSeperator));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public HeaderedSeperator()
    {
        InitializeComponent();
    }
}