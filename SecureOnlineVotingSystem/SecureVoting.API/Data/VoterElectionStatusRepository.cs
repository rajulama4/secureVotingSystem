using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class VoterElectionStatusRepository
    {

        private readonly DbHelper _db;
        public VoterElectionStatusRepository(DbHelper db) => _db = db;

        public bool HasVoted(int electionId, int voterId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT COUNT(1)
                FROM VoterElectionStatus
                WHERE ElectionId = @ElectionId AND VoterId = @VoterId
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;
            cmd.Parameters.Add("@VoterId", SqlDbType.Int).Value = voterId;

            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }

        public void MarkVoted(int electionId, int voterId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO VoterElectionStatus (ElectionId, VoterId, HasVoted)
                VALUES (@ElectionId, @VoterId, 1);
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;
            cmd.Parameters.Add("@VoterId", SqlDbType.Int).Value = voterId;

            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
