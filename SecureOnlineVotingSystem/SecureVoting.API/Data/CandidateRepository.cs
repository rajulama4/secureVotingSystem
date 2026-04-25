using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class CandidateRepository
    {

        private readonly DbHelper _db;
        public CandidateRepository(DbHelper db) => _db = db;
        public int CreateCandidate(CreateCandidateRequest model, string? imageFileName, string? imagePath)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                            INSERT INTO dbo.Candidates
                            (
                                ElectionId,
                                CandidateName,
                                Party,
                                Bio,
                                ISActive,
                                CandidateImageFileName,
                                CandidateImagePath
                            )
                            OUTPUT INSERTED.CandidateId
                            VALUES
                            (
                                @ElectionId,
                                @CandidateName,
                                @Party,
                                @Bio,
                                '1',
                                 @CandidateImageFileName,
                                 @CandidateImagePath   
                            )
                        ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = model.ElectionId;
            cmd.Parameters.Add("@CandidateName", SqlDbType.NVarChar, 150).Value = model.CandidateName;
            cmd.Parameters.Add("@Party", SqlDbType.NVarChar, 100).Value =
                (object?)model.Party ?? DBNull.Value;
            cmd.Parameters.Add("@Bio", SqlDbType.NVarChar, 500).Value =
                (object?)model.Bio ?? DBNull.Value;
            cmd.Parameters.Add("@CandidateImageFileName", SqlDbType.NVarChar, 260).Value =
                (object?)imageFileName ?? DBNull.Value;
            cmd.Parameters.Add("@CandidateImagePath", SqlDbType.NVarChar, 300).Value =
                (object?)imagePath ?? DBNull.Value;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }




        public List<Candidate> GetCandidatesByElectionId(int electionId)
        {
            var list = new List<Candidate>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT CandidateId, ElectionId, CandidateName, Party, Bio, CreatedAt, ISActive,
               CandidateImageFileName, CandidateImagePath
        FROM dbo.Candidates
        WHERE ElectionId = @ElectionId
        ORDER BY CandidateId
    ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new Candidate
                {
                    CandidateId = Convert.ToInt32(r["CandidateId"]),
                    ElectionId = Convert.ToInt32(r["ElectionId"]),
                    CandidateName = r["CandidateName"]?.ToString() ?? string.Empty,
                    Party = r["Party"] == DBNull.Value ? null : r["Party"]?.ToString(),
                    Bio = r["Bio"] == DBNull.Value ? null : r["Bio"]?.ToString(),
                    CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                    IsActive = r["ISActive"] != DBNull.Value && r["ISActive"]!.ToString() == "1",
                    CandidateImageFileName = r["CandidateImageFileName"] == DBNull.Value ? null : r["CandidateImageFileName"]?.ToString(),
                    CandidateImagePath = r["CandidateImagePath"] == DBNull.Value ? null : r["CandidateImagePath"]?.ToString()
                });
            }

            return list;
        }




        public bool DeactivateCandidate(int candidateId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        UPDATE dbo.Candidates
        SET ISActive = '0'
        WHERE CandidateId = @CandidateId
    ", conn);

            cmd.Parameters.Add("@CandidateId", SqlDbType.Int).Value = candidateId;

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }


        public Candidate? GetById(int candidateId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT CandidateId, ElectionId, CandidateName, Party, Bio, CreatedAt, ISActive
        FROM dbo.Candidates
        WHERE CandidateId = @CandidateId
    ", conn);

            cmd.Parameters.Add("@CandidateId", SqlDbType.Int).Value = candidateId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            if (!r.Read()) return null;

            return new Candidate
            {
                CandidateId = Convert.ToInt32(r["CandidateId"]),
                ElectionId = Convert.ToInt32(r["ElectionId"]),
                CandidateName = r["CandidateName"]?.ToString() ?? string.Empty,
                Party = r["Party"] == DBNull.Value ? null : r["Party"]?.ToString(),
                Bio = r["Bio"] == DBNull.Value ? null : r["Bio"]?.ToString(),
                CreatedAt = Convert.ToDateTime(r["CreatedAt"]),
                IsActive = r["ISActive"] != DBNull.Value &&
                           r["ISActive"]!.ToString() == "1"
            };
        }



        public bool UpdateCandidate(UpdateCandidateRequest model, string? imageFileName, string? imagePath, bool replaceImage)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        UPDATE dbo.Candidates
        SET
            CandidateName = @CandidateName,
            Party = @Party,
            Bio = @Bio,
            CandidateImageFileName = CASE WHEN @ReplaceImage = 1 THEN @CandidateImageFileName ELSE CandidateImageFileName END,
            CandidateImagePath = CASE WHEN @ReplaceImage = 1 THEN @CandidateImagePath ELSE CandidateImagePath END
        WHERE CandidateId = @CandidateId
    ", conn);

            cmd.Parameters.Add("@CandidateId", SqlDbType.Int).Value = model.CandidateId;
            cmd.Parameters.Add("@CandidateName", SqlDbType.NVarChar, 150).Value = model.CandidateName;
            cmd.Parameters.Add("@Party", SqlDbType.NVarChar, 100).Value = (object?)model.Party ?? DBNull.Value;
            cmd.Parameters.Add("@Bio", SqlDbType.NVarChar, 500).Value = (object?)model.Bio ?? DBNull.Value;
            cmd.Parameters.Add("@ReplaceImage", SqlDbType.Bit).Value = replaceImage;
            cmd.Parameters.Add("@CandidateImageFileName", SqlDbType.NVarChar, 260).Value = (object?)imageFileName ?? DBNull.Value;
            cmd.Parameters.Add("@CandidateImagePath", SqlDbType.NVarChar, 300).Value = (object?)imagePath ?? DBNull.Value;

            conn.Open();
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
