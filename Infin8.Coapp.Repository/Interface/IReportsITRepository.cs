using Infin8.Coapp.Dto;

namespace Infin8.Coapp.Repository
{
    /// <summary>Income tax reports (109 loans, 110 share capital, 111 members, 112 fixed deposits), ported from TSIS GetITReport*.</summary>
    public interface IReportsITRepository
    {
        Task<List<rptITLoan>> GetITLoansAsync(int loanType, DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptITShareCapital>> GetITShareCapitalAsync(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptITMember>> GetITMembersAsync(DateTime asOnDate, string brCode);
        Task<List<rptITFixedDeposit>> GetITFixedDepositsAsync(DateTime fromDate, DateTime toDate, string brCode);
    }
}
