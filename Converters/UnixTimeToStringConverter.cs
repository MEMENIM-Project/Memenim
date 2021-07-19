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
                var result = 0UL;

                if (value is long longValue)
                    result = (ulong)longValue;
                else if (value is ulong ulongValue)
                    result = ulongValue;

                return TimeUtils.ToDateTime(result)
                    .ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception)
            {
                return TimeUtils.ToDateTime(0UL)
                    .ToString(CultureInfo.CurrentCulture);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var result = string.Empty;

                if (value is string stringValue)
                    result = stringValue;

                return TimeUtils.ToUnixTimeStamp(
                    DateTime.Parse(
                        result,
                        CultureInfo.CurrentCulture));
            }
            catch (Exception)
            {
                var result = TimeUtils.ToDateTime(0UL)
                    .ToString(CultureInfo.CurrentCulture);

                return TimeUtils.ToUnixTimeStamp(
                    DateTime.Parse(
                        result,
                        CultureInfo.CurrentCulture));
            }
        }
    }
}
