using System;
using System.Collections.ObjectModel;
using System.Globalization;
using RIS.Localization;
using RIS.Synchronization;

namespace Memenim.Utils
{
    public static class LocalizationUtils
    {
        public static event EventHandler<LocalizationChangedEventArgs> DefaultLocalizationChanged;
        public static event EventHandler<LocalizationChangedEventArgs> LocalizationChanged;
        public static event EventHandler<LocalizationLoadedEventArgs> LocalizationsLoaded;
        public static event EventHandler<LocalizationFileNotFoundEventArgs> LocalizationFileNotFound;
        public static event EventHandler<LocalizedCultureNotFoundEventArgs> LocalizedCultureNotFound;
        public static event EventHandler<LocalizationEventArgs> LocalizationsNotFound;

        public static event EventHandler<LocalizationEventArgs> LocalizationUpdated;



        public static AsyncLock SyncRoot
        {
            get
            {
                return App.LocalizationFactory
                    .SyncRoot;
            }
        }
        public static ILocalizationModule CurrentDefaultLocalization
        {
            get
            {
                return App.LocalizationFactory
                    .CurrentDefaultLocalization;
            }
        }
        public static ILocalizationModule CurrentLocalization
        {
            get
            {
                return App.LocalizationFactory
                    .CurrentLocalization;
            }
        }
        public static ReadOnlyDictionary<string, ILocalizationModule> Localizations
        {
            get
            {
                return App.LocalizationFactory
                    .Localizations;
            }
        }

        public static CultureInfo DefaultCulture
        {
            get
            {
                return App.LocalizationFactory
                    .DefaultCulture;
            }
        }



        static LocalizationUtils()
        {
            App.LocalizationFactory
                .DefaultLocalizationChanged += OnDefaultLocalizationChanged;
            App.LocalizationFactory
                .LocalizationChanged += OnLocalizationChanged;
            App.LocalizationFactory
                .LocalizationsLoaded += OnLocalizationsLoaded;
            App.LocalizationFactory
                .LocalizationFileNotFound += OnLocalizationFileNotFound;
            App.LocalizationFactory
                .LocalizedCultureNotFound += OnLocalizedCultureNotFound;
            App.LocalizationFactory
                .LocalizationsNotFound += OnLocalizationsNotFound;

            App.LocalizationFactory
                .LocalizationUpdated += OnLocalizationUpdated;
        }



        private static void OnDefaultLocalizationChanged(
            object sender, LocalizationChangedEventArgs e)
        {
            DefaultLocalizationChanged?.Invoke(
                sender, e);
        }

        private static void OnLocalizationChanged(
            object sender, LocalizationChangedEventArgs e)
        {
            LocalizationChanged?.Invoke(
                sender, e);
        }

        private static void OnLocalizationsLoaded(
            object sender, LocalizationLoadedEventArgs e)
        {
            LocalizationsLoaded?.Invoke(
                sender, e);
        }

        private static void OnLocalizationFileNotFound(
            object sender, LocalizationFileNotFoundEventArgs e)
        {
            LocalizationFileNotFound?.Invoke(
                sender, e);
        }

        private static void OnLocalizedCultureNotFound(
            object sender, LocalizedCultureNotFoundEventArgs e)
        {
            LocalizedCultureNotFound?.Invoke(
                sender, e);
        }

        private static void OnLocalizationsNotFound(
            object sender, LocalizationEventArgs e)
        {
            LocalizationsNotFound?.Invoke(
                sender, e);
        }

        private static void OnLocalizationUpdated(
            object sender, LocalizationEventArgs e)
        {
            LocalizationUpdated?.Invoke(
                sender, e);
        }


        public static void OnLocalizationUpdated(
            ILocalizationModule localization)
        {
            App.LocalizationFactory
                .OnLocalizationUpdated(localization);
        }
        public static void OnLocalizationUpdated()
        {
            App.LocalizationFactory
                .OnLocalizationUpdated();
        }



        public static void SetDefaultCulture(
            string cultureName)
        {
            App.LocalizationFactory
                .SetDefaultCulture(cultureName);
        }
        public static void SetDefaultCulture(
            CultureInfo culture)
        {
            App.LocalizationFactory
                .SetDefaultCulture(culture);
        }



        public static void ReloadLocalizations<T>()
            where T : ILocalizationProvider
        {
            App.LocalizationFactory
                .ReloadLocalizations<T>();
        }

        public static string GetDefaultCultureName()
        {
            return App.LocalizationFactory
                .GetDefaultCultureName();
        }


        public static bool SwitchDefaultLocalization(
            string cultureName)
        {
            return App.LocalizationFactory
                .SwitchDefaultLocalization(cultureName);
        }

        public static bool SwitchLocalization(
            string cultureName)
        {
            return App.LocalizationFactory
                .SwitchLocalization(cultureName);
        }


        public static string GetLocalized(
            string key)
        {
            return App.LocalizationFactory
                .GetLocalized(key);
        }

        public static bool TryGetLocalized(
            string key, out string value)
        {
            return App.LocalizationFactory
                .TryGetLocalized(key, out value);
        }
    }
}
