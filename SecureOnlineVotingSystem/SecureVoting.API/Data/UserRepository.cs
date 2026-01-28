using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;


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
                @"SELECT UserId, FullName, Email, PasswordHash, PasswordSalt, Role, IsMfaEnabled
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
                IsMfaEnabled = r.GetBoolean(6)
            };
        }

        public int Create(string fullName, string email, byte[] passwordHash, byte[] passwordSalt, string role)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Users (FullName, Email, PasswordHash, PasswordSalt, Role, IsMfaEnabled)
                VALUES (@FullName, @Email, @PasswordHash, @PasswordSalt, @Role, 1);
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




    }
}
