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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            UIElementCollection result = null;
            int resultParameter = 0;

            if (value is UIElementCollection collectionValue)
                result = collectionValue;

            if (result == null)
                return Visibility.Collapsed;

            if (parameter is int intParameter)
                resultParameter = intParameter;

            for (int i = 0; i < result.Count - resultParameter; ++i)
            {
                UserProfileStat element = result[i] as UserProfileStat;

                if (element == null)
                    continue;

                if (element.Visibility == Visibility.Visible)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
