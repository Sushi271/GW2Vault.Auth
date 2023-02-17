using System;

namespace GW2Vault.Auth.Model
{
    public class MachineActivation
    {
        public int Id { get; set; }
        public int ActivationCodeId { get; set; }
        public int MachineId { get; set; }
        public DateTime ActivationDate { get; set; }
    }
}
