
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class UserSessionRepository
    {
        private readonly DbHelper _db;

        public UserSessionRepository(DbHelper db)
        {
            _db = db;
        }

        public void DeactivateUserSessions(int userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.UserSessions
                SET IsActive = 0
                WHERE UserId = @UserId AND IsActive = 1
            ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void CreateSession(
            Guid sessionId,
            int userId,
            DateTime expiresAtUtc,
            string? deviceInfo,
            string? ipAddress)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO dbo.UserSessions
                (
                    SessionId,
                    UserId,
                    CreatedAtUtc,
                    ExpiresAtUtc,
                    IsActive,
                    DeviceInfo,
                    IpAddress
                )
                VALUES
                (
                    @SessionId,
                    @UserId,
                    SYSUTCDATETIME(),
                    @ExpiresAtUtc,
                    1,
                    @DeviceInfo,
                    @IpAddress
                )
            ", conn);

            cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = sessionId;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@ExpiresAtUtc", SqlDbType.DateTime2).Value = expiresAtUtc;
            cmd.Parameters.Add("@DeviceInfo", SqlDbType.NVarChar, 500).Value =
                string.IsNullOrWhiteSpace(deviceInfo) ? DBNull.Value : deviceInfo;
            cmd.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(ipAddress) ? DBNull.Value : ipAddress;

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public bool IsSessionActive(int userId, Guid sessionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1)
                FROM dbo.UserSessions
                WHERE UserId = @UserId
                  AND SessionId = @SessionId
                  AND IsActive = 1
                  AND ExpiresAtUtc > SYSUTCDATETIME()
            ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = sessionId;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void DeactivateSession(int userId, Guid sessionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE dbo.UserSessions
                SET IsActive = 0
                WHERE UserId = @UserId AND SessionId = @SessionId
            ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;
            cmd.Parameters.Add("@SessionId", SqlDbType.UniqueIdentifier).Value = sessionId;

            conn.Open();
            cmd.ExecuteNonQuery();
        }


        public bool HasActiveSession(int userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT COUNT(1)
        FROM dbo.UserSessions
        WHERE UserId = @UserId
          AND IsActive = 1
          AND ExpiresAtUtc > SYSUTCDATETIME()
    ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }
}