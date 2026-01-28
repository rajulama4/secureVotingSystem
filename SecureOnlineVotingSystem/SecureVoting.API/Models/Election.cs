namespace SecureVoting.API.Models
{
    public class Election
    {

        public int ElectionId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsClosed { get; set; }
    }
}
