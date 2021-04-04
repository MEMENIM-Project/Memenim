using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Dialogs;
using Memenim.Utils;

namespace Memenim.TabLayouts
{
    public static class TabLayoutsManager
    {
        private static string GetAppName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');

            return elNames[0];
        }

        private static string GetElementName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');

            var elName = string.Empty;
            if (elNames.Length >= 2)
                elName = elNames[^1];

            return elName;
        }

        private static string GetLayoutXamlFilePath(FrameworkElement element, string layoutTypeName)
        {
            string elementName = GetElementName(element);
            string layoutXamlFileName = $"{layoutTypeName}.xaml";

            return $"pack://application:,,,/TabLayouts/{elementName}/{layoutXamlFileName}";
        }

        private static async Task<ResourceDictionary> GetLayoutResourceDictionary(FrameworkElement element, string resourceFilePath)
        {
            if (!Uri.TryCreate(resourceFilePath, UriKind.RelativeOrAbsolute, out _))
            {
                var fileTitle = LocalizationUtils.GetLocalized("FileTitle");
                var notFoundTitle = LocalizationUtils.GetLocalized("NotFoundTitle1");

                await DialogManager.ShowErrorDialog($"{fileTitle} '{resourceFilePath}' {notFoundTitle.ToLower()}")
                    .ConfigureAwait(true);
                return null;
            }

            string elementName = GetElementName(element);

            ResourceDictionary layoutDictionary = new ResourceDictionary
            {
                Source = new Uri(resourceFilePath)
            };

            if (!layoutDictionary.Contains("ResourceDictionaryName")
                || layoutDictionary["ResourceDictionaryName"].ToString()?.StartsWith($"layout-{elementName}-") != true)
            {
                return null;
            }

            return layoutDictionary;
        }

        public static Task<ResourceDictionary> GetLayout(FrameworkElement element, string layoutTypeName)
        {
            return GetLayoutResourceDictionary(element, GetLayoutXamlFilePath(element, layoutTypeName));
        }
    }
}
