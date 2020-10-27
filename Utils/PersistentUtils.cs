using System;
using System.Security.Cryptography;
using System.Text;
using RIS.Text.Encoding.Base;

namespace Memenim.Utils
{
    public static class PersistentUtils
    {
        public static string WinProtect(string data, string additionalKey = "MEMENIM")
        {
            return Base64.RemovePadding(Convert.ToBase64String(ProtectedData.Protect(
                Encoding.UTF8.GetBytes(data),
                Encoding.UTF8.GetBytes(additionalKey),
                DataProtectionScope.CurrentUser)));
        }

        public static string WinUnprotect(string data, string additionalKey = "MEMENIM")
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(
                Convert.FromBase64String(Base64.RestorePadding(data)),
                Encoding.UTF8.GetBytes(additionalKey),
                DataProtectionScope.CurrentUser));
        }
    }
}
