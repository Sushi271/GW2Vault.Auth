using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Model;
using System.Linq;

namespace GW2Vault.Auth.Repositories.Implementations
{
    [EnableDependencyInjection]
    public class ActivationCodeRepository : GenericRepository<ActivationCode>, IActivationCodeRepository
    {
        public ActivationCodeRepository(AuthContext context)
            : base(context)
        {
        }

        public ActivationCode GetByValue(string codeValue)
        {
            return Context.ActivationCode
                .FirstOrDefault(x => x.Value == codeValue);
        }
    }
}
