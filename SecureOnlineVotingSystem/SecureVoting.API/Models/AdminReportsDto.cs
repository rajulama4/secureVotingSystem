namespace SecureVotingSystem.Models
{
    public class AdminReportsOverviewDto
    {
        public int TotalVoters { get; set; }
        public int VerifiedVoters { get; set; }
        public int PendingVerifications { get; set; }
        public int RejectedVerifications { get; set; }
        public int TotalElections { get; set; }
        public int PublishedElections { get; set; }
        public int ClosedElections { get; set; }
        public int TotalVotesCast { get; set; }
        public int TotalCandidates { get; set; }
    }

    public class VoterVerificationStatusReportDto
    {
        public string VerificationStatus { get; set; } = "";
        public int TotalCount { get; set; }
    }

    public class ElectionTurnoutReportDto
    {
        public int ElectionId { get; set; }
        public string ElectionTitle { get; set; } = "";
        public int TotalEligibleVoters { get; set; }
        public int TotalVotesCast { get; set; }
        public decimal TurnoutPercentage { get; set; }
    }

    public class CandidatePerformanceReportDto
    {
        public int ElectionId { get; set; }
        public string ElectionTitle { get; set; } = "";
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = "";
        public string? Party { get; set; }
        public int VoteCount { get; set; }
        public decimal VotePercentage { get; set; }
    }
}