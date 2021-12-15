using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class NewPostsCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = 0;

            if (value is int intValue)
                result = intValue;

            return result != -1
                   && result != 0
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
