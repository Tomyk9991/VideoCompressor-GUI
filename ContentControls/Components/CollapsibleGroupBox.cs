using System;
using System.Windows;
using System.Windows.Controls;

namespace VideoCompressorGUI.ContentControls.Components;

public class CollapsibleGroupBox : ContentControl
{
    private bool _isVisibleContent;
    
    public static DependencyProperty IsVisibleContentProperty =
        DependencyProperty.Register(nameof(IsVisibleContent), typeof(bool), typeof(CollapsibleGroupBox)
        , new PropertyMetadata(IsVisibleContentChanged));


    public static DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(CollapsibleGroupBox)
        , new PropertyMetadata(HeaderChanged));


    private static void HeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CollapsibleGroupBox) d).Header = (string) e.NewValue;
    }
    
    private static void IsVisibleContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((CollapsibleGroupBox)d).IsVisibleContent = (bool) e.NewValue;
    }


    // passing the new value
    public event Action<CollapsibleGroupBox, bool> IsVisibilityChanged;


    public string Header { get; set; }
    public bool IsVisibleContent
    {
        get => _isVisibleContent;
        set 
        { 
            SetValue(IsVisibleContentProperty, value);
            _isVisibleContent = value;
            IsVisibleChanged(value);
        }
    }
    
    public void SetIsVisibleContentWithoutEventFire(bool newValue)
    {
        _isVisibleContent = newValue;
        SetValue(IsVisibleContentProperty, newValue);
    }

    private void IsVisibleChanged(bool newValue)
    {
        IsVisibilityChanged?.Invoke(this, newValue);
    }
}