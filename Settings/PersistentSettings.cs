using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RIS.Settings.Ini;

namespace Memenim.Settings
{
    public class PersistentSettings
    {
        private const string SettingsFileName = "PersistentSettings.store";

        public object SyncRoot { get; }
        public string SettingsFilePath { get; }
        public IniFile SettingsFile { get; }

        public string CurrentUserLogin { get; set; }
        public string CurrentUserToken { get; set; }
        public int CurrentUserId { get; set; }

        public PersistentSettings()
        {
            SyncRoot = new object();

            SettingsFilePath = SettingsFileName;
            SettingsFile = new IniFile();

            CurrentUserLogin = string.Empty;
            CurrentUserToken = string.Empty;
            CurrentUserId = -1;

            Load();
        }

        public void Load()
        {
            lock (SyncRoot)
            {
                SettingsFile.Load(SettingsFilePath);
            }
        }

        public void Save()
        {
            Task.Factory.StartNew(() =>
            {
                lock (SyncRoot)
                {
                    SettingsFile.Save();
                }
            });
        }

        public IniSection GetSection(string sectionName)
        {
            lock (SyncRoot)
            {
                return SettingsFile.GetSection(sectionName);
            }
        }

        public void RemoveSection(string sectionName)
        {
            lock (SyncRoot)
            {
                SettingsFile.RemoveSection(sectionName);

                Save();
            }
        }

        public string GetDefault(string settingName, string defaultValue = null)
        {
            return Get(SettingsFile.DefaultSectionName, settingName, defaultValue);
        }

        public string Get(string sectionName, string settingName, string defaultValue = null)
        {
            lock (SyncRoot)
            {
                return SettingsFile.GetString(sectionName, settingName, defaultValue);
            }
        }

        public void SetDefault(string settingName, string value)
        {
            Set(SettingsFile.DefaultSectionName, settingName, value);
        }

        public void Set(string sectionName, string settingName, string value)
        {
            lock (SyncRoot)
            {
                SettingsFile.Set(sectionName, settingName, value);

                Save();
            }
        }

        public void RemoveDefault(string settingName)
        {
            Remove(SettingsFile.DefaultSectionName, settingName);
        }

        public void Remove(string sectionName, string settingName)
        {
            lock (SyncRoot)
            {
                SettingsFile.Remove(sectionName, settingName);

                Save();
            }
        }



        public IEnumerable<string> GetAvailableUserLogins()
        {
            foreach (var sectionName in SettingsFile.GetSections())
            {
                if (sectionName == SettingsFile.DefaultSectionName)
                    continue;

                yield return sectionName;
            }
        }



        public string GetTenorAPIKey()
        {
            return GetDefault("TenorAPIKey", "TKAGGYAX27OJ");
        }

        public string GetCurrentUserLogin()
        {
            return GetDefault("CurrentUserLogin");
        }

        public void SetTenorAPIKey(string apiKey)
        {
            SetDefault("TenorAPIKey", apiKey);
        }

        public void SetCurrentUserLogin(string login)
        {
            SetDefault("CurrentUserLogin", login);
        }



        public IniSection GetUser(string login)
        {
            return GetSection(login);
        }

        public string GetUserToken(string login)
        {
            return Get(login, "UserToken");
        }

        public string GetUserId(string login)
        {
            return Get(login, "UserId");
        }

        public void SetUser(string login, string token, string id)
        {
            SetUserToken(login, token);
            SetUserId(login, id);
        }

        public void SetUserToken(string login, string token)
        {
            Set(login, "UserToken", token);
        }

        public void SetUserId(string login, string id)
        {
            Set(login, "UserId", id);
        }

        public void RemoveUser(string login)
        {
            RemoveSection(login);

            if (GetCurrentUserLogin() == login)
                SetCurrentUserLogin(string.Empty);
        }
    }
}
