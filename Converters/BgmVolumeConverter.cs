using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class BgmVolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return doubleValue * 100;

            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
                return doubleValue / 100;

            return 0.0;
        }
    }
}
