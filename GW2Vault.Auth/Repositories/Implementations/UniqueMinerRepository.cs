using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class UniqueMinerRepository : GenericRepository<UniqueMiner>, IUniqueMinerRepository
    {
        public UniqueMinerRepository(AuthContext context)
            : base(context)
        {
        }

        public UniqueMiner GetBySignature(string signature)
            => Context.UniqueMiner.FirstOrDefault(x => x.Signature == signature);
    }
}
