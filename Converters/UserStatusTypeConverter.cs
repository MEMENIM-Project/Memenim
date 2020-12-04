using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class UserStatusTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserStatusType result = UserStatusType.Active;

            if (value is int intValue)
                result = (UserStatusType)((byte)intValue);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is UserStatusType enumValue)
                result = (byte)enumValue;

            return result;
        }
    }
}
