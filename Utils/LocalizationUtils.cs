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



        public static LocalizationFactory CurrentFactory { get; private set; }

        public static bool IsInitialized { get; private set; }



        public static AsyncLock SyncRoot
        {
            get
            {
                return CurrentFactory
                    .SyncRoot;
            }
        }
        public static ILocalizationModule CurrentDefaultLocalization
        {
            get
            {
                return CurrentFactory
                    .CurrentDefaultLocalization;
            }
        }
        public static ILocalizationModule CurrentLocalization
        {
            get
            {
                return CurrentFactory
                    .CurrentLocalization;
            }
        }
        public static ReadOnlyDictionary<string, ILocalizationModule> Localizations
        {
            get
            {
                return CurrentFactory
                    .Localizations;
            }
        }

        public static CultureInfo DefaultCulture
        {
            get
            {
                return CurrentFactory
                    .DefaultCulture;
            }
        }



        static LocalizationUtils()
        {
            Initialize();
        }



        private static void OnDefaultLocalizationChanged(object sender,
            LocalizationChangedEventArgs e)
        {
            DefaultLocalizationChanged?.Invoke(
                sender, e);
        }

        private static void OnLocalizationChanged(object sender,
            LocalizationChangedEventArgs e)
        {
            LocalizationChanged?.Invoke(
                sender, e);
        }

        private static void OnLocalizationsLoaded(object sender,
            LocalizationLoadedEventArgs e)
        {
            LocalizationsLoaded?.Invoke(
                sender, e);
        }

        private static void OnLocalizationFileNotFound(object sender,
            LocalizationFileNotFoundEventArgs e)
        {
            LocalizationFileNotFound?.Invoke(
                sender, e);
        }

        private static void OnLocalizedCultureNotFound(object sender,
            LocalizedCultureNotFoundEventArgs e)
        {
            LocalizedCultureNotFound?.Invoke(
                sender, e);
        }

        private static void OnLocalizationsNotFound(object sender,
            LocalizationEventArgs e)
        {
            LocalizationsNotFound?.Invoke(
                sender, e);
        }

        private static void OnLocalizationUpdated(object sender,
            LocalizationEventArgs e)
        {
            LocalizationUpdated?.Invoke(
                sender, e);
        }


        public static void OnLocalizationUpdated(
            ILocalizationModule localization)
        {
            CurrentFactory
                .OnLocalizationUpdated(localization);
        }
        public static void OnLocalizationUpdated()
        {
            CurrentFactory
                .OnLocalizationUpdated();
        }



        public static void Initialize()
        {
            if (IsInitialized)
                return;

            var assemblyName = typeof(App)
                .Assembly
                .GetName()
                .Name;

            CurrentFactory = LocalizationFactory
                .Create(assemblyName, "MEMENIM");

            LocalizationManager.SetCurrentFactory(
                assemblyName, CurrentFactory);

            CurrentFactory
                .DefaultLocalizationChanged += OnDefaultLocalizationChanged;
            CurrentFactory
                .LocalizationChanged += OnLocalizationChanged;
            CurrentFactory
                .LocalizationsLoaded += OnLocalizationsLoaded;
            CurrentFactory
                .LocalizationFileNotFound += OnLocalizationFileNotFound;
            CurrentFactory
                .LocalizedCultureNotFound += OnLocalizedCultureNotFound;
            CurrentFactory
                .LocalizationsNotFound += OnLocalizationsNotFound;

            CurrentFactory
                .LocalizationUpdated += OnLocalizationUpdated;

            IsInitialized = true;
        }



        public static void SetDefaultCulture(
            string cultureName)
        {
            CurrentFactory
                .SetDefaultCulture(cultureName);
        }
        public static void SetDefaultCulture(
            CultureInfo culture)
        {
            CurrentFactory
                .SetDefaultCulture(culture);
        }



        public static void ReloadLocalizations<T>()
            where T : ILocalizationProvider
        {
            CurrentFactory
                .ReloadLocalizations<T>();
        }

        public static string GetDefaultCultureName()
        {
            return CurrentFactory
                .GetDefaultCultureName();
        }


        public static bool SwitchDefaultLocalization(
            string cultureName)
        {
            return CurrentFactory
                .SwitchDefaultLocalization(cultureName);
        }

        public static bool SwitchLocalization(
            string cultureName)
        {
            return CurrentFactory
                .SwitchLocalization(cultureName);
        }


        public static string GetLocalized(
            string key)
        {
            return CurrentFactory
                .GetLocalized(key);
        }

        public static bool TryGetLocalized(
            string key, out string value)
        {
            return CurrentFactory
                .TryGetLocalized(key, out value);
        }
    }
}
