using System.Collections.Generic;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;
using GW2Vault.Auth.Repositories;

namespace GW2Vault.Auth.Services.Implementations
{
    [EnableDependencyInjection]
    public class AccountService : BaseService, IAccountService
    {
        IAccountRepository AccountRepository { get; }

        public AccountService(AuthContext context, IAccountRepository accountRepository)
            : base(context)
            => AccountRepository = accountRepository;

        public ServiceResponse<List<Account>> GetAccountList()
        {
            var accounts = AccountRepository.GetList();
            return ServiceResponse.Success(accounts);
        }
    }
}
