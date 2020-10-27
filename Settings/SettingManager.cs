using System;

namespace Memenim.Settings
{
    public static class SettingManager
    {
        public static AppSettings AppSettings { get; }
        public static PersistentSettings PersistentSettings { get; }

        static SettingManager()
        {
            AppSettings = new AppSettings();
            PersistentSettings = new PersistentSettings();
        }
    }
}
