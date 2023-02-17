using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Services
{
    public interface IEncryptionService
    {
        ServiceResponse<byte[]> RSAEncrypt(string controllerName, string actionName, byte[] rawData);
        ServiceResponse<byte[]> RSADecrypt(string controllerName, string actionName, byte[] encryptedData);
        ServiceResponse<byte[]> RSADecrypt(int accountId, int machineId, byte[] encryptedData);
        ServiceResponse<byte[]> RSASign(int accountId, int machineId, byte[] rawData);

        ServiceResponse<byte[]> ComplexEncrypt(string controllerName, string actionName, byte[] rawData);
        ServiceResponse<byte[]> ComplexDecrypt(string controllerName, string actionName, byte[] encryptedData);
    }
}
