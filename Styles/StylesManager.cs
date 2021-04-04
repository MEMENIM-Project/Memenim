using System;
using System.Windows;

namespace Memenim.Styles
{
    public static class StylesManager
    {
        private static string GetStyleXamlFilePath(string styleName)
        {
            string styleXamlFileName = $"{styleName}.xaml";

            return $"pack://application:,,,/Styles/{styleXamlFileName}";
        }

        private static ResourceDictionary GetStyleResourceDictionary(string resourceFilePath)
        {
            if (!Uri.TryCreate(resourceFilePath, UriKind.RelativeOrAbsolute, out _))
                return null;

            ResourceDictionary styleDictionary = new ResourceDictionary
            {
                Source = new Uri(resourceFilePath)
            };

            return styleDictionary;
        }

        public static ResourceDictionary GetStyle(string styleName)
        {
            return GetStyleResourceDictionary(GetStyleXamlFilePath(styleName));
        }
    }
}
