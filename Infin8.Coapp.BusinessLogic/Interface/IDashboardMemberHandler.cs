using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IDashboardMemberHandler
    {
        //List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts(decimal id, DateTime fromDate, DateTime toDate);
        List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate, string brCode);
        Task<List<MemberDashBoardLoans>> GetMemberDashBoardLoans(decimal memId, DateTime fromDate, DateTime toDate,string brCode);
        Task<List<MemberDashBoardTD>> GetMemberDashBoardTDs(decimal memId, DateTime fromDate, DateTime toDate, string brCode);
        Task<MemberDetailsVM2> GetMemberDataDashBoard(decimal memId, string brCode);
        Task<List<MemberLedgerVM>> GetMemberLedger(decimal MemId, int TrnType, DateTime FromDate, DateTime ToDate, string brCode);
        Task<LoanLedgerVM> GetLoanLedger(decimal loanId, DateTime fromDate, DateTime toDate, string brCode);
        Task<TDLedgerVM> GetTDLedgerVM(decimal TDId, DateTime FromDate, DateTime ToDate, string brCode);
    }
}
