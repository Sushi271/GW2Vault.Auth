using System.Collections.Generic;
using GW2Vault.Auth.DTOs.Entities;
using GW2Vault.Auth.DTOs.Requests;
using GW2Vault.Auth.Infrastructure;

namespace GW2Vault.Auth.Services
{
    public interface IKeyPairService
    {
        ServiceResponse<List<SimplifiedKeyPairEntity>> GetKeyPairList();
        ServiceResponse<AbstractKeyPairEntity> GetKeyPair(int keyPairId);
        ServiceResponse<AbstractKeyPairEntity> SaveKeyPair(SaveKeyPairRequest keyPairRequest);
        ServiceResponse RemoveKeyPair(int keyPairId);
    }
}
