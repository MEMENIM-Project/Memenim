using System;

namespace Memenim.Settings.Entities
{
    public class User
    {
        public string Login { get; }
        public string Token { get; }
        public int Id { get; }
        public string RocketPassword { get; private set; }
        public UserStoreType StoreType { get; }



        public User(string login, string token,
            int id, string rocketPassword,
            UserStoreType storeType)
        {
            Login = login;
            Token = token;
            Id = id;
            RocketPassword = rocketPassword;
            StoreType = storeType;
        }



        public bool IsTemporary()
        {
            return StoreType == UserStoreType.Temporary;
        }


        public bool HasRocketPassword()
        {
            return !string.IsNullOrEmpty(RocketPassword);
        }

        public void SetRocketPassword(
            string rocketPassword)
        {
            RocketPassword = rocketPassword;

            SettingsManager.PersistentSettings.SetUserRocketPassword(
                Login, rocketPassword);
        }
    }
}
