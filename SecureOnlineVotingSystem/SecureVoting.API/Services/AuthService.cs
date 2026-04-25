using Microsoft.IdentityModel.Tokens;
using SecureVoting.API.Data;
using SecureVoting.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SecureVoting.API.Services
{
    public class AuthService
    {
        private readonly UserRepository _users;
        private readonly MfaRepository _mfa;
        private readonly CryptoService _crypto;
        private readonly IConfiguration _config;
        private readonly IFileStorageService _fileStorage;
        public AuthService(UserRepository users, MfaRepository mfa, CryptoService crypto, IConfiguration config, IFileStorageService fileStorage)
        {
            _users = users;
            _mfa = mfa;
            _crypto = crypto;
            _config = config;
            _fileStorage = fileStorage;
        }

        public (bool ok, string message) Register(string fullName, string email, string password, string role)
        {
            if (_users.GetByEmail(email) != null)
                return (false, "Email already exists.");

            var (hash, salt) = _crypto.HashPassword(password);
            _users.Create(fullName, email, hash, salt, role);
            return (true, "Registered successfully.");
        }

        // Step 1: validate password; if MFA enabled => create challenge and return challengeId
        public (bool ok, string message, int? challengeId, string? mfaCodeForSimulation, User? user)
            LoginStart(string email, string password)
        {
            var user = _users.GetByEmail(email);
            if (user == null) return (false, "Invalid credentials.", null, null, null);

            if (!_crypto.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return (false, "Invalid credentials.", null, null, null);

            if (!user.IsMfaEnabled)
                return (true, "MFA not enabled; proceed to token.", null, null, user);

            var code = _crypto.GenerateMfaCode6Digits();
            var codeHash = _crypto.Sha256Bytes($"{user.UserId}:{code}");

            var expiresAt = DateTime.UtcNow.AddMinutes(5);
            int challengeId = _mfa.CreateChallenge(user.UserId, codeHash, expiresAt);

            // SIMULATION ONLY
            return (true, "MFA required.", challengeId, code, user);
        }

        // Step 2: verify MFA and issue JWT
        public (bool ok, string message, string? token)
            VerifyMfaAndIssueToken(int userId, int challengeId, string code)
        {
            var submittedHash = _crypto.Sha256Bytes($"{userId}:{code}");
            bool valid = _mfa.VerifyAndConsume(challengeId, userId, submittedHash);

            if (!valid) return (false, "Invalid or expired MFA code.", null);

            return (true, "Login successful.", GenerateJwt(userId));
        }

        public string GenerateJwt(int userId)
        {
            var user = GetUserByIdOrThrow(userId);

            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("fullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiresMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private User GetUserByIdOrThrow(int userId)
        {
            var user = _users.GetById(userId);
            return user ?? throw new InvalidOperationException("User not found.");
        }

        public async Task<RegisterVoterResult> RegisterVoterAsync(RegisterVoterRequest req)
        {
            if (req == null)
            {
                return new RegisterVoterResult
                {
                    Success = false,
                    Message = "Request is required."
                };
            }

            if (string.IsNullOrWhiteSpace(req.FullName) ||
                string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.PhoneNumber) ||
                string.IsNullOrWhiteSpace(req.AddressLine1) ||
                string.IsNullOrWhiteSpace(req.City) ||
                string.IsNullOrWhiteSpace(req.StateCode) ||
                string.IsNullOrWhiteSpace(req.ZipCode) ||
                string.IsNullOrWhiteSpace(req.DOB) ||
                string.IsNullOrWhiteSpace(req.IdDocumentType) ||
                string.IsNullOrWhiteSpace(req.IdDocumentNumber))
            {
                return new RegisterVoterResult
                {
                    Success = false,
                    Message = "Required registration fields are missing."
                };
            }

            if (req.IdPicture == null)
            {
                return new RegisterVoterResult
                {
                    Success = false,
                    Message = "ID picture is required."
                };
            }

            var fileResult = await _fileStorage.SaveIdPictureAsync(req.IdPicture);
            if (!fileResult.ok)
            {
                return new RegisterVoterResult
                {
                    Success = false,
                    Message = fileResult.message
                };
            }

            string loginUserId = "VOT-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
            string tempPassword = GenerateTemporaryPassword();

            var (hash, salt) = _crypto.HashPassword(tempPassword);

            return _users.RegisterVoter(
                req,
                hash,
                salt,
                loginUserId,
                tempPassword,
                fileResult.fileName!,
                fileResult.relativePath!);
        }

        public (bool ok, string message, User? user) LoginPasswordOnly(string loginId, string password)
        {
            var user = _users.GetByEmailOrLoginId(loginId);
            if (user == null) return (false, "Invalid credentials.", null);

            if (!_crypto.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return (false, "Invalid credentials.", null);

            return (true, "Password verified.", user);
        }

        public (bool enabled, string? secret, string? issuer)? GetTotpInfoByLoginId(string loginId)
        {
            var user = _users.GetByEmailOrLoginId(loginId);
            if (user == null) return null;

            return _users.GetTotpInfoByEmail(user.Email);
        }

        public (bool enabled, string? secret, string? issuer)? GetTotpInfoByEmail(string email)
            => _users.GetTotpInfoByEmail(email);

        public void EnableTotpForUser(string email, string secretBase32, string issuer)
        {
            _users.EnableTotpForUser(email, secretBase32, issuer);
        }

        public User? GetUserByEmail(string email)
            => _users.GetByEmail(email);

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }


        public (bool ok, string message) ChangeTemporaryPassword(int userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return (false, "New password is required.");

            if (newPassword.Length < 8)
                return (false, "Password must be at least 8 characters long.");

            var user = _users.GetById(userId);
            if (user == null)
                return (false, "User not found.");

            var (hash, salt) = _crypto.HashPassword(newPassword);
            _users.UpdatePasswordAndClearMustChange(userId, hash, salt);

            return (true, "Password changed successfully.");
        }
    }
}