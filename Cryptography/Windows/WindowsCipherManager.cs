using System;
using System.Security.Cryptography;
using System.Text;
using RIS.Text.Encoding.Base;

namespace Memenim.Cryptography.Windows
{
    public static class WindowsCipherManager
    {
        public static string Encrypt(string data,
            string additionalKey = "MEMENIM")
        {
            if (string.IsNullOrWhiteSpace(data))
                return data;

            return Base64.RemovePadding(
                Convert.ToBase64String(
                    ProtectedData.Protect(
                        Encoding.UTF8.GetBytes(data),
                        Encoding.UTF8.GetBytes(
                            additionalKey),
                        DataProtectionScope.CurrentUser)));
        }

        public static string Decrypt(string data,
            string additionalKey = "MEMENIM")
        {
            if (string.IsNullOrWhiteSpace(data))
                return data;

            return Encoding.UTF8.GetString(
                ProtectedData.Unprotect(
                    Convert.FromBase64String(
                        Base64.RestorePadding(data)),
                    Encoding.UTF8.GetBytes(
                        additionalKey),
                    DataProtectionScope.CurrentUser));
        }
    }
}
