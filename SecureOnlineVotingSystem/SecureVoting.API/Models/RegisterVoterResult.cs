namespace SecureVoting.API.Models
{
    public class RegisterVoterResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string LoginUserId { get; set; } = string.Empty;
        public string TemporaryPassword { get; set; } = string.Empty;
    }
}