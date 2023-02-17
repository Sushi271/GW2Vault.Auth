using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IKeyPairRepository : IGenericRepository<KeyPair>
    {
        KeyPair GetControllerActionKeyPair(string controllerName, string actionName, KeyPairType keyPairType);
        KeyPair GetAccountMachineKeyPair(int accountId, int machineIde, KeyPairType keyPairType);
    }
}
