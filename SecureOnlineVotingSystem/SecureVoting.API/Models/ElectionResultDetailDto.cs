namespace SecureVoting.API.Models
{
    public class ElectionResultDetailDto
    {
        public int CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? Party { get; set; }
        public int VoteCount { get; set; }
        public bool IsWinner { get; set; }
        public string? CandidateImagePath { get; set; }
    }
}