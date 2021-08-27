using System;
using System.Globalization;
using System.Windows.Data;
using Memenim.Core.Api;
using Memenim.Extensions;

namespace Memenim.Converters
{
    public sealed class PostCategorySchemaToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = -1;

            if (value is int intValue)
                result = intValue;

            if (!PostApi.PostCategories.TryGetValue(result, out var category))
                return string.Empty;

            return category?.GetLocalizedName() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = null;

            if (value is string stringValue)
                result = stringValue;

            var category = PostCategorySchemaExtensions
                .ParseLocalizedName(result);

            return category?.Name ?? string.Empty;
        }
    }
}
