using System;
using System.Globalization;
using System.Windows.Data;
using RIS.Extensions;

namespace Memenim.Converters
{
	public sealed class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).ToInt();
        }
    }
}
