using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Settings.Entities;
using Memenim.Utils;
using RIS.Extensions;
using RIS.Settings.Ini;
using Environment = RIS.Environment;

namespace Memenim.Settings
{
    public class PersistentSettings
    {
        private const string SettingsFileName = "PersistentSettings.store";

        public event EventHandler<UserChangedEventArgs> CurrentUserChanged;

        public object SyncRoot { get; }
        public string SettingsFilePath { get; }
        public IniFile SettingsFile { get; }

        public ReadOnlyDictionary<string, User> AvailableUsers { get; private set; }
        public User CurrentUser { get; private set; }

        public PersistentSettings()
        {
            SyncRoot = new object();

            SettingsFilePath = Path.Combine(Environment.ExecProcessDirectoryName, SettingsFileName);
            SettingsFile = new IniFile();

            CurrentUser = new User(
                null,
                null,
                -1);

            Load();

            UpdateAvailableUsers();

            if (!SetCurrentUser(GetCurrentUserLogin()))
            {
                RemoveUser(GetCurrentUserLogin());

                MainWindow.Instance.Dispatcher.Invoke(() =>
                {
                    NavigationController.Instance.RequestPage<LoginPage>();
                });
            }
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
            try
            {
                lock (SyncRoot)
                {
                    return SettingsFile.GetSection(sectionName);
                }
            }
            catch (Exception)
            {
                return null;
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



        public void UpdateAvailableUsers()
        {
            var defaultSectionName = SettingsFile.DefaultSectionName;
            var userLogins = SettingsFile.GetSections()
                .Where(userLogin =>
                    userLogin != defaultSectionName)
                .ToArray();
            var users = new Dictionary<string, User>(userLogins.Length);

            foreach (var sectionName in userLogins)
            {
                try
                {
                    users.Add(
                        sectionName,
                        new User(
                            sectionName,
                            GetUserToken(sectionName),
                            GetUserId(sectionName)));

                }
                catch (CryptographicException)
                {
                    RemoveUser(sectionName);
                }
            }

            AvailableUsers = new ReadOnlyDictionary<string, User>(users);
        }

        public bool IsExistUser(string login)
        {
            return AvailableUsers.ContainsKey(login);
        }

        public bool SetCurrentUser(string login)
        {
            if (!IsExistUser(login))
                return false;

            if (!GetUser(login, out var currentUser))
                return false;

            SetCurrentUserLogin(login);

            var oldUser = CurrentUser;
            CurrentUser = currentUser;

            CurrentUserChanged?.Invoke(this,
                new UserChangedEventArgs(oldUser, CurrentUser));

            return true;
        }

        public void ResetCurrentUser()
        {
            ResetCurrentUserLogin();

            var oldUser = CurrentUser;
            CurrentUser = new User(
                null,
                null,
                -1);

            CurrentUserChanged?.Invoke(this,
                new UserChangedEventArgs(oldUser, CurrentUser));
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

        public void ResetCurrentUserLogin()
        {
            SetDefault("CurrentUserLogin", string.Empty);
        }



        public bool GetUser(string login, out User user)
        {
            user = new User(
                null,
                null,
                -1);

            var userSection = GetSection(login);

            if (userSection == null)
                return false;

            try
            {
                user = new User(
                    login,
                    GetUserToken(login),
                    GetUserId(login));

            }
            catch (CryptographicException)
            {
                return false;
            }

            return true;
        }

        public string GetUserToken(string login)
        {
            return PersistentUtils.WinUnprotect(
                Get(login, "UserToken"),
                $"UserToken-{login}");
        }

        public int GetUserId(string login)
        {
            var userId = PersistentUtils.WinUnprotect(
                Get(login, "UserId"),
                $"UserId-{login}");

            return !string.IsNullOrEmpty(userId)
                ? userId.ToInt()
                : -1;
        }

        public bool SetUser(string login, string token, int id)
        {
            try
            {
                SetUserToken(login, token);
                SetUserId(login, id);
            }
            catch (CryptographicException)
            {
                return false;
            }

            UpdateAvailableUsers();

            return true;
        }

        public void SetUserToken(string login, string token)
        {
            Set(login, "UserToken",
                PersistentUtils.WinProtect(token, $"UserToken-{login}"));
        }

        public void SetUserId(string login, int id)
        {
            Set(login, "UserId",
                PersistentUtils.WinProtect(id.ToString(), $"UserId-{login}"));
        }

        public void RemoveUser(string login)
        {
            RemoveSection(login);

            if (GetCurrentUserLogin() == login)
                ResetCurrentUser();

            UpdateAvailableUsers();
        }
    }
}
