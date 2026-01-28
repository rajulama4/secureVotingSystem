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
        public IActionResult Create(Election model)
        {
            if (model.EndTime <= model.StartTime)
                return BadRequest("EndTime must be after StartTime.");

            int adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            int id = _repo.Create(model, adminId);

            return Ok(new { electionId = id });
        }

        // ADMIN ONLY: Close election
        [HttpPost("{id}/close")]
        [Authorize(Roles = "Admin")]
        public IActionResult Close(int id)
        {
            bool closed = _repo.CloseElection(id);
            return closed ? Ok("Election closed.") : BadRequest("Election already closed or not found.");
        }

        // ANY AUTHENTICATED USER: View elections
        [HttpGet]
        [Authorize]
        public IActionResult GetAll()
        {
            return Ok(_repo.GetAll());
        }

    }
}
