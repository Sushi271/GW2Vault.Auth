namespace GW2Vault.Auth.DTOs.Requests
{
    public class VerifyLicenseRequest
    {
        public string AccountName { get; set; }
        public string PcIdentifier { get; set; }
        public string MinerSignature { get; set; }
        public string Secret { get; set; }
    }
}
