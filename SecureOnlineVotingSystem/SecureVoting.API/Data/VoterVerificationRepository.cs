using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class VoterVerificationRepository
    {
        private readonly DbHelper _db;

        public VoterVerificationRepository(DbHelper db)
        {
            _db = db;
        }

        public void Submit(
          int userId,
          string legalFullName,
          DateTime? dateOfBirth,
          string addressLine1,
          string? addressLine2,
          string city,
          string stateCode,
          string zipCode,
          int? jurisdictionId,
          string phoneNumber,
          string idDocumentType,
          string? idDocumentNumberMasked,
          string? idDocumentState,
          string? idPictureFileName,
          string? idPicturePath)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_VoterVerification_Submit", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@LegalFullName", SqlDbType.NVarChar, 150).Value = legalFullName;
            cmd.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value = (object?)dateOfBirth ?? DBNull.Value;
            cmd.Parameters.Add("@AddressLine1", SqlDbType.NVarChar, 200).Value = addressLine1;
            cmd.Parameters.Add("@AddressLine2", SqlDbType.NVarChar, 200).Value = (object?)addressLine2 ?? DBNull.Value;
            cmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = city;
            cmd.Parameters.Add("@StateCode", SqlDbType.NVarChar, 20).Value = stateCode;
            cmd.Parameters.Add("@ZipCode", SqlDbType.NVarChar, 20).Value = zipCode;
            cmd.Parameters.Add("@JurisdictionId", SqlDbType.Int).Value = (object?)jurisdictionId ?? DBNull.Value;
            cmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 30).Value = phoneNumber;
            cmd.Parameters.Add("@IdDocumentType", SqlDbType.NVarChar, 50).Value = idDocumentType;
            cmd.Parameters.Add("@IdDocumentNumberMasked", SqlDbType.NVarChar, 50).Value = (object?)idDocumentNumberMasked ?? DBNull.Value;
            cmd.Parameters.Add("@IdDocumentState", SqlDbType.NVarChar, 20).Value = (object?)idDocumentState ?? DBNull.Value;
            cmd.Parameters.Add("@IdPictureFileName", SqlDbType.NVarChar, 255).Value = idPictureFileName;
            cmd.Parameters.Add("@IdPicturePath", SqlDbType.NVarChar, 500).Value = idPicturePath;

            conn.Open();
            cmd.ExecuteNonQuery();
        }


        public VoterVerification? GetByUserId(int userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_VoterVerification_GetByUserId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            if (!r.Read()) return null;

            return new VoterVerification
            {
                VerificationId = r.GetInt32(r.GetOrdinal("VerificationId")),
                UserId = r.GetInt32(r.GetOrdinal("UserId")),
                LegalFullName = r.GetString(r.GetOrdinal("LegalFullName")),
                DateOfBirth = r.IsDBNull(r.GetOrdinal("DateOfBirth")) ? null : r.GetDateTime(r.GetOrdinal("DateOfBirth")),
                AddressLine1 = r.GetString(r.GetOrdinal("AddressLine1")),
                AddressLine2 = r.IsDBNull(r.GetOrdinal("AddressLine2")) ? null : r.GetString(r.GetOrdinal("AddressLine2")),
                City = r.GetString(r.GetOrdinal("City")),
                StateCode = r.GetString(r.GetOrdinal("StateCode")),
                ZipCode = r.GetString(r.GetOrdinal("ZipCode")),
                JurisdictionId = r.IsDBNull(r.GetOrdinal("JurisdictionId")) ? null : r.GetInt32(r.GetOrdinal("JurisdictionId")),
                IdDocumentType = r.GetString(r.GetOrdinal("IdDocumentType")),
                IdDocumentNumberMasked = r.IsDBNull(r.GetOrdinal("IdDocumentNumberMasked")) ? null : r.GetString(r.GetOrdinal("IdDocumentNumberMasked")),
                IdDocumentState = r.IsDBNull(r.GetOrdinal("IdDocumentState")) ? null : r.GetString(r.GetOrdinal("IdDocumentState")),
                IsIdentityVerified = r.GetBoolean(r.GetOrdinal("IsIdentityVerified")),
                IsResidenceVerified = r.GetBoolean(r.GetOrdinal("IsResidenceVerified")),
                IsEligibleToVote = r.GetBoolean(r.GetOrdinal("IsEligibleToVote")),
                VerificationStatus = r.GetString(r.GetOrdinal("VerificationStatus")),
                ReviewerNotes = r.IsDBNull(r.GetOrdinal("ReviewerNotes")) ? null : r.GetString(r.GetOrdinal("ReviewerNotes")),
                ReviewedByUserId = r.IsDBNull(r.GetOrdinal("ReviewedByUserId")) ? null : r.GetInt32(r.GetOrdinal("ReviewedByUserId")),
                ReviewedAtUtc = r.IsDBNull(r.GetOrdinal("ReviewedAtUtc")) ? null : r.GetDateTime(r.GetOrdinal("ReviewedAtUtc")),
                SubmittedAtUtc = r.GetDateTime(r.GetOrdinal("SubmittedAtUtc")),
                IdPictureFileName = r.IsDBNull(r.GetOrdinal("idDocumentImageFileName")) ? null : r.GetString(r.GetOrdinal("idDocumentImageFileName")),
                IdPicturePath = r.IsDBNull(r.GetOrdinal("idDocumentImagePath")) ? null : r.GetString(r.GetOrdinal("idDocumentImagePath")),
                UpdatedAtUtc = r.GetDateTime(r.GetOrdinal("UpdatedAtUtc"))
            };
        }

        public void Approve(int userId, int reviewedByUserId, string? reviewerNotes, int jurisdictionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_VoterVerification_Approve", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@ReviewedByUserId", SqlDbType.Int).Value = reviewedByUserId;
            cmd.Parameters.Add("@JurisdictionId", SqlDbType.Int).Value = jurisdictionId;
            cmd.Parameters.Add("@ReviewerNotes", SqlDbType.NVarChar, 500).Value = (object?)reviewerNotes ?? DBNull.Value;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void Reject(int userId, int reviewedByUserId, string reviewerNotes)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_VoterVerification_Reject", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@ReviewedByUserId", SqlDbType.Int).Value = reviewedByUserId;
            cmd.Parameters.Add("@ReviewerNotes", SqlDbType.NVarChar, 500).Value = reviewerNotes;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool CanVoteInElection(int userId, int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_VoterVerification_CanVoteInElection", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null && Convert.ToInt32(result) == 1;
        }

        public List<VoterVerification> GetAll()
        {
            var list = new List<VoterVerification>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT *
        FROM dbo.VoterVerifications
        ORDER BY SubmittedAtUtc DESC
    ", conn);

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new VoterVerification
                {
                    VerificationId = r.GetInt32(r.GetOrdinal("VerificationId")),
                    UserId = r.GetInt32(r.GetOrdinal("UserId")),
                    LegalFullName = r.GetString(r.GetOrdinal("LegalFullName")),
                    DateOfBirth = r.IsDBNull(r.GetOrdinal("DateOfBirth")) ? null : r.GetDateTime(r.GetOrdinal("DateOfBirth")),
                    AddressLine1 = r.GetString(r.GetOrdinal("AddressLine1")),
                    AddressLine2 = r.IsDBNull(r.GetOrdinal("AddressLine2")) ? null : r.GetString(r.GetOrdinal("AddressLine2")),
                    City = r.GetString(r.GetOrdinal("City")),
                    StateCode = r.GetString(r.GetOrdinal("StateCode")),
                    ZipCode = r.GetString(r.GetOrdinal("ZipCode")),
                    JurisdictionId = r.IsDBNull(r.GetOrdinal("JurisdictionId")) ? null : r.GetInt32(r.GetOrdinal("JurisdictionId")),
                    IdDocumentType = r.GetString(r.GetOrdinal("IdDocumentType")),
                    IdDocumentNumberMasked = r.IsDBNull(r.GetOrdinal("IdDocumentNumberMasked")) ? null : r.GetString(r.GetOrdinal("IdDocumentNumberMasked")),
                    IdDocumentState = r.IsDBNull(r.GetOrdinal("IdDocumentState")) ? null : r.GetString(r.GetOrdinal("IdDocumentState")),
                    IsIdentityVerified = r.GetBoolean(r.GetOrdinal("IsIdentityVerified")),
                    IsResidenceVerified = r.GetBoolean(r.GetOrdinal("IsResidenceVerified")),
                    IsEligibleToVote = r.GetBoolean(r.GetOrdinal("IsEligibleToVote")),
                    VerificationStatus = r.GetString(r.GetOrdinal("VerificationStatus")),
                    ReviewerNotes = r.IsDBNull(r.GetOrdinal("ReviewerNotes")) ? null : r.GetString(r.GetOrdinal("ReviewerNotes")),
                    ReviewedByUserId = r.IsDBNull(r.GetOrdinal("ReviewedByUserId")) ? null : r.GetInt32(r.GetOrdinal("ReviewedByUserId")),
                    ReviewedAtUtc = r.IsDBNull(r.GetOrdinal("ReviewedAtUtc")) ? null : r.GetDateTime(r.GetOrdinal("ReviewedAtUtc")),
                    SubmittedAtUtc = r.GetDateTime(r.GetOrdinal("SubmittedAtUtc")),
                    IdPictureFileName = r.IsDBNull(r.GetOrdinal("IdPictureFileName")) ? null : r.GetString(r.GetOrdinal("IdPictureFileName")),
                    IdPicturePath = r.IsDBNull(r.GetOrdinal("IdPicturePath")) ? null : r.GetString(r.GetOrdinal("IdPicturePath")),
                    UpdatedAtUtc = r.GetDateTime(r.GetOrdinal("UpdatedAtUtc"))
                });
            }

            return list;
        }

        public List<VoterVerification> GetPending()
        {
            var list = new List<VoterVerification>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT *
        FROM dbo.VoterVerifications
        WHERE VerificationStatus = 'Pending'
        ORDER BY SubmittedAtUtc DESC
    ", conn);

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new VoterVerification
                {
                    VerificationId = r.GetInt32(r.GetOrdinal("VerificationId")),
                    UserId = r.GetInt32(r.GetOrdinal("UserId")),
                    LegalFullName = r.GetString(r.GetOrdinal("LegalFullName")),
                    DateOfBirth = r.IsDBNull(r.GetOrdinal("DateOfBirth")) ? null : r.GetDateTime(r.GetOrdinal("DateOfBirth")),
                    AddressLine1 = r.GetString(r.GetOrdinal("AddressLine1")),
                    AddressLine2 = r.IsDBNull(r.GetOrdinal("AddressLine2")) ? null : r.GetString(r.GetOrdinal("AddressLine2")),
                    City = r.GetString(r.GetOrdinal("City")),
                    StateCode = r.GetString(r.GetOrdinal("StateCode")),
                    ZipCode = r.GetString(r.GetOrdinal("ZipCode")),
                    JurisdictionId = r.IsDBNull(r.GetOrdinal("JurisdictionId")) ? null : r.GetInt32(r.GetOrdinal("JurisdictionId")),
                    IdDocumentType = r.GetString(r.GetOrdinal("IdDocumentType")),
                    IdDocumentNumberMasked = r.IsDBNull(r.GetOrdinal("IdDocumentNumberMasked")) ? null : r.GetString(r.GetOrdinal("IdDocumentNumberMasked")),
                    IdDocumentState = r.IsDBNull(r.GetOrdinal("IdDocumentState")) ? null : r.GetString(r.GetOrdinal("IdDocumentState")),
                    IsIdentityVerified = r.GetBoolean(r.GetOrdinal("IsIdentityVerified")),
                    IsResidenceVerified = r.GetBoolean(r.GetOrdinal("IsResidenceVerified")),
                    IsEligibleToVote = r.GetBoolean(r.GetOrdinal("IsEligibleToVote")),
                    VerificationStatus = r.GetString(r.GetOrdinal("VerificationStatus")),
                    ReviewerNotes = r.IsDBNull(r.GetOrdinal("ReviewerNotes")) ? null : r.GetString(r.GetOrdinal("ReviewerNotes")),
                    ReviewedByUserId = r.IsDBNull(r.GetOrdinal("ReviewedByUserId")) ? null : r.GetInt32(r.GetOrdinal("ReviewedByUserId")),
                    ReviewedAtUtc = r.IsDBNull(r.GetOrdinal("ReviewedAtUtc")) ? null : r.GetDateTime(r.GetOrdinal("ReviewedAtUtc")),
                    SubmittedAtUtc = r.GetDateTime(r.GetOrdinal("SubmittedAtUtc")),
                    IdPictureFileName = r.IsDBNull(r.GetOrdinal("idDocumentImageFileName")) ? null : r.GetString(r.GetOrdinal("idDocumentImageFileName")),
                    IdPicturePath = r.IsDBNull(r.GetOrdinal("idDocumentImagePath")) ? null : r.GetString(r.GetOrdinal("idDocumentImagePath")),
                    UpdatedAtUtc = r.GetDateTime(r.GetOrdinal("UpdatedAtUtc"))
                });
            }

            return list;
        }
    }
}