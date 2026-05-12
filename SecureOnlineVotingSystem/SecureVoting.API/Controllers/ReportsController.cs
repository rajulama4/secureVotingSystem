using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVotingSystem.Services;

namespace SecureVotingSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportsService _reportsService;

        public ReportsController(ReportsService reportsService)
        {
            _reportsService = reportsService;
        }

        [HttpGet("overview")]
        public IActionResult GetOverview()
        {
            var report = _reportsService.GetOverview();
            return Ok(report);
        }

        [HttpGet("voters-by-verification-status")]
        public IActionResult GetVotersByVerificationStatus()
        {
            var report = _reportsService.GetVotersByVerificationStatus();
            return Ok(report);
        }

        [HttpGet("election-turnout")]
        public IActionResult GetElectionTurnout()
        {
            var report = _reportsService.GetElectionTurnout();
            return Ok(report);
        }

        [HttpGet("candidate-performance")]
        public IActionResult GetCandidatePerformance()
        {
            var report = _reportsService.GetCandidatePerformance();
            return Ok(report);
        }
    }
}