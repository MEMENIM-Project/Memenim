using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Memenim.Core.Schema;

namespace Memenim.Converters
{
    public sealed class UserSexTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserSexType result = UserSexType.Unknown;

            if (value is int intValue)
                result = (UserSexType)((byte)intValue);
            else if (value is UserSexType typeValue)
                result = typeValue;

            return result != UserSexType.Unknown
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
