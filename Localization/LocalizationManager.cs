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

        private static LocalizationXamlModule _currentLanguage;
        public static LocalizationXamlModule CurrentLanguage
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
            var baseAppDirectory = Environment.ExecAppDirectoryName;
            var baseProcessDirectory = Environment.ExecProcessDirectoryName;

            if (string.IsNullOrEmpty(baseAppDirectory) || baseAppDirectory == "Unknown")
                return;
            if (string.IsNullOrEmpty(baseProcessDirectory) || baseProcessDirectory == "Unknown")
                return;

            var directory = Path.Combine(baseAppDirectory,
                "Localization", "localizations");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            directory = Path.Combine(baseProcessDirectory,
                "localizations");

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static void OnLanguageChanged(LocalizationXamlModule newLanguage)
        {
            OnLanguageChanged(null, newLanguage);
        }
        public static void OnLanguageChanged(object sender, LocalizationXamlModule newLanguage)
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

        private static Dictionary<string, List<string>> GetLocalesPaths(
            string directoryBasePath, string directoryRelativePath = null)
        {
            return GetLocalesPaths(MainWindow.Instance,
                directoryBasePath, directoryRelativePath);
        }
        private static Dictionary<string, List<string>> GetLocalesPaths<T>(
            T element, string directoryBasePath, string directoryRelativePath = null)
            where T : FrameworkElement, ILocalizable
        {
            var localesPaths = new Dictionary<string, List<string>>();
            var directory = directoryBasePath;

            if (string.IsNullOrEmpty(directory)
                || directory == "Unknown"
                || !Directory.Exists(directory))
            {
                return localesPaths;
            }

            if (!string.IsNullOrEmpty(directoryRelativePath))
            {
                directory = Path.Combine(directory, directoryRelativePath);

                if (!Directory.Exists(directory))
                    return localesPaths;
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

                    var separatorIndex = fileName.IndexOf('.');
                    var cultureName = fileName[(separatorIndex + 1)..];

                    if (!localesPaths.TryGetValue(cultureName, out _))
                    {
                        localesPaths.Add(
                            cultureName,
                            new List<string>());
                    }

                    localesPaths[cultureName].Add(
                        filePath);
                }
                catch (Exception ex)
                {
                    Events.OnError(new RErrorEventArgs(ex, ex.Message));
                }
            }

            return localesPaths;
        }

        private static Dictionary<string, LocalizationXamlModule> GetLocales()
        {
            return GetLocales(MainWindow.Instance);
        }
        private static Dictionary<string, LocalizationXamlModule> GetLocales<T>(
            T element)
            where T : FrameworkElement, ILocalizable
        {
            var localesPaths = new Dictionary<string, List<string>>();

            foreach (var localePaths in GetLocalesPaths(
                Environment.ExecAppDirectoryName,
                Path.Combine("Localization", "localizations")))
            {
                if (!localesPaths.TryGetValue(localePaths.Key, out _))
                {
                    localesPaths.Add(
                        localePaths.Key,
                        new List<string>());
                }

                localesPaths[localePaths.Key].AddRange(
                    localePaths.Value);
            }

            foreach (var localePaths in GetLocalesPaths(
                Environment.ExecProcessDirectoryName,
                "localizations"))
            {
                if (!localesPaths.TryGetValue(localePaths.Key, out _))
                {
                    localesPaths.Add(
                        localePaths.Key,
                        new List<string>());
                }

                localesPaths[localePaths.Key].AddRange(
                    localePaths.Value);
            }

            var locales = new Dictionary<string, LocalizationXamlModule>();
            var elementName = GetElementName(element);

            foreach (var localePaths in localesPaths)
            {
                try
                {
                    var locale = new LocalizationXamlModule(
                        localePaths.Value, elementName);

                    locales[locale.CultureName] = locale;
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

            var locales = GetLocales(element);

            if (locales == null)
                return;

            localesProperty.SetValue(element,
                new ReadOnlyDictionary<string, LocalizationXamlModule>(locales));
        }

        private static Task<LocalizationXamlModule> GetLocalizationModule(string cultureName)
        {
            return GetLocalizationModule(MainWindow.Instance, cultureName);
        }
        private static async Task<LocalizationXamlModule> GetLocalizationModule<T>(T element, string cultureName)
            where T : FrameworkElement, ILocalizable
        {
            if (element.Locales.TryGetValue(cultureName, out var localizationModule))
            {
                foreach (var localizationFile in localizationModule.Files)
                {
                    if (File.Exists(localizationFile.Path))
                        continue;

                    var fileTitle = LocalizationUtils.TryGetLocalized("FileTitle")
                                    ?? "File";
                    var notFoundTitle = LocalizationUtils.TryGetLocalized("NotFoundTitle1")
                                        ?? "Not found";

                    await DialogManager.ShowErrorDialog(
                            $"{fileTitle} '{localizationFile.Path}' {notFoundTitle.ToLower()}")
                        .ConfigureAwait(true);
                    return null;
                }

                return localizationModule;
            }

            var message = LocalizationUtils.TryGetLocalized("CouldNotGetLocalizationMessage")
                          ?? "Couldn't get localization";

            await DialogManager.ShowErrorDialog(message)
                .ConfigureAwait(true);
            return null;
        }

        private static Task<bool> SetDefaultLocalization(LocalizationXamlModule localizationModule)
        {
            return SetDefaultLocalization(MainWindow.Instance, localizationModule);
        }
        private static async Task<bool> SetDefaultLocalization<T>(T element, LocalizationXamlModule localizationModule)
            where T : FrameworkElement, ILocalizable
        {
            foreach (var localizationFile in localizationModule.Files)
            {
                if (File.Exists(localizationFile.Path))
                    continue;

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
                    localizationModule.Dictionary);
                return true;
            }

            element.Resources.MergedDictionaries[dictionaryIndex] =
                localizationModule.Dictionary;
            return true;
        }

        private static Task<bool> SetLocalization(LocalizationXamlModule localizationModule)
        {
            return SetLocalization(MainWindow.Instance, localizationModule);
        }
        private static async Task<bool> SetLocalization<T>(T element, LocalizationXamlModule localizationModule)
            where T: FrameworkElement, ILocalizable
        {
            foreach (var localizationFile in localizationModule.Files)
            {
                if (File.Exists(localizationFile.Path))
                    continue;

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
                    localizationModule.Dictionary);
                return true;
            }

            element.Resources.MergedDictionaries[dictionaryIndex] =
                localizationModule.Dictionary;
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
            var localizationModule = await GetLocalizationModule(
                    element, cultureName)
                .ConfigureAwait(true);

            if (localizationModule == null)
                return false;

            var currentLanguage = SettingsManager.AppSettings.Language;
            var currentLocalizationModule = await GetLocalizationModule(
                    element, currentLanguage)
                .ConfigureAwait(true);

            if (currentLocalizationModule == null)
                return false;

            var setDefaultLocalizationSuccess = await SetDefaultLocalization(
                    element, localizationModule)
                .ConfigureAwait(true);

            if (!setDefaultLocalizationSuccess)
                return false;

            OnLanguageChanged(null, localizationModule);

            var setCurrentLocalizationSuccess = await SetLocalization(
                    element, currentLocalizationModule)
                .ConfigureAwait(true);

            if (!setCurrentLocalizationSuccess)
                return false;

            Thread.CurrentThread.CurrentCulture = currentLocalizationModule.Culture;
            Thread.CurrentThread.CurrentUICulture = currentLocalizationModule.Culture;

            SettingsManager.AppSettings.Language = currentLocalizationModule.CultureName;
            SettingsManager.AppSettings.Save();

            OnLanguageChanged(null, currentLocalizationModule);

            return true;
        }

        public static Task<bool> SwitchLanguage(string cultureName)
        {
            return SwitchLanguage(MainWindow.Instance, cultureName);
        }
        public static async Task<bool> SwitchLanguage<T>(T element, string cultureName)
            where T : FrameworkElement, ILocalizable
        {
            var localizationModule = await GetLocalizationModule(
                    element, cultureName)
                .ConfigureAwait(true);

            if (localizationModule == null)
                return false;

            var setLocalizationSuccess = await SetLocalization(
                    element, localizationModule)
                .ConfigureAwait(true);

            if (!setLocalizationSuccess)
                return false;

            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);

            SettingsManager.AppSettings.Language = cultureName;
            SettingsManager.AppSettings.Save();

            OnLanguageChanged(null, localizationModule);

            return true;
        }
    }
}
