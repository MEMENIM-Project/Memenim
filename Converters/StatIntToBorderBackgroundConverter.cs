using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Memenim.Converters
{
    public sealed class StatIntToBorderBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = 0;

            if (value is int intValue)
                result = intValue;

            return result > 0
                ? Brushes.ForestGreen.Color
                    .ToString()
                : System.Windows.Application.Current
                    .FindResource("MahApps.Brushes.Gray3")?
                    .ToString();
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
