using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Services;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/mfa")]
    public class MfaController : ControllerBase
    {
        private readonly TotpService _totp;
        private readonly UserRepository _users;

        public MfaController(TotpService totp, UserRepository users)
        {
            _totp = totp;
            _users = users;
        }

        public record EnrollRequest(string Email);

        [HttpPost("enroll")]
        public IActionResult Enroll([FromBody] EnrollRequest req)
        {
            var info = _users.GetTotpInfoByEmail(req.Email);
            if (info == null) return NotFound("User not found.");

            // already enabled -> return minimal info
            if (info.Value.enabled)
                return Ok(new { message = "TOTP_ALREADY_ENABLED" });

            var issuer = "SecureVoting";
            var setup = _totp.GenerateSetup(req.Email, issuer);

            _users.EnableTotpForUser(req.Email, setup.secretBase32, issuer);

            return Ok(new
            {
                message = "TOTP_ENROLL_REQUIRED",
                otpauthUri = setup.otpauthUri,
                qrCodeBase64Png = setup.qrBase64Png
            });
        }
    }
}
