using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        public AccountRepository(AuthContext context)
            : base(context)
        {
        }

        public Account GetByAccountName(string accountName)
            => Context.Account.FirstOrDefault(x => x.AccountName == accountName);
    }
}
