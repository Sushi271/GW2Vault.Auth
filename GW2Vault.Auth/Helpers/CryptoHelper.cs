using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace GW2Vault.Auth
{
    static class CryptoHelper
    {
        const string AuthKeyDirectory = "AuthKeys";
        const string PrivateKeyFilenameSuffix = "priv";
        const string PublicKeyFilenameSuffix = "pub";
        const string KeyFilenameSeparator = "-";

        public static RSACryptoServiceProvider GetPublicRSAForAction(string actionName)
        {
            var filename = Path.Combine(AuthKeyDirectory,
                string.Join(KeyFilenameSeparator,
                actionName.ToLower(), PublicKeyFilenameSuffix));

            var base64 = File.ReadAllText(filename);
            var bytes = Convert.FromBase64String(base64);
            var publicKey = (RsaKeyParameters)PublicKeyFactory.CreateKey(bytes);
            var rsaParams = DotNetUtilities.ToRSAParameters(publicKey);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        public static RSACryptoServiceProvider GetPrivateRSAForAction(string actionName)
        {
            var keyPair = GetOpenSSHKeyPairForAction(actionName);
            var rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParams);
            return rsa;
        }

        static AsymmetricCipherKeyPair GetOpenSSHKeyPairForAction(string actionName)
        {
            var filename = Path.Combine(AuthKeyDirectory,
                string.Join(KeyFilenameSeparator,
                actionName.ToLower(), PrivateKeyFilenameSuffix));

            using (var sr = File.OpenText(filename))
            {
                var pr = new PemReader(sr);
                return (AsymmetricCipherKeyPair)pr.ReadObject();
            }
        }

        public static bool TryDecrypt(this RSACryptoServiceProvider privateRsa, byte[] encrypted, out byte[] decrypted)
        {
            decrypted = null;
            try
            {
                decrypted = privateRsa.Decrypt(encrypted, true);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
