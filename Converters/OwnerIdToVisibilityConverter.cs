using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Memenim.Settings;

namespace Memenim.Converters
{
    public sealed class OwnerIdToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var result = -1;

            if (value is int intValue)
                result = intValue;

            return result != -1
                   && result == SettingsManager.PersistentSettings.CurrentUser.Id
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
