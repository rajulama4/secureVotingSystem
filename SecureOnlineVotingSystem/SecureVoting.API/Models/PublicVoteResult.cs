namespace SecureVoting.API.Models
{
    public class PublicVoteResult
    {
        public Guid ReceiptId { get; set; }
        public string Choice { get; set; } = "";
    }
}
