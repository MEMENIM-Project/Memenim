using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (value is bool boolValue)
                result = boolValue;

            return result
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue)
                return visibilityValue == Visibility.Visible;

            return false;
        }
    }
}
