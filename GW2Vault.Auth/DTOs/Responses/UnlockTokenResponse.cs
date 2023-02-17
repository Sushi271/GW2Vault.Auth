using System;

namespace GW2Vault.Auth.DTOs.Responses
{
    public class UnlockTokenResponse
    {
        public string AccountName{ get; set; }
        public string PcIdentifier { get; set; }
        public string TokenExpiration { get; set; }
        public string Secret { get; set; }
    }
}
