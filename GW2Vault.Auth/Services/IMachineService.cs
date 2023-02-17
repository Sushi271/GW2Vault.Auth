using System.Collections.Generic;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.Services
{
    public interface IMachineService
    {
        ServiceResponse<List<Machine>> SearchMachinesByAccount(int accountId);
    }
}
