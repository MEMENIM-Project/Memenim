using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class UserBannerImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return result == null || !Uri.TryCreate(result, UriKind.Absolute, out Uri _)
                ? "pack://application:,,,/Resources/Images/Placeholders/placeholder_avatar.jpg"
                : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return result == null || !Uri.TryCreate(result, UriKind.Absolute, out Uri _)
                                  || result == "pack://application:,,,/Resources/Images/Placeholders/placeholder_avatar.jpg"
                ? string.Empty
                : result;
        }
    }
}
