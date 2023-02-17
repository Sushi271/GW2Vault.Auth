using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class ControllerActionKeyPairRepository : GenericRepository<ControllerActionKeyPair>, IControllerActionKeyPairRepository
    {
        public ControllerActionKeyPairRepository(AuthContext context)
            : base(context)
        {
        }

        public ControllerActionKeyPair GetByKeyPairId(int keyPairId)
            => Context.ControllerActionKeyPair.FirstOrDefault(x => x.KeyPairId == keyPairId);

    }
}
