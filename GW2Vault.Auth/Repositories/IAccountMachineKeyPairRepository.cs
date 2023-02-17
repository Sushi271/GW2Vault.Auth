using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IAccountMachineKeyPairRepository : IGenericRepository<AccountMachineKeyPair>
    {
        AccountMachineKeyPair GetByKeyPairId(int keyPairId);
    }
}
