namespace SecureVoting.API.Models
{
    public class VoteCountStatusDto
    {
        public int ElectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? JurisdictionId { get; set; }
        public string? JurisdictionName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsClosed { get; set; }
        public bool IsPublished { get; set; }
        public int TotalVotes { get; set; }
        public string CountStatus { get; set; } = string.Empty;
    }
}