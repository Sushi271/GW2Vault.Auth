using GW2Vault.Auth.Infrastructure;

namespace GW2Vault.Auth.Services
{
    public interface IAuthenticationService
    {
        ServiceResponse Authenticate(string username, string passwordHash);
    }
}
