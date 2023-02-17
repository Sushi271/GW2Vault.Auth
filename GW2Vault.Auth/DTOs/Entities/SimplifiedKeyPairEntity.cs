using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.DTOs.Entities
{
    public class SimplifiedKeyPairEntity : Entity
    {
        public KeyPairPurpose Purpose { get; set; }
        public string PurposeDetails { get; set; }
        public KeyPairType? Type { get; set; }
    }
}
