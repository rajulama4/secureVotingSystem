namespace SecureVoting.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Role { get; set; }
        public bool IsMfaEnabled { get; set; }
        public bool TotpEnabled { get; set; }
        public string? TotpSecretBase32 { get; set; }
        public string? TotpIssuer { get; set; }
        public bool MustChangePassword { get; set; }

    }
}
