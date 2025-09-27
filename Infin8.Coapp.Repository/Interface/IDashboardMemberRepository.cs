using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;

namespace Infin8.Coapp.Repository
{
    public interface IDashboardMemberRepository
    {
        List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts_old(decimal id, DateTime fromDate, DateTime toDate);
        List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate,string brCode);
        Task<List<MemberDashBoardLoans>> GetMemberDashBoardLoans(decimal memId, DateTime fromDate, DateTime toDate,string brCode);
        Task<List<MemberDashBoardTD>> GetMemberDashBoardTDs(decimal memId, DateTime fromDate, DateTime toDate,string brCode);
        Task<MemberDetailsVM2> GetMemberDataDashBoard(decimal memId, string brCode);
        Task<List<MemberLedgerVM>> GetMemberLedger(decimal MemId, int TrnType, DateTime FromDate, DateTime ToDate,string brCode);

        Task<LoanLedgerVM> GetLoanLedger(decimal loanId, DateTime fromDate, DateTime toDate, string brCode);

        Task<LoanHeaderDetailsVM> GetLoanHeader(decimal LoanId, string brCode);
        Task<List<Loan_Roi>> GetLoanLedgerROIAndPI(decimal loanid, string brCode);
        Task<List<Loan_Inst>> GetLoanLedgerInstalment(decimal loanid, string brCode);
        Task<LoanTransactionVM> GetLoanOBOnLoanId(decimal LoanId, DateTime FromDate, string brCode);
        Task<List<LoanTransactionVM>> GetLoanDetailsBetweenDatesOnLoanId(decimal LoanId, DateTime FromDate, DateTime ToDate, string brCode);
        Task<TDLedgerVM> GetTDLedgerVM(decimal TDId, DateTime FromDate, DateTime ToDate, string brCode);
    }
}
