namespace SecureVoting.API.Models
{
    public class VoteLedgerEntry
    {
        public int LedgerId { get; set; }
        public int ElectionId { get; set; }
        public byte[] EncryptedVote { get; set; }
        public Guid ReceiptId { get; set; }
        public byte[] VoteHash { get; set; }
        public byte[] PreviousHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
