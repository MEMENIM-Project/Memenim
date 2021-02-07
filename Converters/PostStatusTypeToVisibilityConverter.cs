using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Memenim.Core.Schema;

namespace Memenim.Converters
{
    public sealed class PostStatusTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PostStatusType result = PostStatusType.Premoderating;

            if (value is int intValue)
                result = (PostStatusType)((byte)intValue);
            else if (value is PostStatusType typeValue)
                result = typeValue;

            return result != PostStatusType.Published
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
