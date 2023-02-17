using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class AccountMachineKeyPairRepository : GenericRepository<AccountMachineKeyPair>, IAccountMachineKeyPairRepository
    {
        public AccountMachineKeyPairRepository(AuthContext context)
            : base(context)
        {
        }

        public AccountMachineKeyPair GetByKeyPairId(int keyPairId)
            => Context.AccountMachineKeyPair.FirstOrDefault(x => x.KeyPairId == keyPairId);

    }
}
