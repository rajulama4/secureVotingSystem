using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Models;
using System.Security.Claims;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/elections")]
    public class ElectionsController : ControllerBase
    {
        private readonly ElectionRepository _repo;

        public ElectionsController(ElectionRepository repo)
        {
            _repo = repo;
        }

        // ADMIN ONLY: Create election
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromBody] Election model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
                return BadRequest(new { message = "Title is required." });

            if (model.EndTime <= model.StartTime)
                return BadRequest(new { message = "EndTime must be after StartTime." });

            if (!model.JurisdictionId.HasValue || model.JurisdictionId <= 0)
                return BadRequest(new { message = "Jurisdiction is required." });

            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            int id = _repo.Create(model, adminId);

            return Ok(new
            {
                message = "Election created successfully.",
                electionId = id
            });
        }

        // ADMIN ONLY: Close election
        [HttpPost("{id:int}/close")]
        [Authorize(Roles = "Admin")]
        public IActionResult Close(int id)
        {
            bool closed = _repo.CloseElection(id);

            return closed
                ? Ok(new { message = "Election closed successfully." })
                : BadRequest(new { message = "Election already closed or not found." });
        }

        // ADMIN ONLY: Publish results
        [HttpPost("{id:int}/publish-results")]
        [Authorize(Roles = "Admin")]
        public IActionResult PublishResults(int id)
        {
            bool published = _repo.PublishResults(id);

            return published
                ? Ok(new { message = "Results published successfully." })
                : BadRequest(new { message = "Results could not be published. Make sure the election is closed first." });
        }

        // ANY AUTHENTICATED USER: View elections
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            return Ok(_repo.GetAll());
        }



        [HttpGet("{electionId:int}/candidates")]
        [Authorize(Roles = "Voter,Admin")]
        public IActionResult GetCandidatesByElection(int electionId)
        {
            var election = _repo.GetById(electionId);
            if (election == null)
                return NotFound(new { message = "Election not found." });

            var candidates = _repo.GetCandidatesByElectionId(electionId);

            return Ok(candidates);
        }



        [HttpGet("mine")]
        [Authorize(Roles = "Voter")]
        public IActionResult GetMine()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userIdValue))
                return Unauthorized(new { message = "Invalid user session." });

            int userId = int.Parse(userIdValue);

            var jurisdictionId = _repo.GetJurisdictionId(userId);

            if (!jurisdictionId.HasValue)
                return BadRequest(new { message = "Your jurisdiction is not assigned yet. Please contact admin." });

            return Ok(_repo.GetByJurisdiction(jurisdictionId.Value));
        }


        [HttpGet("count-status")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetCountStatus()
        {
            return Ok(_repo.GetVoteCountStatus());
        }


        [HttpGet("{electionId:int}/result-details")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetResultDetails(int electionId)
        {
            var election = _repo.GetById(electionId);
            if (election == null)
                return NotFound(new { message = "Election not found." });

            return Ok(_repo.GetElectionResultDetails(electionId));
        }
    }
}