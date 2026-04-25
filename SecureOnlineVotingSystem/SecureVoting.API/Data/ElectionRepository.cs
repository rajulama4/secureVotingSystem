using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class ElectionRepository
    {

        private readonly DbHelper _db;
        public ElectionRepository(DbHelper db) => _db = db;

        public int Create(Election model, int adminId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        INSERT INTO Elections
        (
            Title,
            Description,
            StartTime,
            EndTime,
            IsClosed,
            IsPublished,
            CreatedBy,
            CreatedAt,
            JurisdictionId
        )
        OUTPUT INSERTED.ElectionId
        VALUES
        (
            @Title,
            @Description,
            @StartTime,
            @EndTime,
            0,
            0,
            @CreatedBy,
            SYSUTCDATETIME(),
            @JurisdictionId
        )
    ", conn);

            cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = model.Title;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value =
                (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@StartTime", SqlDbType.DateTime2).Value = model.StartTime;
            cmd.Parameters.Add("@EndTime", SqlDbType.DateTime2).Value = model.EndTime;
            cmd.Parameters.Add("@CreatedBy", SqlDbType.Int).Value = adminId;
            cmd.Parameters.Add("@JurisdictionId", SqlDbType.Int).Value =
                (object?)model.JurisdictionId ?? DBNull.Value;

            conn.Open();
            return (int)cmd.ExecuteScalar()!;
        }




        public List<Election> GetAll()
        {
            var list = new List<Election>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Election_GetAll", conn)
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
                    JurisdictionId = r.GetInt32(r.GetOrdinal("JurisdictionId")),
                    CreatedBy = r.IsDBNull(r.GetOrdinal("CreatedBy")) ? null : r.GetInt32(r.GetOrdinal("CreatedBy")),
                    CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt"))
                });
            }

            return list;
        }

        public bool CloseElection(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Election_Close", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            var rows = Convert.ToInt32(cmd.ExecuteScalar());
            return rows > 0;
        }



        public bool PublishResults(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Election_PublishResults", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            var rows = Convert.ToInt32(cmd.ExecuteScalar());
            return rows > 0;
        }

        public Election? GetById(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT ElectionId, Title, Description, StartTime, EndTime, IsClosed,IsPublished,CreatedBy,CreatedAt
                FROM Elections WHERE ElectionId = @ElectionId
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            if (!r.Read()) return null;

            return new Election
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
            };
        }



        public List<Candidate> GetCandidatesByElectionId(int electionId)
        {
            var list = new List<Candidate>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT CandidateId, ElectionId, CandidateName, Party, Bio, IsActive
        FROM Candidates
        WHERE ElectionId = @ElectionId
        ORDER BY CandidateId
    ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                bool isActive = Convert.ToInt32(r["IsActive"]) == 1;

                if (!isActive)
                    continue;

                list.Add(new Candidate
                {
                    CandidateId = Convert.ToInt32(r["CandidateId"]),
                    ElectionId = Convert.ToInt32(r["ElectionId"]),
                    CandidateName = r["CandidateName"]?.ToString() ?? string.Empty,
                    Party = r["Party"] == DBNull.Value ? null : r["Party"]?.ToString(),
                    Bio = r["Bio"] == DBNull.Value ? null : r["Bio"]?.ToString(),
                    IsActive = isActive
                });
            }

            return list;
        }




        public int? GetJurisdictionId(int userId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1
                    JurisdictionId
                FROM Users u
                LEFT JOIN VoterVerifications v ON v.UserId = u.UserId
                WHERE u.UserId = @UserId
            ", conn);

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            conn.Open();
            var result = cmd.ExecuteScalar();

            if (result == null || result == DBNull.Value)
                return null;

            return Convert.ToInt32(result);
        }


        public List<Election> GetByJurisdiction(int jurisdictionId)
        {
            var list = new List<Election>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_Election_GetByJurisdiction", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@JurisdictionId", SqlDbType.Int).Value = jurisdictionId;

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
                    CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                    JurisdictionId = r.IsDBNull(r.GetOrdinal("JurisdictionId")) ? null : r.GetInt32(r.GetOrdinal("JurisdictionId"))
                });
            }

            return list;
        }


        public List<VoteCountStatusDto> GetVoteCountStatus()
        {
            var list = new List<VoteCountStatusDto>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT
            e.ElectionId,
            e.Title,
            e.JurisdictionId,
            j.JurisdictionName,
            e.StartTime,
            e.EndTime,
            e.IsClosed,
            e.IsPublished,
            COUNT(CASE WHEN ves.HasVoted = 1 THEN 1 END) AS TotalVotes
        FROM dbo.Elections e
        LEFT JOIN dbo.Jurisdictions j
            ON e.JurisdictionId = j.JurisdictionId
        LEFT JOIN dbo.VoterElectionStatus ves
            ON e.ElectionId = ves.ElectionId
        GROUP BY
            e.ElectionId,
            e.Title,
            e.JurisdictionId,
            j.JurisdictionName,
            e.StartTime,
            e.EndTime,
            e.IsClosed,
            e.IsPublished
        ORDER BY e.StartTime DESC
    ", conn);

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                var startTime = r.GetDateTime(r.GetOrdinal("StartTime"));
                var endTime = r.GetDateTime(r.GetOrdinal("EndTime"));
                var isClosed = r.GetBoolean(r.GetOrdinal("IsClosed"));
                var isPublished = r.GetBoolean(r.GetOrdinal("IsPublished"));
                var now = DateTime.UtcNow;

                string countStatus;
                if (isPublished)
                    countStatus = "Published";
                else if (isClosed)
                    countStatus = "Counting";
                else if (now < startTime.ToUniversalTime())
                    countStatus = "Upcoming";
                else if (now >= startTime.ToUniversalTime() && now <= endTime.ToUniversalTime())
                    countStatus = "Active";
                else
                    countStatus = "Ended";

                list.Add(new VoteCountStatusDto
                {
                    ElectionId = r.GetInt32(r.GetOrdinal("ElectionId")),
                    Title = r.GetString(r.GetOrdinal("Title")),
                    JurisdictionId = r.IsDBNull(r.GetOrdinal("JurisdictionId"))
                        ? null
                        : r.GetInt32(r.GetOrdinal("JurisdictionId")),
                    JurisdictionName = r.IsDBNull(r.GetOrdinal("JurisdictionName"))
                        ? null
                        : r.GetString(r.GetOrdinal("JurisdictionName")),
                    StartTime = startTime,
                    EndTime = endTime,
                    IsClosed = isClosed,
                    IsPublished = isPublished,
                    TotalVotes = Convert.ToInt32(r["TotalVotes"]),
                    CountStatus = countStatus
                });
            }

            return list;
        }




        public List<ElectionResultDetailDto> GetElectionResultDetails(int electionId)
        {
            var list = new List<ElectionResultDetailDto>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT
            c.CandidateId,
            c.CandidateName,
            c.Party,
            c.CandidateImagePath,
            ISNULL(r.VoteCount, 0) AS VoteCount
        FROM dbo.Candidates c
        LEFT JOIN dbo.ElectionCandidateResults r
            ON r.ElectionId = c.ElectionId
           AND r.CandidateId = c.CandidateId
        WHERE c.ElectionId = @ElectionId
          AND (c.ISActive IS NULL OR c.ISActive = '1')
        ORDER BY ISNULL(r.VoteCount, 0) DESC, c.CandidateName ASC
    ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new ElectionResultDetailDto
                {
                    CandidateId = Convert.ToInt32(r["CandidateId"]),
                    CandidateName = r["CandidateName"]?.ToString() ?? string.Empty,
                    Party = r["Party"] == DBNull.Value ? null : r["Party"]?.ToString(),
                    CandidateImagePath = r["CandidateImagePath"] == DBNull.Value ? null : r["CandidateImagePath"]?.ToString(),
                    VoteCount = Convert.ToInt32(r["VoteCount"]),
                    IsWinner = false
                });
            }

            if (list.Count > 0)
            {
                var maxVotes = list.Max(x => x.VoteCount);
                if (maxVotes > 0)
                {
                    foreach (var item in list)
                    {
                        item.IsWinner = item.VoteCount == maxVotes;
                    }
                }
            }

            return list;
        }


        public void EnsureCandidateResultRows(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        INSERT INTO dbo.ElectionCandidateResults (ElectionId, CandidateId, VoteCount, LastUpdatedAt)
        SELECT c.ElectionId, c.CandidateId, 0, SYSUTCDATETIME()
        FROM dbo.Candidates c
        WHERE c.ElectionId = @ElectionId
          AND NOT EXISTS (
              SELECT 1
              FROM dbo.ElectionCandidateResults r
              WHERE r.ElectionId = c.ElectionId
                AND r.CandidateId = c.CandidateId
          )
    ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            cmd.ExecuteNonQuery();
        }


        public void IncrementCandidateVoteCount(int electionId, int candidateId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        UPDATE dbo.ElectionCandidateResults
        SET VoteCount = VoteCount + 1,
            LastUpdatedAt = SYSUTCDATETIME()
        WHERE ElectionId = @ElectionId
          AND CandidateId = @CandidateId
    ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;
            cmd.Parameters.Add("@CandidateId", SqlDbType.Int).Value = candidateId;

            conn.Open();
            cmd.ExecuteNonQuery();
        }



    }
}
