using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Schema;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class UserPurposeTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = UserPurposeType.Unknown;

            if (value is int intValue)
                result = (UserPurposeType)((byte)intValue);
            else if (value is UserPurposeType typeValue)
                result = typeValue;

            return result.GetLocalizedName();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = UserPurposeType.Unknown;

            if (value is string stringValue)
            {
                result = UserPurposeType.Unknown
                    .ParseLocalizedName(stringValue);
            }

            return result;
        }
    }
}
