using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Utils;

namespace Memenim.Converters
{
    public sealed class UnixTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                long result = 0;

                if (value is long longValue)
                    result = longValue;

                return TimeUtils.UnixTimeStampToDateTime(
                        result <= 0
                            ? 0
                            : result)
                    .ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
                return TimeUtils.UnixTimeStampToDateTime(0)
                    .ToString(CultureInfo.CurrentCulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
