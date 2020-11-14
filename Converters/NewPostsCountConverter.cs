using System;
using System.Globalization;
using System.Windows.Data;
using RIS.Extensions;

namespace Memenim.Converters
{
    public sealed class NewPostsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return $"+{((int)(value ?? 0) > 0 ? value : 0)}";
            }
            catch (Exception)
            {
                return "+0";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return !string.IsNullOrEmpty(((string)value)?.TrimStart('+'))
                    ? ((string)value).TrimStart('+').ToInt()
                    : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
