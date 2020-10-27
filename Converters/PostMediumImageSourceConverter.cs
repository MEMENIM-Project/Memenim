using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class PostMediumImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || !Uri.TryCreate((string)value, UriKind.Absolute, out Uri _)
                ? DependencyProperty.UnsetValue //Binding.DoNothing
                : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || !Uri.TryCreate((string)value, UriKind.Absolute, out Uri _)
                ? DependencyProperty.UnsetValue //Binding.DoNothing
                : value;
        }
    }
}
