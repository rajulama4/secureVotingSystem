using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Models;
using SecureVoting.API.Services;
using System.Text.Json;

namespace SecureVoting.API.Controllers
{

    [ApiController]
    [Route("api/results")]
    public class ResultsController : Controller
    {

        private readonly ElectionRepository _elections;
        private readonly VoteLedgerRepository _ledger;
        private readonly VoteCryptoService _crypto;

        public ResultsController(
            ElectionRepository elections,
            VoteLedgerRepository ledger,
            VoteCryptoService crypto)
        {
            _elections = elections;
            _ledger = ledger;
            _crypto = crypto;
        }


        // 🌍 PUBLIC: Get anonymous results for an election
        [HttpGet("{electionId}")]
        public IActionResult GetResults(int electionId)
        {
            var election = _elections.GetById(electionId);
            if (election == null) return NotFound("Election not found.");

            if (!election.IsClosed)
                return BadRequest("Election results are not available until the election is closed.");

            var votes = _ledger.GetVotesForElection(electionId);
            var results = new List<PublicVoteResult>();

            foreach (var (receiptId, encryptedVote) in votes)
            {
                string json = _crypto.DecryptToJson(encryptedVote);

                using var doc = JsonDocument.Parse(json);
                string choice = doc.RootElement.GetProperty("choice").GetString()!;

                results.Add(new PublicVoteResult
                {
                    ReceiptId = receiptId,
                    Choice = choice
                });
            }

            return Ok(results);
        }

        // 🔎 PUBLIC: Verify a vote using Receipt ID
        [HttpGet("verify/{receiptId}")]
        public IActionResult VerifyReceipt(Guid receiptId)
        {
            var encrypted = _ledger.GetEncryptedVoteByReceipt(receiptId);
            if (encrypted == null)
                return NotFound("Receipt ID not found.");

            string json = _crypto.DecryptToJson(encrypted);
            return Ok(JsonDocument.Parse(json));
        }

    }
}
