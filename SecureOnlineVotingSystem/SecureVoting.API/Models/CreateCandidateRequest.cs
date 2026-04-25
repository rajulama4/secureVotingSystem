namespace SecureVoting.API.Models
{
    public class CreateCandidateRequest
    {

        public int ElectionId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string? Party { get; set; }
        public string? Bio { get; set; }

        public IFormFile? CandidatePicture { get; set; }
    }



}
