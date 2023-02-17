using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Repositories
{
    public interface IActivationCodeRepository : IGenericRepository<ActivationCode>
    {
        ActivationCode GetByValue(string codeValue);
    }
}
