using System;
using System.Globalization;
using System.Windows.Data;
using RIS.Extensions;

namespace Memenim.Converters
{
    public sealed class NewPostsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                var result = 0;

                if (value is int intValue)
                    result = intValue;

                if (result < 0)
                    result = 0;
                if (result > 99)
                    result = 99;

                return $"+{result}";
            }
            catch (Exception)
            {
                return "+0";
            }
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                return ((string)value)?
                    .TrimStart('+')
                    .ToInt() ?? 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
