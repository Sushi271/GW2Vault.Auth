using System.Collections.Generic;
using GW2Vault.Auth.Data;
using GW2Vault.Auth.Helpers;
using GW2Vault.Auth.Infrastructure;
using GW2Vault.Auth.Model;
using GW2Vault.Auth.Repositories;

namespace GW2Vault.Auth.Services.Implementations
{
    [EnableDependencyInjection]
    public class MachineService : BaseService, IMachineService
    {
        IMachineRepository MachineRepository { get; }

        public MachineService(AuthContext context, IMachineRepository machineRepository)
            : base(context)
            => MachineRepository = machineRepository;

        public ServiceResponse<List<Machine>> SearchMachinesByAccount(int accountId)
        {
            var machines = MachineRepository.GetList(m => m.AccountId == accountId);
            return ServiceResponse.Success(machines);
        }
    }
}
