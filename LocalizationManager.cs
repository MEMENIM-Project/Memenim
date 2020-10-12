using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using AnonymDesktopClient.Core.Settings;

namespace AnonymDesktopClient.Core
{
    public static class LocalizationManager
    {
        public static FrameworkElement MainWindow;

        private static string GetAppName()
        {
            return GetAppName(MainWindow);
        }
        private static string GetAppName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');
            return elNames[0];
        }

        private static string GetElementName()
        {
            return GetElementName(MainWindow);
        }
        private static string GetElementName(FrameworkElement element)
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');
            var elName = "";
            if (elNames.Length >= 2) elName = elNames[elNames.Length - 1];
            return elName;
        }

        private static string GetLocXamlFilePath(string element, string сultureName)
        {
            string locXamlFile = $"{element}.{сultureName}.xaml";
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            return string.IsNullOrEmpty(directory)
                ? Path.Combine("Localization", locXamlFile)
                : Path.Combine(directory, "Localization", locXamlFile);
        }

        private static void SetLanguageResourceDictionary(string resourceFile)
        {
            SetLanguageResourceDictionary(MainWindow, resourceFile);
        }
        private static void SetLanguageResourceDictionary(FrameworkElement element, string resourceFile)
        {
            if (!File.Exists(resourceFile))
                DialogManager.ShowDialog("F U C K", "'" + resourceFile + "' not found.");

            ResourceDictionary languageDictionary = new ResourceDictionary
            {
                Source = new Uri(resourceFile)
            };

            int langDictId = -1;

            for (int i = 0; i < element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary md = element.Resources.MergedDictionaries[i];

                if (!md.Contains("ResourceDictionaryName") ||
                    md["ResourceDictionaryName"].ToString()?.StartsWith("Loc-") != true)
                {
                    continue;
                }

                langDictId = i;
                break;
            }

            if (langDictId == -1)
                element.Resources.MergedDictionaries.Add(languageDictionary);
            else
                element.Resources.MergedDictionaries[langDictId] = languageDictionary;
        }

        public static string GetCurrentCultureName()
        {
            return CultureInfo.CurrentUICulture.Name;
        }

        public static void SetDefaultLanguage()
        {
            SetDefaultLanguage(MainWindow);
        }
        public static void SetDefaultLanguage(FrameworkElement element)
        {
            SetLanguageResourceDictionary(element, GetLocXamlFilePath(GetElementName(element), GetCurrentCultureName()));
        }

        public static void SwitchLanguage(string сultureName)
        {
            SwitchLanguage(MainWindow, сultureName);
        }
        public static void SwitchLanguage(FrameworkElement element, string сultureName)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(сultureName);
            SetLanguageResourceDictionary(element, GetLocXamlFilePath(GetElementName(element), сultureName));

            SettingManager.AppSettings.Language = сultureName;
            SettingManager.AppSettings.Save();
        }
    }
}
