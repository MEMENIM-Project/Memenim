using System;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Dialogs;
using Memenim.Utils;

namespace Memenim.Layouts
{
    public static class LayoutsManager
    {
        private static string GetElementName(
            FrameworkElement element)
        {
            return element.GetType()
                .Name;
        }

        private static string GetLayoutXamlFilePath(
            FrameworkElement element, string layoutTypeName)
        {
            var elementName = GetElementName(element);
            var layoutXamlFileName = $"{layoutTypeName}.xaml";

            return $"pack://application:,,,/Layouts/{elementName}/{layoutXamlFileName}";
        }

        private static async Task<ResourceDictionary> GetLayoutResourceDictionary(
            FrameworkElement element, string resourceFilePath)
        {
            if (!Uri.TryCreate(resourceFilePath, UriKind.RelativeOrAbsolute, out _))
            {
                var fileTitle = LocalizationUtils.GetLocalized("FileTitle");
                var notFoundTitle = LocalizationUtils.GetLocalized("NotFoundTitle1");

                await DialogManager.ShowErrorDialog($"{fileTitle} '{resourceFilePath}' {notFoundTitle.ToLower()}")
                    .ConfigureAwait(true);
                return null;
            }

            var elementName = GetElementName(element);

            var layoutDictionary = new ResourceDictionary
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



        public static Task<ResourceDictionary> GetLayout(
            FrameworkElement element, string layoutTypeName)
        {
            return GetLayoutResourceDictionary(element,
                GetLayoutXamlFilePath(element, layoutTypeName));
        }
    }
}
