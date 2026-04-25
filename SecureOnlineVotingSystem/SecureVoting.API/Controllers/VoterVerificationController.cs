using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using System.Security.Claims;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/voter-verification")]
    public class VoterVerificationController : ControllerBase
    {
        private readonly VoterVerificationRepository _repo;

        public VoterVerificationController(VoterVerificationRepository repo)
        {
            _repo = repo;
        }

        public record SubmitVerificationRequest(
            string LegalFullName,
            DateTime? DateOfBirth,
            string AddressLine1,
            string? AddressLine2,
            string City,
            string StateCode,
            string ZipCode,
            int? JurisdictionId,
            string PhoneNumber,
            string IdDocumentType,
            string? IdDocumentNumberMasked,
            string? IdDocumentState,
            string? idPictureFileName,
            string? idPicturePath
        );

        public record ReviewRequest(string? ReviewerNotes, int? JurisdictionId);

        [Authorize(Roles = "Voter")]
        [HttpPost("submit")]
        public IActionResult Submit([FromBody] SubmitVerificationRequest req)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (string.IsNullOrWhiteSpace(req.LegalFullName))
                return BadRequest(new { message = "Legal full name is required." });

            if (string.IsNullOrWhiteSpace(req.AddressLine1) ||
                string.IsNullOrWhiteSpace(req.City) ||
                string.IsNullOrWhiteSpace(req.StateCode) ||
                string.IsNullOrWhiteSpace(req.ZipCode) ||
                string.IsNullOrWhiteSpace(req.PhoneNumber) ||
                string.IsNullOrWhiteSpace(req.IdDocumentType))
            {
                return BadRequest(new { message = "Required voter verification fields are missing." });
            }

            _repo.Submit(
                userId,
                req.LegalFullName,
                req.DateOfBirth,
                req.AddressLine1,
                req.AddressLine2,
                req.City,
                req.StateCode,
                req.ZipCode,
                req.JurisdictionId,
                req.PhoneNumber,
                req.IdDocumentType,
                req.IdDocumentNumberMasked,
                req.IdDocumentState,
                req.idPictureFileName,
                req.idPicturePath
            );

            return Ok(new { message = "Verification submitted successfully." });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMyVerification()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var data = _repo.GetByUserId(userId);
            if (data == null)
                return NotFound(new { message = "No verification record found." });

            return Ok(data);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId:int}/approve")]
        public IActionResult Approve(int userId, [FromBody] ReviewRequest req)
        {

            if (req?.JurisdictionId == null || req.JurisdictionId <= 0)
                return BadRequest(new { message = "Jurisdiction is required for approval." });

            int reviewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _repo.Approve(userId, reviewerId, req.ReviewerNotes, req.JurisdictionId.Value);
            return Ok(new { message = "Voter verification approved." });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{userId:int}/reject")]
        public IActionResult Reject(int userId, [FromBody] ReviewRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.ReviewerNotes))
                return BadRequest(new { message = "Reviewer notes are required for rejection." });

            int reviewerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            _repo.Reject(userId, reviewerId, req.ReviewerNotes);
            return Ok(new { message = "Voter verification rejected." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public IActionResult GetAll()
        {
            return Ok(_repo.GetAll());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            return Ok(_repo.GetPending());
        }



    }
}