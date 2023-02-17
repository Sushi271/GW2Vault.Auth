using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IMachineRepository : IGenericRepository<Machine>
    {
        Machine GetByPcIdentifier(string pcIdentifier);
    }
}
