using System.Security.Cryptography;

namespace LicenseGeneratorAPK.Helpers
{
    public static class RsaKeyHelper
    {
        public static (string publicKey, string privateKey) GenerateKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);
                return (publicKey, privateKey);
            }
        }

        public static byte[] SignData(byte[] data, string privateKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(privateKeyXml);
                return rsa.SignData(data, CryptoConfig.MapNameToOID("SHA256"));
            }
        }

        public static bool VerifySignature(byte[] data, byte[] signature, string publicKeyXml)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.FromXmlString(publicKeyXml);
                return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature);
            }
        }
    }
}