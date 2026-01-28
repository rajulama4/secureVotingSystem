using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class MfaRepository
    {

        private readonly DbHelper _db;
        public MfaRepository(DbHelper db) => _db = db;

        public int CreateChallenge(int userId, byte[] codeHash, DateTime expiresAt)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO MfaChallenges (UserId, CodeHash, ExpiresAt)
                VALUES (@UserId, @CodeHash, @ExpiresAt);
                SELECT SCOPE_IDENTITY();
            ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@CodeHash", SqlDbType.VarBinary, 64).Value = codeHash;
            cmd.Parameters.Add("@ExpiresAt", SqlDbType.DateTime2).Value = expiresAt;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool VerifyAndConsume(int challengeId, int userId, byte[] submittedHash)
        {
            using var conn = _db.GetConnection();
            conn.Open();

            // 1) read
            using var select = new SqlCommand(@"
                SELECT CodeHash, ExpiresAt, IsUsed
                FROM MfaChallenges
                WHERE ChallengeId = @ChallengeId AND UserId = @UserId
            ", conn);

            select.Parameters.Add("@ChallengeId", SqlDbType.Int).Value = challengeId;
            select.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            byte[]? storedHash = null;
            DateTime expiresAt = default;
            bool isUsed = true;

            using (var r = select.ExecuteReader())
            {
                if (!r.Read()) return false;
                storedHash = (byte[])r[0];
                expiresAt = r.GetDateTime(1);
                isUsed = r.GetBoolean(2);
            }

            if (isUsed) return false;
            if (DateTime.UtcNow > expiresAt.ToUniversalTime()) return false;

            if (!storedHash.SequenceEqual(submittedHash)) return false;

            // 2) consume (one-time)
            using var update = new SqlCommand(@"
                UPDATE MfaChallenges SET IsUsed = 1
                WHERE ChallengeId = @ChallengeId AND UserId = @UserId AND IsUsed = 0
            ", conn);

            update.Parameters.Add("@ChallengeId", SqlDbType.Int).Value = challengeId;
            update.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            return update.ExecuteNonQuery() == 1;
        }
    }
}
