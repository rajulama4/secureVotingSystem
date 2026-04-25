namespace SecureVoting.API.Models
{
    public class VoteResult
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = "";
        public string? Party { get; set; }
        public int VoteCount { get; set; }
    }
}
