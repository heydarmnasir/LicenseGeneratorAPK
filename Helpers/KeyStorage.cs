using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LicenseGeneratorAPK.Helpers
{
    public static class KeyStorage
    {
        private static string GetPrivateKeyPath()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VakilawLicenseGenerator");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "private_key.dat");
        }

        public static void SavePrivateKeyEncrypted(string privateKeyXml)
        {
            var path = GetPrivateKeyPath();
            var bytes = Encoding.UTF8.GetBytes(privateKeyXml);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, encrypted);
        }

        public static string LoadPrivateKeyEncrypted()
        {
            var path = GetPrivateKeyPath();
            if (!File.Exists(path)) return null;
            var encrypted = File.ReadAllBytes(path);
            try
            {
                var bytes = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return null;
            }
        }
    }
}