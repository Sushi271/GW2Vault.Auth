using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.DTOs.Entities
{
    public class ControllerActionKeyPairEntity : AbstractKeyPairEntity
    {
        public override KeyPairPurpose Purpose => KeyPairPurpose.ControllerAction;

        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public KeyPairType? Type { get; set; }
    }
}
