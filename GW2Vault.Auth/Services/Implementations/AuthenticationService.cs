using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Repositories;
using System;
using System.Security.Cryptography;

namespace GW2Vault.Auth.Services.Implementations
{
    [EnableDependencyInjection]
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        ICredentialsRepository CredentialsRepository { get; }

        public AuthenticationService(AuthContext context,
            ICredentialsRepository credentialsRepository)
            : base(context)
            => CredentialsRepository = credentialsRepository;

        public ServiceResponse Authenticate(string username, string passwordHash)
        {
            var credentials = CredentialsRepository.GetByUsername(username);
            if (credentials == null)
                return ServiceResponse.Error(401, "User authentication failed.");

            var salt = new byte[16];
            var hashBytes = Convert.FromBase64String(passwordHash);
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var password = credentials.Password;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                var hash = pbkdf2.GetBytes(20);

                for (int i = 0; i < 20; i++)
                    if (hashBytes[i + 16] != hash[i])
                        return ServiceResponse.Error(401, "User authentication failed.");
            }

            return ServiceResponse.Success();
        }
    }
}
