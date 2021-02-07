using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Schema;

namespace Memenim.Converters
{
    public sealed class UserStatusTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserStatusType result = UserStatusType.Active;

            if (value is int intValue)
                result = (UserStatusType)((byte)intValue);
            else if (value is UserStatusType typeValue)
                result = typeValue;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is int intValue)
                result = intValue;
            else if (value is UserStatusType typeValue)
                result = (byte)typeValue;

            return result;
        }
    }
}
