using System;

namespace GW2Vault.Auth.Model
{
    public class ActivationCode
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public bool Available { get; set; }
        public DateTime ExpirationDate { get; set; }
        public int DaysGranted { get; set; }
        public ActivationType ActivationTypeId { get; set; }
    }
}
