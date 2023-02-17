namespace GW2Vault.Auth.DTOs.Requests
{
    public class ActivationRequest
    {
        public string ActivationCode { get; set; }
        public string AccountName { get; set; }
        public string PcIdentifier { get; set; }
    }
}
