using System;
using System.Globalization;
using System.Windows.Data;

namespace Memenim.Converters
{
    public sealed class ProfileStatSexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is int intValue)
            {
                if (intValue == 0)
                    return null;

                result = ProfileStatSex.ParseValue((byte) intValue);
            }

            return result != null
                ? MainWindow.Instance.FindResource(ProfileStatSex.GetResourceKey(result))
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int result = 0;

            if (value is string stringValue)
                result = ProfileStatSex.ParseName(stringValue);

            return result;
        }
    }
}
