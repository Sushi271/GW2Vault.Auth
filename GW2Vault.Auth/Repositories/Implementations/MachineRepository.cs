using System.Linq;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class MachineRepository : GenericRepository<Machine>, IMachineRepository
    {
        public MachineRepository(AuthContext context)
            : base(context)
        {
        }

        public Machine GetByPcIdentifier(string pcIdentifier)
            => Context.Machine.FirstOrDefault(x => x.PcIdentifier == pcIdentifier);
    }
}
