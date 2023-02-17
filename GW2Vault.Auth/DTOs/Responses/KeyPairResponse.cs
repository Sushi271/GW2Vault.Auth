using GW2Vault.Auth.DTOs.Entities;

namespace GW2Vault.Auth.DTOs.Responses
{
    class KeyPairResponse : Entity
    {
        public KeyPairPurpose Purpose => EntityDetails.Purpose;
        public AbstractKeyPairEntity EntityDetails { get; set; }
    }
}
