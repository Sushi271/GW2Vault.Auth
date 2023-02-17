using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface ICredentialsRepository : IGenericRepository<Credentials>
    {
        Credentials GetByUsername(string username);
    }
}
