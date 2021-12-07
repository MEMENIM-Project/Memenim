using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Memenim.Generating;
using Memenim.Styles.Loading.Entities;

namespace Memenim.Styles
{
    public static class StylesManager
    {
        private static ReadOnlyCollection<LoadingStyle> LoadingStyles { get; }



        static StylesManager()
        {
            LoadingStyles = GetLoadingStyles();
        }



        private static string GetStyleXamlFilePath(
            string categoryName, string styleName)
        {
            var styleXamlFileName = $"{styleName}.xaml";

            categoryName = categoryName
                .Replace('\\', '/');

            return $"pack://application:,,,/Styles/{categoryName}/{styleXamlFileName}";
        }

        private static ResourceDictionary GetStyleResourceDictionary(
            string resourceFilePath)
        {
            if (!Uri.TryCreate(resourceFilePath, UriKind.RelativeOrAbsolute, out _))
                return null;

            var styleDictionary = new ResourceDictionary
            {
                Source = new Uri(resourceFilePath)
            };

            return styleDictionary;
        }

        private static ReadOnlyCollection<LoadingStyle> GetLoadingStyles()
        {
            var loadingStyles = new List<LoadingStyle>();

            void AddStyle(string styleName)
            {
                styleName += "Theme";

                loadingStyles.Add(new LoadingStyle(
                    styleName, GetStyle("Loading", styleName)));
            }

            AddStyle("SmileWithTear");
            AddStyle("Marina");
            AddStyle("Kurtka");

            return new ReadOnlyCollection<LoadingStyle>(
                loadingStyles);
        }



        public static ResourceDictionary GetStyle(
            string categoryName, string styleName)
        {
            return GetStyleResourceDictionary(
                GetStyleXamlFilePath(
                    categoryName, styleName));
        }

        public static LoadingStyle GetRandomLoadingStyle()
        {
            var index = (int)GeneratingManager.RandomGenerator
                .GetNormalizedIndex((uint)LoadingStyles.Count);

            return LoadingStyles[index];
        }
    }
}
