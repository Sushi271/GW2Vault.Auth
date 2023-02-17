using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class CredentialsRepository : GenericRepository<Credentials>, ICredentialsRepository
    {
        public CredentialsRepository(AuthContext context)
            : base(context)
        {
        }

        public Credentials GetByUsername(string username)
            => Context.Credentials.FirstOrDefault(x => x.Username == username);
    }
}
