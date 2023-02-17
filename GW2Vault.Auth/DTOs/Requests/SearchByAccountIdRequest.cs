namespace GW2Vault.Auth.DTOs.Requests
{
    public class SearchByAccountIdRequest : CredentialsRequest
    {
        public int AccountId { get; set; }
    }
}
