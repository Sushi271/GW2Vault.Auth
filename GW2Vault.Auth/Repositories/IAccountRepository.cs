using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Account GetByAccountName(string accountName);
    }
}
