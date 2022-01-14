using System;
using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls.Components;

public partial class CollapsibleGroupBox : UserControl
{
    public static DependencyProperty CollapsibleContentProperty = DependencyProperty.Register(nameof(CollapsibleContent), typeof(UIElement), typeof(CollapsibleGroupBox));
    public static DependencyProperty IsVisibleContentProperty = DependencyProperty.Register(nameof(IsVisibleContent), typeof(bool), typeof(CollapsibleGroupBox));
    public static DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(CollapsibleGroupBox));

    // passing the new value
    public event Action<CollapsibleGroupBox, bool> IsVisibilityChanged;
    
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
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

    public void SetIsVisibleContentWithoutEventFire(bool newValue)
    {
        SetValue(IsVisibleContentProperty, newValue);
    }
    
    private void IsVisibleChanged(bool newValue)
    {
        collapsibleContentControl.Visibility = newValue ? Visibility.Visible : Visibility.Collapsed;
        IsVisibilityChanged?.Invoke(this, newValue);
    }

    private void ToggleButton_OnChecked(object sender, RoutedEventArgs e) => IsVisibleChanged(true);

    private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e) => IsVisibleChanged(false);
}