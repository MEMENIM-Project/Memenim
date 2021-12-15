using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Memenim.Widgets;

namespace Memenim.Converters
{
    public sealed class ProfileStatCollectionToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            UIElementCollection result = null;
            var resultParameter = 0;

            if (value is UIElementCollection collectionValue)
                result = collectionValue;

            if (result == null)
                return Visibility.Collapsed;

            if (parameter is int intParameter)
                resultParameter = intParameter;

            for (var i = 0; i < result.Count - resultParameter; ++i)
            {
                if (!(result[i] is UserProfileStat element))
                    continue;

                if (element.Visibility == Visibility.Visible)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
