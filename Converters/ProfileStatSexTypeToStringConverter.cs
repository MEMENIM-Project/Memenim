using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class ProfileStatSexTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProfileStatSexType result = ProfileStatSexType.Unknown;

            if (value is int intValue)
            {
                if (intValue == 0)
                    return null;

                result = (ProfileStatSexType)((byte)intValue);
            }

            return result.GetLocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is string stringValue)
                result = (byte)ProfileStatSexType.Unknown.ParseLocalizedName<ProfileStatSexType>(stringValue);

            return result;
        }
    }
}
