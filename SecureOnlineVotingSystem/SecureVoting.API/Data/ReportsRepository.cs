using SecureVoting.API.Data;
using SecureVotingSystem.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVotingSystem.Data
{
    public class ReportsRepository
    {
        private readonly DbHelper _dbHelper;

        public ReportsRepository(DbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public AdminReportsOverviewDto GetOverview()
        {
            var report = new AdminReportsOverviewDto();

            using var connection = _dbHelper.GetConnection();
            using var command = new SqlCommand("sp_Reports_GetOverview", connection);
            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                report.TotalVoters = Convert.ToInt32(reader["TotalVoters"]);
                report.VerifiedVoters = Convert.ToInt32(reader["VerifiedVoters"]);
                report.PendingVerifications = Convert.ToInt32(reader["PendingVerifications"]);
                report.RejectedVerifications = Convert.ToInt32(reader["RejectedVerifications"]);
                report.TotalElections = Convert.ToInt32(reader["TotalElections"]);
                report.PublishedElections = Convert.ToInt32(reader["PublishedElections"]);
                report.ClosedElections = Convert.ToInt32(reader["ClosedElections"]);
                report.TotalVotesCast = Convert.ToInt32(reader["TotalVotesCast"]);
                report.TotalCandidates = Convert.ToInt32(reader["TotalCandidates"]);
            }

            return report;
        }

        public List<VoterVerificationStatusReportDto> GetVotersByVerificationStatus()
        {
            var list = new List<VoterVerificationStatusReportDto>();

            using var connection = _dbHelper.GetConnection();
            using var command = new SqlCommand("sp_Reports_GetVotersByVerificationStatus", connection);
            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new VoterVerificationStatusReportDto
                {
                    VerificationStatus = reader["VerificationStatus"].ToString() ?? "",
                    TotalCount = Convert.ToInt32(reader["TotalCount"])
                });
            }

            return list;
        }

        public List<ElectionTurnoutReportDto> GetElectionTurnout()
        {
            var list = new List<ElectionTurnoutReportDto>();

            using var connection = _dbHelper.GetConnection();
            using var command = new SqlCommand("sp_Reports_GetElectionTurnout", connection);
            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new ElectionTurnoutReportDto
                {
                    ElectionId = Convert.ToInt32(reader["ElectionId"]),
                    ElectionTitle = reader["ElectionTitle"].ToString() ?? "",
                    TotalEligibleVoters = Convert.ToInt32(reader["TotalEligibleVoters"]),
                    TotalVotesCast = Convert.ToInt32(reader["TotalVotesCast"]),
                    TurnoutPercentage = Convert.ToDecimal(reader["TurnoutPercentage"])
                });
            }

            return list;
        }

        public List<CandidatePerformanceReportDto> GetCandidatePerformance()
        {
            var list = new List<CandidatePerformanceReportDto>();

            using var connection = _dbHelper.GetConnection();
            using var command = new SqlCommand("sp_Reports_GetCandidatePerformance", connection);
            command.CommandType = CommandType.StoredProcedure;

            connection.Open();

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CandidatePerformanceReportDto
                {
                    ElectionId = Convert.ToInt32(reader["ElectionId"]),
                    ElectionTitle = reader["ElectionTitle"].ToString() ?? "",
                    CandidateId = Convert.ToInt32(reader["CandidateId"]),
                    CandidateName = reader["CandidateName"].ToString() ?? "",
                    Party = reader["Party"] == DBNull.Value ? null : reader["Party"].ToString(),
                    VoteCount = Convert.ToInt32(reader["VoteCount"]),
                    VotePercentage = Convert.ToDecimal(reader["VotePercentage"])
                });
            }

            return list;
        }
    }
}