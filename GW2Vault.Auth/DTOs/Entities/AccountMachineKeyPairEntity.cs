using GW2Vault.Auth.Model;

namespace GW2Vault.Auth.DTOs.Entities
{
    public class AccountMachineKeyPairEntity : AbstractKeyPairEntity
    {
        public override KeyPairPurpose Purpose => KeyPairPurpose.AccountMachine;
        
        public int MachineId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public KeyPairType? Type { get; set; }
    }
}
