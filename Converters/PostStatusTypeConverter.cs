using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class PostStatusTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PostStatusType result = PostStatusType.Premoderating;

            if (value is int intValue)
                result = (PostStatusType)((byte)intValue);

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is PostStatusType enumValue)
                result = (byte)enumValue;

            return result;
        }
    }
}
