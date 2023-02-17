namespace GW2Vault.Auth.DTOs.Requests
{
    public class CredentialsRequest
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
