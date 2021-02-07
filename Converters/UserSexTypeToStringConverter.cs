using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Schema;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class UserSexTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserSexType result = UserSexType.Unknown;

            if (value is int intValue)
            {
                if (intValue == 0)
                    return null;

                result = (UserSexType)((byte)intValue);
            }

            return result.GetLocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is string stringValue)
                result = (byte)UserSexType.Unknown.ParseLocalizedName<UserSexType>(stringValue);

            return result;
        }
    }
}
