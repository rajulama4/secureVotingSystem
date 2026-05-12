using SecureVotingSystem.Data;
using SecureVotingSystem.Models;

namespace SecureVotingSystem.Services
{
    public class ReportsService
    {
        private readonly ReportsRepository _reportsRepository;

        public ReportsService(ReportsRepository reportsRepository)
        {
            _reportsRepository = reportsRepository;
        }

        public AdminReportsOverviewDto GetOverview()
        {
            return _reportsRepository.GetOverview();
        }

        public List<VoterVerificationStatusReportDto> GetVotersByVerificationStatus()
        {
            return _reportsRepository.GetVotersByVerificationStatus();
        }

        public List<ElectionTurnoutReportDto> GetElectionTurnout()
        {
            return _reportsRepository.GetElectionTurnout();
        }

        public List<CandidatePerformanceReportDto> GetCandidatePerformance()
        {
            return _reportsRepository.GetCandidatePerformance();
        }
    }
}