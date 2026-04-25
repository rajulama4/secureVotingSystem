namespace SecureVoting.API.Models
{
    public class RegisterVoterRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;

        public int? JurisdictionId { get; set; }

        public string IdDocumentType { get; set; } = string.Empty;
        public string IdDocumentNumber { get; set; } = string.Empty;
        public string? IdDocumentState { get; set; }
        public string DOB { get; set; } = string.Empty;
        public IFormFile? IdPicture { get; set; }
    }
}