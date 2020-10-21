using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class UserBannerImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || !Uri.TryCreate((string)value, UriKind.Absolute, out Uri _)
                ? "pack://application:,,,/resources/placeholder_avatar.jpg"
                : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || !Uri.TryCreate((string)value, UriKind.Absolute, out Uri _)
                ? "pack://application:,,,/resources/placeholder_avatar.jpg"
                : value;
        }
    }
}
