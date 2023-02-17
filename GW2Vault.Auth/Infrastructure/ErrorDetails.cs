namespace GW2Vault.Auth.Infrastructure
{
    public class ErrorDetails
    {
        public int Code { get; set; }
        public string Message { get; set; }

        public ErrorDetails()
        {
        }

        public ErrorDetails(int code, string message)
        {
            Code = code;
            Message = message;
        }

        public override string ToString()
        {
            return $"Error {Code} : {Message}";
        }
    }
}
