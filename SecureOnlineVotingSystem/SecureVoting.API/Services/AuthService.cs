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

        public AuthService(UserRepository users, MfaRepository mfa, CryptoService crypto, IConfiguration config)
        {
            _users = users;
            _mfa = mfa;
            _crypto = crypto;
            _config = config;
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

            // SIMULATION ONLY: we return the code so you can demo without SMS/email.
            return (true, "MFA required.", challengeId, code, user);
        }

        // Step 2: verify MFA and issue JWT
        public (bool ok, string message, string? token)
            VerifyMfaAndIssueToken(int userId, int challengeId, string code)
        {
            var submittedHash = _crypto.Sha256Bytes($"{userId}:{code}");
            bool valid = _mfa.VerifyAndConsume(challengeId, userId, submittedHash);

            if (!valid) return (false, "Invalid or expired MFA code.", null);

            // Issue JWT
            return (true, "Login successful.", GenerateJwt(userId));
        }

        public string GenerateJwt(int userId)
        {
            // minimal: read user again for role/email claims
            // (you could also pass User object in if you prefer)
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
            // small helper: easiest approach is to add GetById in UserRepository.
            // For now, reuse GetByEmail isn’t possible; so add GetById quickly:
            // Implement in repository if you want, but here's a quick inline approach:
            var user = _users.GetById(userId);
            return user ?? throw new InvalidOperationException("User not found.");
        }
    }
}
