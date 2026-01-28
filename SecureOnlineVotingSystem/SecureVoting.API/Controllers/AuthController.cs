using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Services;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth) => _auth = auth;

        public record RegisterRequest(string FullName, string Email, string Password, string Role);
        public record LoginRequest(string Email, string Password);
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
        public IActionResult Login(LoginRequest req)
        {
            var (ok, msg, challengeId, mfaCodeSim, user) = _auth.LoginStart(req.Email, req.Password);
            if (!ok) return Unauthorized(new { message = msg });

            // If MFA disabled, issue token immediately
            if (challengeId == null && user != null)
            {
                var token = _auth.GenerateJwt(user.UserId);
                return Ok(new { message = "Login successful.", token });
            }

            // MFA required:
            return Ok(new
            {
                message = msg,
                userId = user!.UserId,
                challengeId,
                // SIMULATION ONLY: remove in real deployments
                mfaCodeForSimulation = mfaCodeSim
            });
        }

        [HttpPost("verify-mfa")]
        public IActionResult VerifyMfa(VerifyMfaRequest req)
        {
            var (ok, msg, token) = _auth.VerifyMfaAndIssueToken(req.UserId, req.ChallengeId, req.Code);
            return ok ? Ok(new { message = msg, token }) : Unauthorized(new { message = msg });
        }
    }
}
