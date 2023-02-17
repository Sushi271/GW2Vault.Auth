namespace GW2Vault.Auth.Model
{
    public class ControllerActionKeyPair
    {
        public int Id { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public KeyPairType KeyPairTypeId { get; set; }
        public int KeyPairId { get; set; }
    }
}
