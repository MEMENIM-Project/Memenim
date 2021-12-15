using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Memenim.Core.Schema;

namespace Memenim.Converters
{
    public sealed class UserPurposeTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = UserPurposeType.Unknown;

            if (value is int intValue)
                result = (UserPurposeType)((byte)intValue);
            else if (value is UserPurposeType typeValue)
                result = typeValue;

            return result != UserPurposeType.Unknown
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
