namespace GW2Vault.Auth.Model
{
    public class AccountMachineKeyPair
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int MachineId { get; set; }
        public KeyPairType KeyPairTypeId { get; set; }
        public int KeyPairId { get; set; }
    }
}
