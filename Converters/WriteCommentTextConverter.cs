using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class WriteCommentTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return result
                   ?? Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            return result
                   ?? Binding.DoNothing;
        }
    }
}
