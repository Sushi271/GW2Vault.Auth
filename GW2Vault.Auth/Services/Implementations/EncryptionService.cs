using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;
using GW2Vault.Auth.Repositories;
using GW2Vault.Utility.Crypto;

namespace GW2Vault.Auth.Services.Implementations
{
    [EnableDependencyInjection]
    public class EncryptionService : BaseService, IEncryptionService
    {
        IKeyPairRepository EncryptionKeyPairRepository { get; }

        public EncryptionService(AuthContext context,
            IKeyPairRepository encryptionKeyPairRepository)
            : base(context)
            => EncryptionKeyPairRepository = encryptionKeyPairRepository;

        public ServiceResponse<byte[]> RSAEncrypt(string controllerName, string actionName, byte[] rawData) => TryExecute(() =>
        {
            var keyPair = EncryptionKeyPairRepository.GetControllerActionKeyPair(controllerName, actionName, KeyPairType.Encryption);
            if (keyPair == null)
                return ServiceResponse.Unauthorized<byte[]>();
            var rsa = new RSACrypter(publicKeyProvider: new PublicKeyProvider(keyPair.PublicKey));
            var encrypted = rsa.Encrypt(rawData);
            return ServiceResponse.Success(encrypted);
        });

        public ServiceResponse<byte[]> RSADecrypt(string controllerName, string actionName, byte[] encryptedData) => TryExecute(() =>
        {
            var keyPair = EncryptionKeyPairRepository.GetControllerActionKeyPair(controllerName, actionName, KeyPairType.Encryption);
            var rsa = new RSACrypter(privateKeyProvider: new PrivateKeyProvider(keyPair.PrivateKey));
            var decrypted = rsa.Decrypt(encryptedData);
            if (decrypted == null)
                return ServiceResponse<byte[]>.Error(500, "Failed to decrypt data.");
            return ServiceResponse.Success(decrypted);
        });

        public ServiceResponse<byte[]> RSADecrypt(int accountId, int machineId, byte[] encryptedData) => TryExecute(() =>
        {
            var keyPair = EncryptionKeyPairRepository.GetAccountMachineKeyPair(accountId, machineId, KeyPairType.Encryption);
            if (keyPair == null)
                return ServiceResponse.Unauthorized<byte[]>();
            var rsa = new RSACrypter(privateKeyProvider: new PrivateKeyProvider(keyPair.PrivateKey));
            var decrypted = rsa.Decrypt(encryptedData);
            return ServiceResponse.Success(decrypted);
        });

        public ServiceResponse<byte[]> RSASign(int accountId, int machineId, byte[] rawData) => TryExecute(() =>
        {
            var keyPair = EncryptionKeyPairRepository.GetAccountMachineKeyPair(accountId, machineId, KeyPairType.Signing);
            if (keyPair == null)
                return ServiceResponse.Unauthorized<byte[]>();
            var rsa = new RSASigner(privateKeyProvider: new PrivateKeyProvider(keyPair.PrivateKey));
            var signature = rsa.Sign(rawData);
            return ServiceResponse.Success(signature);
        });

        public ServiceResponse<byte[]> ComplexEncrypt(string controllerName, string actionName, byte[] rawData) => TryExecute(() =>
        {
            var encryptionKeyPair = EncryptionKeyPairRepository.GetControllerActionKeyPair(controllerName, actionName, KeyPairType.Encryption);
            var signingKeyPair = EncryptionKeyPairRepository.GetControllerActionKeyPair(controllerName, actionName, KeyPairType.Signing);
            var crypter = new ComplexCrypter(
                encryptionPublicKeyProvider: new PublicKeyProvider(encryptionKeyPair.PublicKey),
                signingPrivateKeyProvider: new PrivateKeyProvider(signingKeyPair.PrivateKey));

            var encrypted = crypter.SignAndEncryptLargeData(rawData).Result;
            if (encrypted == null)
                return ServiceResponse<byte[]>.Error(500, "Failed to encrypt data.");
            return ServiceResponse.Success(encrypted);
        });

        public ServiceResponse<byte[]> ComplexDecrypt(string controllerName, string actionName, byte[] encryptedData) => TryExecute(() =>
        {
            var encryptionKeyPair = EncryptionKeyPairRepository.GetControllerActionKeyPair(controllerName, actionName, KeyPairType.Encryption);
            var signingKeyPair = EncryptionKeyPairRepository.GetControllerActionKeyPair(controllerName, actionName, KeyPairType.Signing);
            var crypter = new ComplexCrypter(
                decryptionPrivateKeyProvider: new PrivateKeyProvider(encryptionKeyPair.PrivateKey),
                veryfingPublicKeyProvider: new PublicKeyProvider(signingKeyPair.PublicKey));

            var decrypted = crypter.DecryptAndVerifyLargeData(encryptedData).Result;
            if (decrypted == null)
                return ServiceResponse<byte[]>.Error(500, "Failed to decrypt data.");
            return ServiceResponse.Success(decrypted);
        });

        class PublicKeyProvider : IPublicKeyProvider
        {
            string base64PublicKey;

            public PublicKeyProvider(string base64PublicKey)
                => this.base64PublicKey = base64PublicKey;

            public string ProvideBase64PublicKey()
                => base64PublicKey;
        }

        class PrivateKeyProvider : IPrivateKeyProvider
        {
            string privateKey;

            public PrivateKeyProvider(string privateKey)
                => this.privateKey = privateKey;

            public string ProvidePrivateKey()
                => privateKey;
        }
    }
}
