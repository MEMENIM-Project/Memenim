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
using Memenim.Localization.Entities;
using Memenim.Settings;
using Memenim.Utils;
using RIS;
using RIS.Extensions;
using Environment = RIS.Environment;

namespace Memenim.Localization
{
    public static class LocalizationManager
    {
        public static event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        private static LocalizationXamlFile _currentLanguage;
        public static LocalizationXamlFile CurrentLanguage
        {
            get
            {
                return _currentLanguage;
            }
            private set
            {
                Interlocked.Exchange(ref _currentLanguage, value);
            }
        }

        static LocalizationManager()
        {
            var directory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(directory) || directory == "Unknown")
                return;

            directory = Path.Combine(directory, "localizations");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static void OnLanguageChanged(LocalizationXamlFile newLanguage)
        {
            OnLanguageChanged(null, newLanguage);
        }
        public static void OnLanguageChanged(object sender, LocalizationXamlFile newLanguage)
        {
            var oldLanguage = Interlocked.Exchange(ref _currentLanguage, newLanguage);

            if (oldLanguage == newLanguage)
                return;

            LanguageChanged?.Invoke(sender,
                new LanguageChangedEventArgs(oldLanguage, newLanguage));
        }

        private static void OnLanguageChanged(LanguageChangedEventArgs e)
        {
            OnLanguageChanged(null, e);
        }
        private static void OnLanguageChanged(object sender, LanguageChangedEventArgs e)
        {
            LanguageChanged?.Invoke(sender, e);
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

        private static Dictionary<string, LocalizationXamlFile> GetLocales(
            string directoryBasePath, string directoryRelativePath = null)
        {
            return GetLocales(MainWindow.Instance,
                directoryBasePath, directoryRelativePath);
        }
        private static Dictionary<string, LocalizationXamlFile> GetLocales<T>(
            T element, string directoryBasePath, string directoryRelativePath = null)
            where T : FrameworkElement, ILocalizable
        {
            var locales = new Dictionary<string, LocalizationXamlFile>();
            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return locales;
            }

            if (!string.IsNullOrEmpty(directoryRelativePath))
            {
                directory = Path.Combine(directory, directoryRelativePath);

                if (!Directory.Exists(directory))
                    return locales;
            }

            var elementName = GetElementName(element);

            foreach (var filePath in DirectoryExtensions.EnumerateAllFiles(directory))
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var fileExtension = Path.GetExtension(filePath);

                    if (!fileName.StartsWith($"{elementName}.")
                        || fileExtension != ".xaml")
                    {
                        continue;
                    }

                    var file = new LocalizationXamlFile(
                        filePath, elementName);

                    locales[file.CultureName] = file;
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
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

            var locales = new Dictionary<string, LocalizationXamlFile>();

            foreach (var locale in GetLocales(
                Environment.ExecAppDirectoryName, "Localization"))
            {
                locales[locale.Key] = locale.Value;
            }

            foreach (var locale in GetLocales(
                Environment.ExecProcessDirectoryName, "localizations"))
            {
                locales[locale.Key] = locale.Value;
            }

            localesProperty.SetValue(element,
                new ReadOnlyDictionary<string, LocalizationXamlFile>(locales));
        }

        private static Task<LocalizationXamlFile> GetLocalizationFile(string cultureName)
        {
            return GetLocalizationFile(MainWindow.Instance, cultureName);
        }
        private static async Task<LocalizationXamlFile> GetLocalizationFile<T>(T element, string cultureName)
            where T : FrameworkElement, ILocalizable
        {
            if (element.Locales.TryGetValue(cultureName, out var localizationFile))
            {
                if (File.Exists(localizationFile.Path))
                    return localizationFile;

                var fileTitle = LocalizationUtils.TryGetLocalized("FileTitle")
                                ?? "File";
                var notFoundTitle = LocalizationUtils.TryGetLocalized("NotFoundTitle1")
                                    ?? "Not found";

                await DialogManager.ShowErrorDialog(
                        $"{fileTitle} '{localizationFile.Path}' {notFoundTitle.ToLower()}")
                    .ConfigureAwait(true);
                return null;
            }

            var message = LocalizationUtils.TryGetLocalized("CouldNotGetLocalizationMessage")
                          ?? "Couldn't get localization";

            await DialogManager.ShowErrorDialog(message)
                .ConfigureAwait(true);
            return null;
        }

        private static Task<bool> SetDefaultLocalization(LocalizationXamlFile localizationFile)
        {
            return SetDefaultLocalization(MainWindow.Instance, localizationFile);
        }
        private static async Task<bool> SetDefaultLocalization<T>(T element, LocalizationXamlFile localizationFile)
            where T : FrameworkElement, ILocalizable
        {
            if (!File.Exists(localizationFile.Path))
            {
                var fileTitle = LocalizationUtils.TryGetLocalized("FileTitle")
                                ?? "File";
                var notFoundTitle = LocalizationUtils.TryGetLocalized("NotFoundTitle1")
                                    ?? "Not found";

                await DialogManager.ShowErrorDialog(
                        $"{fileTitle} '{localizationFile.Path}' {notFoundTitle.ToLower()}")
                    .ConfigureAwait(true);
                return false;
            }

            int dictionaryIndex = -1;

            for (var i = 0; i < element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary dictionary =
                    element.Resources.MergedDictionaries[i];

                if (!dictionary.Contains("ResourceDictionaryName")
                    || dictionary["ResourceDictionaryName"].ToString() != "localization-xaml")
                {
                    continue;
                }

                dictionaryIndex = i;
                break;
            }

            if (dictionaryIndex == -1)
            {
                element.Resources.MergedDictionaries.Add(
                    localizationFile.Dictionary);
                return true;
            }

            element.Resources.MergedDictionaries[dictionaryIndex] =
                localizationFile.Dictionary;
            return true;
        }

