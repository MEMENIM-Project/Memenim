using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Schema;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class UserPurposeTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserPurposeType result = UserPurposeType.Unknown;

            if (value is int intValue)
            {
                if (intValue == 0)
                    return null;

                result = (UserPurposeType)((byte)intValue);
            }

            return result.GetLocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is string stringValue)
                result = (byte)UserPurposeType.Unknown.ParseLocalizedName<UserPurposeType>(stringValue);

            return result;
        }
    }
}
