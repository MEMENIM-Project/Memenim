using System;

namespace Memenim.Settings
{
    public static class SettingManager
    {
        public static AppSettings AppSettings { get; }

        static SettingManager()
        {
            AppSettings = new AppSettings();
        }
    }
}
