using System;

namespace Memenim.Settings
{
    public static class SettingsManager
    {
        private static readonly object AppSettingsSyncRoot = new object();
        private static volatile AppSettings _appSettings;
        public static AppSettings AppSettings
        {
            get
            {
                if (_appSettings == null)
                {
                    lock (AppSettingsSyncRoot)
                    {
                        if (_appSettings == null)
                            _appSettings = new AppSettings();
                    }
                }

                return _appSettings;
            }
        }
        private static readonly object PersistentSettingsSyncRoot = new object();
        private static volatile PersistentSettings _persistentSettings;
        public static PersistentSettings PersistentSettings
        {
            get
            {
                if (_persistentSettings == null)
                {
                    lock (PersistentSettingsSyncRoot)
                    {
                        if (_persistentSettings == null)
                            _persistentSettings = new PersistentSettings();
                    }
                }

                return _persistentSettings;
            }
        }
    }
}
