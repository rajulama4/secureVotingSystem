using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Models;
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
        private readonly VoterVerificationRepository _verification;
        private readonly CandidateRepository _candidates;

        public VotingController(
            ElectionRepository elections,
            VoterElectionStatusRepository status,
            VoteLedgerRepository ledger,
            VoteCryptoService crypto,
            VoterVerificationRepository verification,
            CandidateRepository candidates)
        {
            _elections = elections;
            _status = status;
            _ledger = ledger;
            _crypto = crypto;
            _verification = verification;
            _candidates = candidates;
            _candidates = candidates;
        }

        public record CastVoteRequest(int ElectionId, int CandidateId);

        // VOTER ONLY: Cast vote
        [HttpPost("cast")]
        [Authorize(Roles = "Voter")]
        public IActionResult Cast(CastVoteRequest req)
        {
            int voterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var election = _elections.GetById(req.ElectionId);
            if (election == null) return NotFound("Election not found.");
            if (election.IsClosed) return BadRequest("Election is closed.");

            var candidate = _candidates.GetById(req.CandidateId);
            if (candidate == null) return NotFound(new { message = "Candidate not found." });
            if (candidate.ElectionId != req.ElectionId)
                return BadRequest(new { message = "Candidate does not belong to the selected election." });

            var now = DateTime.UtcNow;
            if (now < election.StartTime.ToUniversalTime() || now > election.EndTime.ToUniversalTime())
                return BadRequest("Election is not active right now.");

            if (_status.HasVoted(req.ElectionId, voterId))
                return BadRequest("You have already voted in this election.");

            if (!_verification.CanVoteInElection(voterId, req.ElectionId))
                return BadRequest(new { message = "You are not eligible to vote in this election." });

            Guid receiptId = Guid.NewGuid();

            var payload = new
            {
                electionId = req.ElectionId,
                candidateId = req.CandidateId,
                candidateName = candidate.CandidateName,
                castAtUtc = DateTime.UtcNow
            };

            byte[] encrypted = _crypto.EncryptVotePayload(payload);

            byte[]? prevHash = _ledger.GetLastVoteHash(req.ElectionId);
            byte[] voteHash = VoteLedgerRepository.ComputeVoteHash(req.ElectionId, receiptId, encrypted, prevHash);

            _ledger.InsertVote(req.ElectionId, encrypted, receiptId, voteHash, prevHash);

            _status.MarkVoted(req.ElectionId, voterId);

            _elections.EnsureCandidateResultRows(req.ElectionId);
            _elections.IncrementCandidateVoteCount(req.ElectionId, req.CandidateId);

            return Ok(new
            {
                message = "Vote cast successfully.",
                receiptId = receiptId
            });
        }

    }
}
