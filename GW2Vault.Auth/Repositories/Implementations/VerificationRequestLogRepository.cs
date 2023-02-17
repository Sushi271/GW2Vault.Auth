using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class VerificationRequestLogRepository : GenericRepository<VerificationRequestLog>, IVerificationRequestLogRepository
    {
        public VerificationRequestLogRepository(AuthContext context)
            : base(context)
        {
        }
    }
}
