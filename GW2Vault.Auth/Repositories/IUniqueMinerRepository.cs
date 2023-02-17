using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IUniqueMinerRepository : IGenericRepository<UniqueMiner>
    {
        UniqueMiner GetBySignature(string signature);
    }
}
