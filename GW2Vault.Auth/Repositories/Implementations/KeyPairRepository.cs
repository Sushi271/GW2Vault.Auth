using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class KeyPairRepository : GenericRepository<KeyPair>, IKeyPairRepository
    {
        public KeyPairRepository(AuthContext context)
            : base(context)
        {
        }

        public KeyPair GetControllerActionKeyPair(string controllerName, string actionName, KeyPairType keyPairType)
        {
            var controllerActionKeyPair = Context.ControllerActionKeyPair.FirstOrDefault(x =>
                x.ControllerName == controllerName &&
                x.ActionName == actionName &&
                x.KeyPairTypeId == keyPairType);
            if (controllerActionKeyPair == null)
                return null;
            return GetById(controllerActionKeyPair.KeyPairId);
        }

        public KeyPair GetAccountMachineKeyPair(int accountId, int machineId, KeyPairType keyPairType)
        {
            var accountMachineKeyPair = Context.AccountMachineKeyPair.FirstOrDefault(x =>
                x.AccountId == accountId &&
                x.MachineId == machineId &&
                x.KeyPairTypeId == keyPairType);
            if (accountMachineKeyPair == null)
                return null;
            return GetById(accountMachineKeyPair.KeyPairId);
        }
    }
}
