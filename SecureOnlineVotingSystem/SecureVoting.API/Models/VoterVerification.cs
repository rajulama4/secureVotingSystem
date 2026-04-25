namespace SecureVoting.API.Models
{
    public class VoterVerification
    {
        public int VerificationId { get; set; }
        public int UserId { get; set; }

        public string LegalFullName { get; set; } = "";
        public DateTime? DateOfBirth { get; set; }

        public string AddressLine1 { get; set; } = "";
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = "";
        public string StateCode { get; set; } = "";
        public string ZipCode { get; set; } = "";

        public int? JurisdictionId { get; set; }

        public string IdDocumentType { get; set; } = "";
        public string? IdDocumentNumberMasked { get; set; }
        public string? IdDocumentState { get; set; }

        public bool IsIdentityVerified { get; set; }
        public bool IsResidenceVerified { get; set; }
        public bool IsEligibleToVote { get; set; }

        public string VerificationStatus { get; set; } = "";
        public string? ReviewerNotes { get; set; }
        public int? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }

        public DateTime SubmittedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
        public string? PhoneNumber { get; set; }

        public string? IdPictureFileName { get; set; }
        public string? IdPicturePath { get; set; }
    }
}