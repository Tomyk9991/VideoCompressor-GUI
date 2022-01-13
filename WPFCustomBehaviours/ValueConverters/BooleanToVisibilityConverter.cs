using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValueConverters;

public class BooleanToVisibilityConverter : IValueConverter
{
    public Boolean InvertVisibility { get; set; }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        
        if (targetType == typeof(Visibility))
        {
            var visible = System.Convert.ToBoolean(value, culture);
            if (InvertVisibility)
                visible = !visible;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        throw new InvalidOperationException("Converter can only convert to value of type Visibility");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new InvalidOperationException("Converter cannot convert back");
    }
}