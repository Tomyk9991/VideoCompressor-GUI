using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace VideoCompressorGUI.WPFCustomBehaviours.ValueConverters
{
    public class FileNameFromPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}