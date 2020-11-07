using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class PostImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return result == null || !Uri.TryCreate(result, UriKind.Absolute, out Uri _)
                ? DependencyProperty.UnsetValue //Binding.DoNothing
                : result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return result == null || !Uri.TryCreate(result, UriKind.Absolute, out Uri _)
                ? string.Empty
                : result;
        }
    }
}
