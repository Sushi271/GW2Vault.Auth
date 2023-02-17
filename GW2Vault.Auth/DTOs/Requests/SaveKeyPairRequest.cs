using GW2Vault.Auth.DTOs.Entities;
using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.DTOs.Requests
{
    public class SaveKeyPairRequest : EntityRequest
    {
        public string PrivateKey { get; set; }

        public KeyPairPurpose Purpose { get; set; }
        public KeyPairType? Type { get; set; }

        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        public int? AccountId { get; set; }
        public int? MachineId { get; set; }
    }
}
