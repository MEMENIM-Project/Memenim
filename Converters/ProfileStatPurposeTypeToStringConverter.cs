using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class ProfileStatPurposeTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ProfileStatPurposeType result = ProfileStatPurposeType.Unknown;

            if (value is int intValue)
            {
                if (intValue == 0)
                    return null;

                result = (ProfileStatPurposeType)((byte)intValue);
            }

            return result.GetLocalizedName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is string stringValue)
                result = (byte)ProfileStatPurposeType.Unknown.ParseLocalizedName<ProfileStatPurposeType>(stringValue);

            return result;
        }
    }
}
