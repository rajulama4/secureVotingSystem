using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Models;
using SecureVoting.API.Services;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/candidates")]
    public class CandidatesController : ControllerBase
    {
        private readonly CandidateRepository _repo;
        private readonly ElectionRepository _erepo;
        private readonly IFileStorageService _fileStorage;

        public CandidatesController(CandidateRepository repo, ElectionRepository erepo, IFileStorageService fileStorage)
        {
            _repo = repo;
            _erepo = erepo;
            _fileStorage = fileStorage;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([FromForm] CreateCandidateRequest model)
        {
            if (model.ElectionId <= 0)
                return BadRequest(new { message = "Election is required." });

            if (string.IsNullOrWhiteSpace(model.CandidateName))
                return BadRequest(new { message = "Candidate name is required." });

            var election = _erepo.GetById(model.ElectionId);
            if (election == null)
                return NotFound(new { message = "Election not found." });

            string? imageFileName = null;
            string? imagePath = null;

            if (model.CandidatePicture != null && model.CandidatePicture.Length > 0)
            {
                var fileResult = _fileStorage.SaveCandidatePictureAsync(model.CandidatePicture).Result;

                if (!fileResult.ok)
                    return BadRequest(new { message = fileResult.message });

                imageFileName = fileResult.fileName;
                imagePath = fileResult.relativePath;
            }

            int id = _repo.CreateCandidate(model, imageFileName, imagePath);
            _erepo.EnsureCandidateResultRows(model.ElectionId);

            return Ok(new
            {
                message = "Candidate created successfully.",
                candidateId = id
            });
        }

        [HttpGet("election/{electionId:int}")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetByElection(int electionId)
        {
            return Ok(_repo.GetCandidatesByElectionId(electionId));
        }

        [HttpPost("{candidateId:int}/deactivate")]
        [Authorize(Roles = "Admin")]
        public IActionResult Deactivate(int candidateId)
        {
            bool ok = _repo.DeactivateCandidate(candidateId);

            return ok
                ? Ok(new { message = "Candidate deactivated successfully." })
                : BadRequest(new { message = "Candidate not found or already inactive." });
        }

        [HttpPost("update")]
        [Authorize(Roles = "Admin")]
        public IActionResult Update([FromForm] UpdateCandidateRequest model)
        {
            if (model.CandidateId <= 0)
                return BadRequest(new { message = "Candidate id is required." });

            if (string.IsNullOrWhiteSpace(model.CandidateName))
                return BadRequest(new { message = "Candidate name is required." });

            string? imageFileName = null;
            string? imagePath = null;
            bool replaceImage = false;

            if (model.CandidatePicture != null && model.CandidatePicture.Length > 0)
            {
                var fileResult = _fileStorage.SaveCandidatePictureAsync(model.CandidatePicture).Result;

                if (!fileResult.ok)
                    return BadRequest(new { message = fileResult.message });

                imageFileName = fileResult.fileName;
                imagePath = fileResult.relativePath;
                replaceImage = true;
            }

            bool ok = _repo.UpdateCandidate(model, imageFileName, imagePath, replaceImage);

            return ok
                ? Ok(new { message = "Candidate updated successfully." })
                : BadRequest(new { message = "Candidate update failed." });
        }
    }
}