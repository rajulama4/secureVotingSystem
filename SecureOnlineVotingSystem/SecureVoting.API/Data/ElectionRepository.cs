using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class ElectionRepository
    {

        private readonly DbHelper _db;
        public ElectionRepository(DbHelper db) => _db = db;

        public int Create(Election election, int adminUserId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO Elections (Title, Description, StartTime, EndTime, CreatedBy)
                VALUES (@Title, @Description, @StartTime, @EndTime, @CreatedBy);
                SELECT SCOPE_IDENTITY();
            ", conn);

            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = election.Title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = election.Description ?? "";
            cmd.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = election.StartTime;
            cmd.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = election.EndTime;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = adminUserId;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<Election> GetAll()
        {
            var list = new List<Election>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT ElectionId, Title, Description, StartTime, EndTime, IsClosed
                FROM Elections
                ORDER BY CreatedAt DESC
            ", conn);

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Election
                {
                    ElectionId = r.GetInt32(0),
                    Title = r.GetString(1),
                    Description = r.GetString(2),
                    StartTime = r.GetDateTime(3),
                    EndTime = r.GetDateTime(4),
                    IsClosed = r.GetBoolean(5)
                });
            }

            return list;
        }

        public bool CloseElection(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                UPDATE Elections
                SET IsClosed = 1
                WHERE ElectionId = @ElectionId AND IsClosed = 0
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            return cmd.ExecuteNonQuery() == 1;
        }

        public Election? GetById(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT ElectionId, Title, Description, StartTime, EndTime, IsClosed
                FROM Elections WHERE ElectionId = @ElectionId
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            if (!r.Read()) return null;

            return new Election
            {
                ElectionId = r.GetInt32(0),
                Title = r.GetString(1),
                Description = r.GetString(2),
                StartTime = r.GetDateTime(3),
                EndTime = r.GetDateTime(4),
                IsClosed = r.GetBoolean(5)
            };
        }
    }
}
