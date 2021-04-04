using System;
using System.Windows;

namespace Memenim.Styles
{
    public static class StylesManager
    {
        private static string GetStyleXamlFilePath(
            string categoryName, string styleName)
        {
            string styleXamlFileName = $"{styleName}.xaml";

            return $"pack://application:,,,/Styles/{categoryName}/{styleXamlFileName}";
        }

        private static ResourceDictionary GetStyleResourceDictionary(
            string resourceFilePath)
        {
            if (!Uri.TryCreate(resourceFilePath, UriKind.RelativeOrAbsolute, out _))
                return null;

            ResourceDictionary styleDictionary = new ResourceDictionary
            {
                Source = new Uri(resourceFilePath)
            };

            return styleDictionary;
        }

        public static ResourceDictionary GetStyle(
            string categoryName, string styleName)
        {
            return GetStyleResourceDictionary(
                GetStyleXamlFilePath(
                    categoryName, styleName));
        }
    }
}