        private static Task<bool> SetLocalization(LocalizationXamlFile localizationFile)
        {
            return SetLocalization(MainWindow.Instance, localizationFile);
        }
        private static async Task<bool> SetLocalization<T>(T element, LocalizationXamlFile localizationFile)
            where T: FrameworkElement, ILocalizable
        {
            if (!File.Exists(localizationFile.Path))
            {
                var fileTitle = LocalizationUtils.TryGetLocalized("FileTitle")
                                ?? "File";
                var notFoundTitle = LocalizationUtils.TryGetLocalized("NotFoundTitle1")
                                    ?? "Not found";

                await DialogManager.ShowErrorDialog(
                        $"{fileTitle} '{localizationFile.Path}' {notFoundTitle.ToLower()}")
                    .ConfigureAwait(true);
                return false;
            }

            int dictionaryIndex = -1;
            bool defaultLanguageDictionary = true;

            for (var i = 0; i < element.Resources.MergedDictionaries.Count; ++i)
            {
                ResourceDictionary dictionary =
                    element.Resources.MergedDictionaries[i];

                if (!dictionary.Contains("ResourceDictionaryName")
                    || dictionary["ResourceDictionaryName"].ToString() != "localization-xaml")
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
                element.Resources.MergedDictionaries.Add(
                    localizationFile.Dictionary);
                return true;
            }

            element.Resources.MergedDictionaries[dictionaryIndex] =
                localizationFile.Dictionary;
            return true;
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

        public static Task<bool> SetDefaultLanguage()
        {
            return SetDefaultLanguage(MainWindow.Instance);
        }
        public static async Task<bool> SetDefaultLanguage<T>(T element)
            where T : FrameworkElement, ILocalizable
        {
            if (element.Locales.Count == 0)
            {
                var message = LocalizationUtils.TryGetLocalized("NoLocalizationsFoundMessage")
                              ?? "No localizations found";

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);
                return false;
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
                return false;
            }

            await SwitchDefaultLanguage(element, locale)
                .ConfigureAwait(true);
            return true;
        }

        public static Task<bool> SwitchDefaultLanguage(string cultureName)
        {
            return SwitchDefaultLanguage(MainWindow.Instance, cultureName);
        }
        public static async Task<bool> SwitchDefaultLanguage<T>(T element, string cultureName)
            where T : FrameworkElement, ILocalizable
        {
            var localizationFile = await GetLocalizationFile(
                    element, cultureName)
                .ConfigureAwait(true);

            if (localizationFile == null)
                return false;

            var currentLanguage = SettingsManager.AppSettings.Language;
            var currentLocalizationFile = await GetLocalizationFile(
                    element, currentLanguage)
                .ConfigureAwait(true);

            if (currentLocalizationFile == null)
                return false;

            var setDefaultLocalizationSuccess = await SetDefaultLocalization(
                    element, localizationFile)
                .ConfigureAwait(true);

            if (!setDefaultLocalizationSuccess)
                return false;

            OnLanguageChanged(null, localizationFile);

            var setCurrentLocalizationSuccess = await SetLocalization(
                    element, currentLocalizationFile)
                .ConfigureAwait(true);

            if (!setCurrentLocalizationSuccess)
                return false;

            Thread.CurrentThread.CurrentCulture = currentLocalizationFile.Culture;
            Thread.CurrentThread.CurrentUICulture = currentLocalizationFile.Culture;

            SettingsManager.AppSettings.Language = currentLocalizationFile.CultureName;
            SettingsManager.AppSettings.Save();

            OnLanguageChanged(null, currentLocalizationFile);

            return true;
        }

        public static Task<bool> SwitchLanguage(string cultureName)
        {
            return SwitchLanguage(MainWindow.Instance, cultureName);
        }
        public static async Task<bool> SwitchLanguage<T>(T element, string cultureName)
            where T : FrameworkElement, ILocalizable
        {
            var localizationFile = await GetLocalizationFile(
                    element, cultureName)
                .ConfigureAwait(true);

            if (localizationFile == null)
                return false;

            var setLocalizationSuccess = await SetLocalization(
                    element, localizationFile)
                .ConfigureAwait(true);

            if (!setLocalizationSuccess)
                return false;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);

            SettingsManager.AppSettings.Language = cultureName;
            SettingsManager.AppSettings.Save();

            OnLanguageChanged(null, localizationFile);

            return true;
        }
    }
}
