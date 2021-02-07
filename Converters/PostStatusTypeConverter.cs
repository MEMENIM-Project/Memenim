using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Schema;

namespace Memenim.Converters
{
    public sealed class PostStatusTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PostStatusType result = PostStatusType.Premoderating;

            if (value is int intValue)
                result = (PostStatusType)((byte)intValue);
            else if (value is PostStatusType typeValue)
                result = typeValue;

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is int intValue)
                result = intValue;
            else if (value is PostStatusType typeValue)
                result = (byte)typeValue;

            return result;
        }
    }
}
