using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Services;
using System.Security.Claims;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/vote")]
    public class VotingController : ControllerBase
    {

        private readonly ElectionRepository _elections;
        private readonly VoterElectionStatusRepository _status;
        private readonly VoteLedgerRepository _ledger;
        private readonly VoteCryptoService _crypto;

        public VotingController(
            ElectionRepository elections,
            VoterElectionStatusRepository status,
            VoteLedgerRepository ledger,
            VoteCryptoService crypto)
        {
            _elections = elections;
            _status = status;
            _ledger = ledger;
            _crypto = crypto;
        }

        public record CastVoteRequest(int ElectionId, string Choice);

        // VOTER ONLY: Cast vote
        [HttpPost("cast")]
        [Authorize(Roles = "Voter")]
        public IActionResult Cast(CastVoteRequest req)
        {
            int voterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var election = _elections.GetById(req.ElectionId);
            if (election == null) return NotFound("Election not found.");
            if (election.IsClosed) return BadRequest("Election is closed.");

            var now = DateTime.UtcNow;
            if (now < election.StartTime.ToUniversalTime() || now > election.EndTime.ToUniversalTime())
                return BadRequest("Election is not active right now.");

            if (_status.HasVoted(req.ElectionId, voterId))
                return BadRequest("You have already voted in this election.");

            // Create anonymous receipt
            Guid receiptId = Guid.NewGuid();

            // Encrypt a minimal payload (do NOT include voterId)
            var payload = new
            {
                electionId = req.ElectionId,
                choice = req.Choice,
                castAtUtc = DateTime.UtcNow
            };

            byte[] encrypted = _crypto.EncryptVotePayload(payload);

            // Hash chain
            byte[]? prevHash = _ledger.GetLastVoteHash(req.ElectionId);
            byte[] voteHash = VoteLedgerRepository.ComputeVoteHash(req.ElectionId, receiptId, encrypted, prevHash);

            // Insert ledger entry
            _ledger.InsertVote(req.ElectionId, encrypted, receiptId, voteHash, prevHash);

            // Mark voted (separate table keeps identity off ledger)
            _status.MarkVoted(req.ElectionId, voterId);

            return Ok(new
            {
                message = "Vote cast successfully.",
                receiptId = receiptId
            });
        }

    }
}
