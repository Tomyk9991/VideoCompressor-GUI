using System;
using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class CollapsibleGroupBox : UserControl
{
    public static DependencyProperty CollapsibleContentProperty = DependencyProperty.Register("CollapsibleContent", typeof(UIElement), typeof(CollapsibleGroupBox));
    public static DependencyProperty IsVisibleContentProperty = DependencyProperty.Register("IsVisibleContent", typeof(bool), typeof(CollapsibleGroupBox));
    
    public bool IsVisibleContent
    {
        get => (bool)GetValue(IsVisibleContentProperty);
        set { SetValue(IsVisibleContentProperty, value); IsVisibleChanged(value); }
    }


    public UIElement CollapsibleContent
    {
        get => (UIElement)GetValue(CollapsibleContentProperty);
        set => SetValue(CollapsibleContentProperty, value);
    }

    public CollapsibleGroupBox()
    {
        InitializeComponent();
    }

    private void IsVisibleChanged(bool newValue)
    {
        collapsibleContentControl.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e) => IsVisibleChanged(true);

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e) => IsVisibleChanged(false);
}