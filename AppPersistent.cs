using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AnonymDesktopClient.Core
{
    public static class AppPersistent
    {
        private const string StoreFileName = "PersistentStore.store";

        public const string TenorAPIKey = "TKAGGYAX27OJ";

        private static readonly object StoreSyncRoot;
        private static readonly Dictionary<string, string> Store;

        public static string UserToken { get; set; }
        public static int LocalUserId { get; set; }

        static AppPersistent()
        {
            StoreSyncRoot = new object();
            Store = new Dictionary<string, string>(2);

            LoadStore();
        }

        private static void LoadStore()
        {
            lock (StoreSyncRoot)
            {
                if (!File.Exists(StoreFileName))
                    File.Create(StoreFileName).Close();

                using (StreamReader reader = new StreamReader(StoreFileName, new UTF8Encoding(false)))
                {
                    while (!reader.EndOfStream)
                    {
                        string storePair = reader.ReadLine();

                        if (string.IsNullOrEmpty(storePair))
                            continue;

                        storePair = storePair.Trim();

                        if (storePair.Length == 0)
                            continue;

                        string[] storePairComponents = storePair.Split('~');

                        if (storePairComponents.Length < 2)
                            continue;

                        string storePairKey = storePairComponents[0].Trim().Substring(1);
                        string storePairValue = storePairComponents[1].Trim();

                        if (!Store.ContainsKey(storePairKey))
                            Store.Add(storePairKey, storePairValue);
                    }
                }
            }
        }

        private static void SaveStore()
        {
            if (Store == null)
                return;

            using (StreamWriter writer = new StreamWriter(StoreFileName, false, new UTF8Encoding(false)))
            {
                foreach (KeyValuePair<string, string> storePair in Store)
                {
                    writer.WriteLine($":{storePair.Key}~{storePair.Value}");
                }
            }
        }

        public static void AddToStore(string key, string value)
        {
            lock (StoreSyncRoot)
            {
                if (Store == null)
                    return;

                if (!Store.ContainsKey(key))
                    Store.Add(key, value);
                else
                    Store[key] = value;

                SaveStore();
            }
        }

        public static string GetFromStore(string key)
        {
            lock (StoreSyncRoot)
            {
                if (Store == null)
                    return null;

                if (Store.TryGetValue(key, out string value))
                    return value;

                return null;
            }
        }

        public static void RemoveFromStore(string key)
        {
            lock (StoreSyncRoot)
            {
                if (Store == null)
                    return;

                if (Store.ContainsKey(key))
                    Store.Remove(key);

                SaveStore();
            }
        }

        public static string WinProtect(string data, string additionalKey = "AnonymDesktopClient")
        {
            return Convert.ToBase64String(ProtectedData.Protect(
                Encoding.UTF8.GetBytes(data),
                Encoding.UTF8.GetBytes(additionalKey),
                DataProtectionScope.CurrentUser));
        }

        public static string WinUnprotect(string data, string additionalKey = "AnonymDesktopClient")
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(
                Convert.FromBase64String(data),
                Encoding.UTF8.GetBytes(additionalKey),
                DataProtectionScope.CurrentUser));
        }
    }
}
