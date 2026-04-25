namespace SecureVoting.API.Models
{
    public class Candidate
    {
        public int CandidateId { get; set; }
        public int ElectionId { get; set; }
        public string CandidateName { get; set; } = "";
        public string? Party { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public string? CandidateImagePath { get; set; }
        public string? CandidateImageFileName { get; set; }

    }
}