using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Memenim.Core.Schema;

namespace Memenim.Converters
{
    public sealed class UserStatusTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UserStatusType result = UserStatusType.Active;

            if (value is int intValue)
                result = (UserStatusType)((byte)intValue);
            else if (value is UserStatusType typeValue)
                result = typeValue;

            return result != UserStatusType.Active
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
