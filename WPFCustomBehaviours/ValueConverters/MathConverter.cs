using System;
using System.Globalization;
using System.Windows.Data;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValueConverters
{
    public class MathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new ArgumentException("Target type has to be of type string");

            if ((double)value <= 0.0d || (double) value > 1.0d)
                return "";
        
        
            return ((double)value * 100.0d).ToString("F1") + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}