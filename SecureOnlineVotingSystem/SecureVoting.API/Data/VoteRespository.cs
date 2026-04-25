using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class VoteRepository
    {
        private readonly DbHelper _db;
        public VoteRepository(DbHelper db) => _db = db;

        public List<Election> GetOpenElections()
        {
            var list = new List<Election>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Election_GetOpenForVoting", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Election
                {
                    ElectionId = r.GetInt32(r.GetOrdinal("ElectionId")),
                    Title = r.GetString(r.GetOrdinal("Title")),
                    Description = r.IsDBNull(r.GetOrdinal("Description")) ? null : r.GetString(r.GetOrdinal("Description")),
                    StartTime = r.GetDateTime(r.GetOrdinal("StartTime")),
                    EndTime = r.GetDateTime(r.GetOrdinal("EndTime")),
                    IsClosed = r.GetBoolean(r.GetOrdinal("IsClosed")),
                    IsPublished = r.GetBoolean(r.GetOrdinal("IsPublished")),
                    CreatedBy = r.IsDBNull(r.GetOrdinal("CreatedBy")) ? null : r.GetInt32(r.GetOrdinal("CreatedBy")),
                    CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
                });
            }

            return list;
        }

        public List<Candidate> GetCandidatesByElection(int electionId)
        {
            var list = new List<Candidate>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Candidate_GetByElection", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Candidate
                {
                    CandidateId = r.GetInt32(r.GetOrdinal("CandidateId")),
                    ElectionId = r.GetInt32(r.GetOrdinal("ElectionId")),
                    CandidateName = r.GetString(r.GetOrdinal("CandidateName")),
                    Party = r.IsDBNull(r.GetOrdinal("Party")) ? null : r.GetString(r.GetOrdinal("Party")),
                    Bio = r.IsDBNull(r.GetOrdinal("Bio")) ? null : r.GetString(r.GetOrdinal("Bio")),
                    CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
                });
            }

            return list;
        }

        public int CastVote(int electionId, int candidateId, int voterUserId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Vote_Cast", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;
            cmd.Parameters.Add("@CandidateId", SqlDbType.Int).Value = candidateId;
            cmd.Parameters.Add("@VoterUserId", SqlDbType.Int).Value = voterUserId;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<VoteResult> GetPublishedResults(int electionId)
        {
            var list = new List<VoteResult>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Election_GetPublishedResults", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new VoteResult
                {
                    CandidateId = r.GetInt32(r.GetOrdinal("CandidateId")),
                    CandidateName = r.GetString(r.GetOrdinal("CandidateName")),
                    Party = r.IsDBNull(r.GetOrdinal("Party")) ? null : r.GetString(r.GetOrdinal("Party")),
                    VoteCount = r.GetInt32(r.GetOrdinal("VoteCount"))
                });
            }

            return list;
        }


    }
}