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
                -1,
                UserStoreType.Unknown);

            Load();

            if (GetCurrentUserLogin() == null)
                ResetCurrentUserLogin();

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
                            GetUserId(sectionName),
                            UserStoreType.Permanent));

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
            if (string.IsNullOrEmpty(login))
                return false;

            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                return true;
            }

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

        public bool SetCurrentUserTemporary(string login, string token, int id)
        {
            ResetCurrentUserLogin();

            var oldUser = CurrentUser;
            CurrentUser = new User(
                login,
                token,
                id,
                UserStoreType.Temporary);

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
                -1,
                UserStoreType.Unknown);

            CurrentUserChanged?.Invoke(this,
                new UserChangedEventArgs(oldUser, CurrentUser));
        }



        public string GetTenorAPIKey()
        {
            return GetDefault("TenorAPIKey", "TKAGGYAX27OJ");
        }

        public string GetCurrentUserLogin()
        {
            if (CurrentUser.StoreType == UserStoreType.Temporary)
                return CurrentUser.Login;

            return GetDefault("CurrentUserLogin");
        }

        public void SetTenorAPIKey(string apiKey)
        {
            SetDefault("TenorAPIKey", apiKey);
        }

        public void SetCurrentUserLogin(string login)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                return;
            }

            SetDefault("CurrentUserLogin", login);
        }

        public void ResetCurrentUserLogin()
        {
            if (CurrentUser.StoreType == UserStoreType.Temporary)
                return;

            SetDefault("CurrentUserLogin", string.Empty);
        }



        public bool GetUser(string login, out User user)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                user = CurrentUser;

                return true;
            }

            user = new User(
                null,
                null,
                -1,
                UserStoreType.Unknown);

            var userSection = GetSection(login);

            if (userSection == null)
                return false;

            try
            {
                user = new User(
                    login,
                    GetUserToken(login),
                    GetUserId(login),
                    UserStoreType.Permanent);

            }
            catch (CryptographicException)
            {
                return false;
            }

            return true;
        }

        public string GetUserToken(string login)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                return CurrentUser.Token;
            }

            return PersistentUtils.WinUnprotect(
                Get(login, "UserToken"),
                $"UserToken-{login}");
        }

        public int GetUserId(string login)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                return CurrentUser.Id;
            }

            var userId = PersistentUtils.WinUnprotect(
                Get(login, "UserId"),
                $"UserId-{login}");

            return !string.IsNullOrEmpty(userId)
                ? userId.ToInt()
                : -1;
        }

        public bool SetUser(string login, string token, int id)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                CurrentUser = new User(
                    CurrentUser.Login,
                    token,
                    id,
                    CurrentUser.StoreType);

                return true;
            }

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
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                CurrentUser = new User(
                    CurrentUser.Login,
                    token,
                    CurrentUser.Id,
                    CurrentUser.StoreType);

                return;
            }

            Set(login, "UserToken",
                PersistentUtils.WinProtect(token, $"UserToken-{login}"));
        }

        public void SetUserId(string login, int id)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                CurrentUser = new User(
                    CurrentUser.Login,
                    CurrentUser.Token,
                    id,
                    CurrentUser.StoreType);

                return;
            }

            Set(login, "UserId",
                PersistentUtils.WinProtect(id.ToString(), $"UserId-{login}"));
        }

        public void RemoveUser(string login)
        {
            if (CurrentUser.Login == login
                && CurrentUser.StoreType == UserStoreType.Temporary)
            {
                ResetCurrentUser();

                return;
            }

            RemoveSection(login);

            if (GetCurrentUserLogin() == login)
                ResetCurrentUser();

            UpdateAvailableUsers();
        }
    }
}
