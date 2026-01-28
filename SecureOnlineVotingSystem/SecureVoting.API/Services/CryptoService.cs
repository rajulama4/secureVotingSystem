using System.Security.Cryptography;
using System.Text;

namespace SecureVoting.API.Services
{
    public class CryptoService
    {

        public (byte[] hash, byte[] salt) HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return (hash, salt);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, 100_000, HashAlgorithmName.SHA256);
            byte[] computed = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }

        public byte[] Sha256Bytes(string input)
        {
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public string GenerateMfaCode6Digits()
        {
            // cryptographically strong 6-digit code
            int code = RandomNumberGenerator.GetInt32(0, 1_000_000);
            return code.ToString("D6");
        }
    }
}
