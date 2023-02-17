using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IControllerActionKeyPairRepository : IGenericRepository<ControllerActionKeyPair>
    {
        ControllerActionKeyPair GetByKeyPairId(int keyPairId);
    }
}
