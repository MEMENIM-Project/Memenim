using System;
using System.Windows;
using System.Windows.Media;

namespace Memenim.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T GetChildOfType<T>(this DependencyObject source)
            where T : DependencyObject
        {
            if (source == null)
                return null;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(source); ++i)
            {
                var child = VisualTreeHelper
                    .GetChild(source, i);
                var result = (child as T) ?? GetChildOfType<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
