using System;

namespace GW2Vault.Auth.Model
{
    public class ActivationRequestLog
    {
        public int Id { get; set; }
        public string ActivationCode { get; set; }
        public string AccountName { get; set; }
        public string PcIdentifier { get; set; }
        public DateTime ReceivedDate { get; set; }
        public bool Successful { get; set; }
        public int? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
