using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;


namespace SecureVoting.API.Data
{
    public class UserRepository
    {
        private readonly DbHelper _db;
        public UserRepository(DbHelper db) => _db = db;

        public User? GetByEmail(string email)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(
                @"SELECT UserId, FullName, Email, PasswordHash, PasswordSalt, Role, IsMfaEnabled,TotpEnabled,TotpSecretBase32,TotpIssuer
                  FROM Users WHERE Email = @Email", conn);

            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                UserId = r.GetInt32(0),
                FullName = r.GetString(1),
                Email = r.GetString(2),
                PasswordHash = (byte[])r[3],
                PasswordSalt = (byte[])r[4],
                Role = r.GetString(5),
                IsMfaEnabled = r.GetBoolean(6),

                TotpEnabled = r.GetBoolean(7),
                TotpSecretBase32 = r.IsDBNull(8) ? null : r.GetString(8),
                TotpIssuer = r.IsDBNull(9) ? null : r.GetString(9)
            };
        }

        public int Create(string fullName, string email, byte[] passwordHash, byte[] passwordSalt, string role)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Role, IsMfaEnabled,TotpEnabled)
                VALUES (@FullName, @Email, @PasswordHash, @PasswordSalt, @Role, 0, 0);
                SELECT SCOPE_IDENTITY();
            ", conn);

            cmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = fullName;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = email;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 256).Value = passwordHash;
            cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 256).Value = passwordSalt;
            cmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = role;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public User? GetById(int userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(
                @"SELECT UserId, FullName, Email, PasswordHash, PasswordSalt, Role, IsMfaEnabled
          FROM Users WHERE UserId = @UserId", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                UserId = r.GetInt32(0),
                FullName = r.GetString(1),
                Email = r.GetString(2),
                PasswordHash = (byte[])r[3],
                PasswordSalt = (byte[])r[4],
                Role = r.GetString(5),
                IsMfaEnabled = r.GetBoolean(6)
            };
        }


        public (bool enabled, string? secret, string? issuer)? GetTotpInfoByEmail(string email)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT TotpEnabled, TotpSecretBase32, TotpIssuer
        FROM Users
        WHERE Email = @Email
    ", conn);

            cmd.Parameters.AddWithValue("@Email", email);

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return (r.GetBoolean(0), r.IsDBNull(1) ? null : r.GetString(1), r.IsDBNull(2) ? null : r.GetString(2));
        }



        public void EnableTotpForUser(string email, string secretBase32, string issuer)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE Users
                SET TotpEnabled = 1,
                    TotpSecretBase32 = @Secret,
                    TotpIssuer = @Issuer
                WHERE Email = @Email
                    ", conn);

            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Secret", secretBase32);
            cmd.Parameters.AddWithValue("@Issuer", issuer);

            conn.Open();
            cmd.ExecuteNonQuery();
        }






        public RegisterVoterResult RegisterVoter(RegisterVoterRequest req,
    byte[] passwordHash,
    byte[] passwordSalt,
    string loginUserId,
    string tempPassword,
    string idPictureFileName,
    string idPicturePath)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            using var tx = conn.BeginTransaction();

            try
            {
                // Check duplicate email
                using (var checkCmd = new SqlCommand(
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email", conn, tx))
                {
                    checkCmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = req.Email.Trim();

                    int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (exists > 0)
                    {
                        tx.Rollback();
                        return new RegisterVoterResult
                        {
                            Success = false,
                            Message = "Email already exists."
                        };
                    }
                }

                //string loginUserId = GenerateLoginUserId();
                //string tempPassword = GenerateTemporaryPassword();

                //CreatePasswordHash(tempPassword, out byte[] passwordHash, out byte[] passwordSalt);

                int newUserId;

                // Insert into Users
                using (var userCmd = new SqlCommand(@"
            INSERT INTO Users
            (
                FullName,
                Email,
                PasswordHash,
                PasswordSalt,
                Role,
                IsMfaEnabled,
                CreatedAt,
                TotpEnabled,
                TotpSecretBase32,
                TotpIssuer,
                LoginUserId,
                MustChangePassword
            )
            OUTPUT INSERTED.UserId
            VALUES
            (
                @FullName,
                @Email,
                @PasswordHash,
                @PasswordSalt,
                @Role,
                @IsMfaEnabled,
                SYSDATETIME(),
                @TotpEnabled,
                @TotpSecretBase32,
                @TotpIssuer,
                @LoginUserId,
                @MustChangePassword
            )
        ", conn, tx))
                {
                    userCmd.Parameters.Add("@FullName", SqlDbType.NVarChar, 100).Value = req.FullName.Trim();
                    userCmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = req.Email.Trim();
                    userCmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 256).Value = passwordHash;
                    userCmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 256).Value = passwordSalt;
                    userCmd.Parameters.Add("@Role", SqlDbType.NVarChar, 20).Value = "Voter";
                    userCmd.Parameters.Add("@IsMfaEnabled", SqlDbType.Bit).Value = true;
                    userCmd.Parameters.Add("@TotpEnabled", SqlDbType.Bit).Value = false;
                    userCmd.Parameters.Add("@TotpSecretBase32", SqlDbType.NVarChar, 128).Value = DBNull.Value;
                    userCmd.Parameters.Add("@TotpIssuer", SqlDbType.NVarChar, 100).Value = DBNull.Value;
                    userCmd.Parameters.Add("@LoginUserId", SqlDbType.NVarChar, 30).Value = loginUserId;
                    userCmd.Parameters.Add("@MustChangePassword", SqlDbType.Bit).Value = true;
                  

                    newUserId = Convert.ToInt32(userCmd.ExecuteScalar());
                }

                string maskedIdNumber = MaskIdNumber(req.IdDocumentNumber);

                // Insert into VoterVerification
                using (var verificationCmd = new SqlCommand(@"
            INSERT INTO VoterVerifications
            (
                UserId,
                LegalFullName,
                DateOfBirth,
                AddressLine1,
                AddressLine2,
                City,
                StateCode,
                ZipCode,
                JurisdictionId,
                PhoneNumber,
                IdDocumentType,
                IdDocumentNumberMasked,
                IdDocumentState,
                VerificationStatus,
                IsEligibleToVote,
                SubmittedAtUTC,
                ReviewedAtUTC,
                ReviewedByUserId,
                ReviewerNotes,
                IdDocumentImageFileName,
                IdDocumentImagePath
            )
            VALUES
            (
                @UserId,
                @LegalFullName,
                @DateOfBirth,
                @AddressLine1,
                @AddressLine2,
                @City,
                @StateCode,
                @ZipCode,
                @JurisdictionId,
                @PhoneNumber,
                @IdDocumentType,
                @IdDocumentNumberMasked,
                @IdDocumentState,
                @VerificationStatus,
                @IsEligibleToVote,
                SYSDATETIME(),
                NULL,
                NULL,
                NULL,
                @IdPictureFileName,
                @IdPicturePath
            )
        ", conn, tx))
                {
                    verificationCmd.Parameters.Add("@UserId", SqlDbType.Int).Value = newUserId;
                    verificationCmd.Parameters.Add("@LegalFullName", SqlDbType.NVarChar, 150).Value = req.FullName.Trim();
                    verificationCmd.Parameters.Add("@DateOfBirth", SqlDbType.Date).Value =(object?)req.DOB ?? DBNull.Value;
                    verificationCmd.Parameters.Add("@AddressLine1", SqlDbType.NVarChar, 200).Value = req.AddressLine1.Trim();
                    verificationCmd.Parameters.Add("@AddressLine2", SqlDbType.NVarChar, 200).Value =
                        string.IsNullOrWhiteSpace(req.AddressLine2) ? DBNull.Value : req.AddressLine2.Trim();
                    verificationCmd.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = req.City.Trim();
                    verificationCmd.Parameters.Add("@StateCode", SqlDbType.NVarChar, 20).Value = req.StateCode.Trim();
                    verificationCmd.Parameters.Add("@ZipCode", SqlDbType.NVarChar, 20).Value = req.ZipCode.Trim();
                    verificationCmd.Parameters.Add("@JurisdictionId", SqlDbType.Int).Value =
                        (object?)req.JurisdictionId ?? DBNull.Value;
                    verificationCmd.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar, 30).Value = req.PhoneNumber.Trim();
                    verificationCmd.Parameters.Add("@IdDocumentType", SqlDbType.NVarChar, 50).Value = req.IdDocumentType.Trim();
                    verificationCmd.Parameters.Add("@IdDocumentNumberMasked", SqlDbType.NVarChar, 50).Value = maskedIdNumber;
                    verificationCmd.Parameters.Add("@IdDocumentState", SqlDbType.NVarChar, 20).Value =
                        string.IsNullOrWhiteSpace(req.IdDocumentState) ? DBNull.Value : req.IdDocumentState.Trim();
                    verificationCmd.Parameters.Add("@VerificationStatus", SqlDbType.NVarChar, 20).Value = "Pending";
                    verificationCmd.Parameters.Add("@IsEligibleToVote", SqlDbType.Bit).Value = false;

                    verificationCmd.Parameters.Add("@IdPictureFileName", SqlDbType.NVarChar, 255).Value = idPictureFileName;
                    verificationCmd.Parameters.Add("@IdPicturePath", SqlDbType.NVarChar, 500).Value = idPicturePath;

                    verificationCmd.ExecuteNonQuery();
                }

                tx.Commit();

                return new RegisterVoterResult
                {
                    Success = true,
                    Message = "Voter registered successfully. Verification is pending admin review.",
                    LoginUserId = loginUserId,
                    TemporaryPassword = tempPassword
                };
            }
            catch (Exception ex)
            {
                tx.Rollback();

                return new RegisterVoterResult
                {
                    Success = false,
                    Message = "Registration failed: " + ex.Message
                };
            }
        }



        public User? GetByEmailOrLoginId(string loginId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Users_GetByEmailOrLoginId", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@LoginId", SqlDbType.NVarChar, 150).Value = loginId;

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new User
            {
                UserId = r.GetInt32(0),
                FullName = r.GetString(1),
                Email = r.GetString(2),
                PasswordHash = (byte[])r[3],
                PasswordSalt = (byte[])r[4],
                Role = r.GetString(5),
                IsMfaEnabled = r.GetBoolean(6),
                TotpEnabled = r.GetBoolean(7),
                TotpSecretBase32 = r.IsDBNull(8) ? null : r.GetString(8),
                TotpIssuer = r.IsDBNull(9) ? null : r.GetString(9),
                MustChangePassword = r.GetBoolean(11)
            };
        }



        private static string GenerateLoginUserId()
        {
            return "VOT-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
        }

        private static string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789@#";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }

        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static string MaskIdNumber(string idNumber)
        {
            if (string.IsNullOrWhiteSpace(idNumber))
                return string.Empty;

            string trimmed = idNumber.Trim();

            if (trimmed.Length <= 4)
                return new string('*', trimmed.Length);

            return new string('*', trimmed.Length - 4) + trimmed[^4..];
        }


        public void UpdatePasswordAndClearMustChange(int userId, byte[] passwordHash, byte[] passwordSalt)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        UPDATE Users
        SET PasswordHash = @PasswordHash,
            PasswordSalt = @PasswordSalt,
            MustChangePassword = 0
        WHERE UserId = @UserId
    ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.VarBinary, 256).Value = passwordHash;
            cmd.Parameters.Add("@PasswordSalt", SqlDbType.VarBinary, 256).Value = passwordSalt;

            conn.Open();
            cmd.ExecuteNonQuery();
        }







    }
}
