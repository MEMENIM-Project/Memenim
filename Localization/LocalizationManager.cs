using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Dialogs;
using Memenim.Settings;
using Memenim.Utils;
using RIS.Extensions;
using Environment = RIS.Environment;

namespace Memenim.Localization
{
    public static class LocalizationManager
    {
        public static event EventHandler<EventArgs> LanguageChanged;

        static LocalizationManager()
        {
            var directory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(directory) || directory == "Unknown")
                return;

            directory = Path.Combine(directory, "localizations");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        private static IEnumerable<KeyValuePair<string, string>> EnumerateLocales(
            string directoryBasePath, string directoryRelativePath)
        {
            return EnumerateLocales(MainWindow.Instance,
                directoryBasePath, directoryRelativePath);
        }
        private static IEnumerable<KeyValuePair<string, string>> EnumerateLocales<T>(
            T element, string directoryBasePath, string directoryRelativePath)
            where T : FrameworkElement, ILocalizable
        {
            var result = Enumerable.Empty<KeyValuePair<string, string>>();

            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return result;
            }

            directory = Path.Combine(directory, directoryRelativePath);

            if (!Directory.Exists(directory))
                return result;

            var locales = new Dictionary<string, string>();
            var elementName = GetElementName(element);

            foreach (var localeFile in DirectoryExtensions.EnumerateAllFiles(directory))
            {
                var localeFileName = Path.GetFileName(localeFile);

                if (!localeFileName.StartsWith($"{elementName}.")
                    || !localeFileName.EndsWith(".xaml"))
                {
                    continue;
                }

                ResourceDictionary languageDictionary;

                try
                {
                    languageDictionary = new ResourceDictionary
                    {
                        Source = new Uri(localeFile)
                    };
                }
                catch (Exception)
                {
                    continue;
                }

                if (!languageDictionary.Contains("ResourceDictionaryName")
                    || languageDictionary["ResourceDictionaryName"].ToString()?.StartsWith("loc-") != true)
                {
                    continue;
                }

                if (!languageDictionary.Contains("ResourceLocaleName")
                    || !languageDictionary.Contains("ResourceCultureName"))
                {
                    continue;
                }

                string localeName = languageDictionary["ResourceLocaleName"].ToString();
                string cultureName = languageDictionary["ResourceCultureName"].ToString();

                if (string.IsNullOrWhiteSpace(localeName)
                    || string.IsNullOrWhiteSpace(cultureName))
                {
                    continue;
                }

                locales[cultureName] = localeName;
            }

            return locales;
        }

