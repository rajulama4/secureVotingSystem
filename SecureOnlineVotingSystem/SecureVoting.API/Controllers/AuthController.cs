using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Models;
using SecureVoting.API.Services;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        public record ChangePasswordRequest(int UserId, string NewPassword);
        public AuthController(AuthService auth) => _auth = auth;

        public record RegisterRequest(string FullName, string Email, string Password, string Role);
        //public record LoginRequest(string Email, string Password);
        public record LoginRequest(string LoginId, string Password);
        public record VerifyMfaRequest(int UserId, int ChallengeId, string Code);

        [HttpPost("register")]
        public IActionResult Register(RegisterRequest req)
        {
            if (req.Role != "Admin" && req.Role != "Voter")
                return BadRequest("Role must be Admin or Voter.");

            var (ok, msg) = _auth.Register(req.FullName, req.Email, req.Password, req.Role);
            return ok ? Ok(new { message = msg }) : BadRequest(new { message = msg });
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest req, [FromServices] TotpService totp)
        {
            var (ok, msg, user) = _auth.LoginPasswordOnly(req.LoginId, req.Password);
            if (!ok || user == null)
                return Unauthorized(new { message = msg });

            HttpContext.Items["LogUserId"] = user.UserId;
            HttpContext.Items["LogEmail"] = user.Email;

            if (user.MustChangePassword)
            {
                return Ok(new
                {
                    message = "PASSWORD_CHANGE_REQUIRED",
                    userId = user.UserId,
                    email = user.Email,
                    loginUserId = user.UserId
                });
            }

            var totpInfo = _auth.GetTotpInfoByLoginId(req.LoginId);
            if (totpInfo == null)
                return Unauthorized(new { message = "User not found." });

            if (!totpInfo.Value.enabled)
            {
                var issuer = "SecureVoting";
                var setup = totp.GenerateSetup(user.Email, issuer);

                _auth.EnableTotpForUser(user.Email, setup.secretBase32, issuer);

                return Ok(new
                {
                    message = "TOTP_ENROLL_REQUIRED",
                    email = user.Email,
                    otpauthUri = setup.otpauthUri,
                    qrCodeBase64Png = setup.qrBase64Png
                });
            }

            return Ok(new
            {
                message = "TOTP_REQUIRED",
                email = user.Email
            });
        }

        [HttpPost("verify-mfa")]
        public IActionResult VerifyMfa(VerifyMfaRequest req)
        {
            var (ok, msg, token) = _auth.VerifyMfaAndIssueToken(req.UserId, req.ChallengeId, req.Code);
            return ok ? Ok(new { message = msg, token }) : Unauthorized(new { message = msg });
        }


        [HttpPost("verify-totp")]
        public IActionResult VerifyTotp([FromBody] VerifyTotpRequest req, [FromServices] TotpService totp)
        {
            // 1) Load user first (so we can log UserId/Email even if TOTP fails)
            var user = _auth.GetUserByEmail(req.Email);
            if (user == null)
                return Unauthorized(new { message = "User not found." });

            // Put these for the logging middleware/repository (pre-auth request)
            HttpContext.Items["LogUserId"] = user.UserId;
            HttpContext.Items["LogEmail"] = user.Email;

            // 2) Load TOTP info
            var info = _auth.GetTotpInfoByEmail(req.Email);
            if (info == null)
                return Unauthorized(new { message = "User not found." });

            if (!info.Value.enabled || string.IsNullOrWhiteSpace(info.Value.secret))
                return BadRequest(new { message = "TOTP_NOT_ENROLLED" });

            // 3) Verify code
            var ok = totp.Verify(info.Value.secret!, req.Code);
            if (!ok)
                return Unauthorized(new { message = "INVALID_TOTP" });

            // 4) Issue token
            var token = _auth.GenerateJwt(user.UserId);
            return Ok(new { message = "Login successful.", token });
        }


        public record VerifyTotpRequest(string Email, string Code);






        [Authorize(Roles = "Admin")]
        [HttpGet("admin/api-logs")]
        public IActionResult GetLogs([FromServices] ApiLogRepository repo,
                             [FromServices] ApiLogCryptoService crypto)
        {
            var logs = repo.GetLatest(100); // returns encrypted bytes

            var result = logs.Select(l => new {
                l.LogId,
                l.UserId,
                l.Email,
                l.Endpoint,
                l.HttpMethod,
                l.RequestTimeUtc,
                l.IpAddress,
                l.StatusCode,
                l.IsSuccess,
                ApiReq = crypto.DecryptToString(l.ApiReqEnc),
                ApiRes = crypto.DecryptToString(l.ApiResEnc)
            });

            return Ok(result);
        }


        [AllowAnonymous]
        [HttpPost("register-voter")]
        public async Task<IActionResult> RegisterVoter([FromForm] RegisterVoterRequest req)
        {
            var result = await _auth.RegisterVoterAsync(req);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new
            {
                message = result.Message,
                userId = result.LoginUserId,
                password = result.TemporaryPassword
            });
        }

        [Authorize]
        [HttpGet("whoami")]
        public IActionResult WhoAmI()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }


        [AllowAnonymous]
        [HttpPost("change-temp-password")]
        public IActionResult ChangeTempPassword([FromBody] ChangePasswordRequest req)
        {
            if (req == null || req.UserId <= 0 || string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest(new { message = "UserId and new password are required." });

            var (ok, msg) = _auth.ChangeTemporaryPassword(req.UserId, req.NewPassword);
            return ok ? Ok(new { message = msg }) : BadRequest(new { message = msg });
        }

    }
}
