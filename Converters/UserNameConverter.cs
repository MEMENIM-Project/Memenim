using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class UserNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return string.IsNullOrEmpty(result)
                ? "Unknown"
                : result;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return string.IsNullOrEmpty(result)
                   || result == "Unknown"
                ? string.Empty
                : result;
        }
    }
}