        private static void LoadLocales()
        {
            LoadLocales(MainWindow.Instance);
        }
        private static void LoadLocales<T>(T element)
            where T : FrameworkElement, ILocalizable
        {
            var localesProperty = element.GetType().GetProperty("Locales",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (localesProperty == null)
                return;

            Dictionary<string, string> locales = new Dictionary<string, string>();

            foreach (var locale in EnumerateLocales(
                Environment.ExecAppDirectoryName, "Localization"))
            {
                locales[locale.Key] = locale.Value;
            }

            foreach (var locale in EnumerateLocales(
                Environment.ExecProcessDirectoryName, "localizations"))
            {
                locales[locale.Key] = locale.Value;
            }

            localesProperty.SetValue(element,
                new ReadOnlyDictionary<string, string>(locales));
        }

        private static string GetAppName()
        {
            return GetAppName(MainWindow.Instance);
        }
        private static string GetAppName<T>(T element)
            where T : FrameworkElement, ILocalizable
        {
            var elType = element.GetType().ToString();
            var elNames = elType.Split('.');

            return elNames[0];
        }

        private static string GetElementName()
        {
            return GetElementName(MainWindow.Instance);
        }
        private static string GetElementName<T>(T element)
            where T : FrameworkElement, ILocalizable
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
        private static string GetLocXamlFilePath<T>(T element, string сultureName)
            where T : FrameworkElement, ILocalizable
        {
            string file = $"{GetElementName(element)}.{сultureName}.xaml";
            string directory = Environment.ExecProcessDirectoryName;
            string path = string.IsNullOrEmpty(directory) || directory == "Unknown"
                ? Path.Combine("localizations", file)
                : Path.Combine(directory, "localizations", file);

            if (File.Exists(path))
                return path;

            directory = Environment.ExecAppDirectoryName;
            path = string.IsNullOrEmpty(directory) || directory == "Unknown"
                ? Path.Combine("Localization", file)
                : Path.Combine(directory, "Localization", file);

            return path;
        }

        private static Task SetDefaultLanguageResourceDictionary(string resourceFile)
        {
            return SetDefaultLanguageResourceDictionary(MainWindow.Instance, resourceFile);
        }
        private static async Task SetDefaultLanguageResourceDictionary<T>(T element, string resourceFilePath)
            where T : FrameworkElement, ILocalizable
        {
            if (!File.Exists(resourceFilePath))
            {
                var fileTitle = LocalizationUtils.TryGetLocalized("FileTitle")
                                ?? "File";
                var notFoundTitle = LocalizationUtils.TryGetLocalized("NotFoundTitle1")
                                    ?? "Not found";

                await DialogManager.ShowErrorDialog($"{fileTitle} '{resourceFilePath}' {notFoundTitle.ToLower()}")
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

            for (var i = 0; i < element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary dictionary = element.Resources.MergedDictionaries[i];

                if (!dictionary.Contains("ResourceDictionaryName")
                    || dictionary["ResourceDictionaryName"].ToString()?.StartsWith("loc-") != true)
                {
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

        private static Task SetLanguageResourceDictionary(string resourceFile)
        {
            return SetLanguageResourceDictionary(MainWindow.Instance, resourceFile);
        }
        private static async Task SetLanguageResourceDictionary<T>(T element, string resourceFilePath)
            where T: FrameworkElement, ILocalizable
        {
            if (!File.Exists(resourceFilePath))
            {
                var fileTitle = LocalizationUtils.TryGetLocalized("FileTitle")
                                      ?? "File";
                var notFoundTitle = LocalizationUtils.TryGetLocalized("NotFoundTitle1")
                                       ?? "Not found";

                await DialogManager.ShowErrorDialog($"{fileTitle} '{resourceFilePath}' {notFoundTitle.ToLower()}")
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
                break;
            }

            if (dictionaryIndex == -1)
            {
                element.Resources.MergedDictionaries.Add(languageDictionary);
                return;
            }

            element.Resources.MergedDictionaries[dictionaryIndex] = languageDictionary;
        }

        public static void ReloadLocales()
        {
            ReloadLocales(MainWindow.Instance);
        }
        public static void ReloadLocales<T>(T element)
            where T : FrameworkElement, ILocalizable
        {
            LoadLocales(element);
        }

        public static string GetCurrentCultureName()
        {
            return CultureInfo.CurrentCulture.Name;
        }

        public static Task SetDefaultLanguage()
        {
            return SetDefaultLanguage(MainWindow.Instance);
        }
        public static async Task SetDefaultLanguage<T>(T element)
            where T : FrameworkElement, ILocalizable
        {
            if (element.Locales.Count == 0)
            {
                var message = LocalizationUtils.TryGetLocalized("NoLocalizationsFoundMessage")
                              ?? "No localizations found";

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);
                return;
            }

            string locale;

            if (element.Locales.ContainsKey("en-US"))
                locale = "en-US";
            else if (element.Locales.ContainsKey(SettingsManager.AppSettings.Language))
                locale = SettingsManager.AppSettings.Language;
            else
                locale = element.Locales.Keys.FirstOrDefault();

            if (string.IsNullOrEmpty(locale))
            {
                var message = LocalizationUtils.TryGetLocalized("CouldNotGetLocalizationMessage")
                              ?? "Couldn't get localization";

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);
                return;
            }

            await SwitchDefaultLanguage(element, locale)
                .ConfigureAwait(true);
        }

        public static Task SwitchDefaultLanguage(string сultureName)
        {
            return SwitchDefaultLanguage(MainWindow.Instance, сultureName);
        }
        public static async Task SwitchDefaultLanguage<T>(T element, string сultureName)
            where T : FrameworkElement, ILocalizable
        {
            string locXamlFilePath = GetLocXamlFilePath(element, сultureName);

            if (!File.Exists(locXamlFilePath))
                return;

            var primaryLanguage = SettingsManager.AppSettings.Language;

            await SetDefaultLanguageResourceDictionary(element, locXamlFilePath)
                .ConfigureAwait(true);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(сultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(сultureName);

            SettingsManager.AppSettings.Language = сultureName;

            SettingsManager.AppSettings.Save();

            LanguageChanged?.Invoke(null, EventArgs.Empty);

            await SwitchLanguage(element, primaryLanguage)
                .ConfigureAwait(true);
        }

        public static Task SwitchLanguage(string сultureName)
        {
            return SwitchLanguage(MainWindow.Instance, сultureName);
        }
        public static async Task SwitchLanguage<T>(T element, string сultureName)
            where T : FrameworkElement, ILocalizable
        {
            string locXamlFilePath = GetLocXamlFilePath(element, сultureName);

            if (!File.Exists(locXamlFilePath))
                return;

            await SetLanguageResourceDictionary(element, locXamlFilePath)
                .ConfigureAwait(true);

            Thread.CurrentThread.CurrentCulture = new CultureInfo(сultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(сultureName);

            SettingsManager.AppSettings.Language = сultureName;

            SettingsManager.AppSettings.Save();

            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
