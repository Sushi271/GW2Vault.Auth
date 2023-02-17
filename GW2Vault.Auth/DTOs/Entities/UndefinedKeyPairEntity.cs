namespace GW2Vault.Auth.DTOs.Entities
{
    public class UndefinedKeyPairEntity : AbstractKeyPairEntity
    {
        public override KeyPairPurpose Purpose => KeyPairPurpose.Undefined;
    }
}
