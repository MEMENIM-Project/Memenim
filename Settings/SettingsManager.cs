using System;

namespace Memenim.Settings
{
    public static class SettingsManager
    {
        public static AppSettings AppSettings { get; }
        public static PersistentSettings PersistentSettings { get; }

        static SettingsManager()
        {
            AppSettings = new AppSettings();
            PersistentSettings = new PersistentSettings();
        }
    }
}
