namespace GW2Vault.Auth.DTOs.Entities
{
    public abstract class AbstractKeyPairEntity : Entity
    {
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }

        public abstract KeyPairPurpose Purpose { get; }
    }
}
