using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Localization
{
    public static class LocalizationManager
    {
        private static string GetAppName()
        {
            return GetAppName(MainWindow.Instance);
        }
        private static string GetAppName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');

            return elNames[0];
        }

        private static string GetElementName()
        {
            return GetElementName(MainWindow.Instance);
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

        private static string GetLocXamlFilePath(string сultureName)
        {
            return GetLocXamlFilePath(MainWindow.Instance, сultureName);
        }
        private static string GetLocXamlFilePath(FrameworkElement element, string сultureName)
        {
            string locXamlFile = $"{GetElementName(element)}.{сultureName}.xaml";
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            return string.IsNullOrEmpty(directory)
                ? Path.Combine("Localization", locXamlFile)
                : Path.Combine(directory, "Localization", locXamlFile);
        }

        private static Task SetLanguageResourceDictionary(string resourceFile)
        {
            return SetLanguageResourceDictionary(MainWindow.Instance, resourceFile);
        }
        private static async Task SetLanguageResourceDictionary(FrameworkElement element, string resourceFilePath)
        {
            if (!File.Exists(resourceFilePath))
            {
                await DialogManager.ShowDialog("F U C K", "'" + resourceFilePath + "' not found.")
                    .ConfigureAwait(true);
                return;
            }

            ResourceDictionary languageDictionary = new ResourceDictionary
            {
                Source = new Uri(resourceFilePath)
            };

            if (!languageDictionary.Contains("ResourceDictionaryName")
                || languageDictionary["ResourceDictionaryName"].ToString()?.StartsWith("loc-") != true)
            {
                return;
            }

            int dictionaryIndex = -1;
            bool defaultLanguageDictionary = true;

            for (var i = 0; i < element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary dictionary = element.Resources.MergedDictionaries[i];

                if (!dictionary.Contains("ResourceDictionaryName")
                    || dictionary["ResourceDictionaryName"].ToString()?.StartsWith("loc-") != true)
                {
                    continue;
                }

                if (defaultLanguageDictionary)
                {
                    defaultLanguageDictionary = false;
                    continue;
                }

                dictionaryIndex = i;
            }

            if (dictionaryIndex == -1)
            {
                element.Resources.MergedDictionaries.Add(languageDictionary);
                return;
            }

            element.Resources.MergedDictionaries[dictionaryIndex] = languageDictionary;
        }

        public static string GetCurrentCultureName()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public static Task SetDefaultLanguage()
        {
            return SetDefaultLanguage(MainWindow.Instance);
        }
        public static Task SetDefaultLanguage(FrameworkElement element)
        {
            return SwitchLanguage(element, GetCurrentCultureName());
        }

        public static Task SwitchLanguage(string сultureName)
        {
            return SwitchLanguage(MainWindow.Instance, сultureName);
        }
        public static async Task SwitchLanguage(FrameworkElement element, string сultureName)
        {
            await SetLanguageResourceDictionary(element, GetLocXamlFilePath(element, сultureName))
                .ConfigureAwait(true);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(сultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(сultureName);

            SettingsManager.AppSettings.Language = сultureName;

            SettingsManager.AppSettings.Save();
        }
    }
}
