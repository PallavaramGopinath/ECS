using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;

namespace Infin8.Coapp.BusinessLogic
{
    public class DashboardMemberHandler : IDashboardMemberHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DashboardMemberHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //public List<MemberDashBoardAccountsDto> GetMemberDashBoardAccounts(decimal id, DateTime fromDate, DateTime toDate)
        //{
        //    List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new();
        //    try
        //    {
        //        memberDashBoardAccounts = _unitOfWork.DashboardMember.GetMemberDashBoardAccounts(id, fromDate, toDate);
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //    return memberDashBoardAccounts;
        //}

        public  List<MemberDashBoardAccountsDto> DashboardAccounts(decimal id, DateTime fromDate, DateTime toDate, string brCode)
        {
            List<MemberDashBoardAccountsDto> memberDashBoardAccounts = new();
            try
            {
                memberDashBoardAccounts =  _unitOfWork.DashboardMember.DashboardAccounts(id, fromDate, toDate,brCode );
            }
            catch (Exception)
            {

                throw;
            }
            return memberDashBoardAccounts;
        }

        public async Task<List<LoanTransactionVM>> GetLoanDetailsBetweenDatesOnLoanId(decimal LoanId, DateTime FromDate, DateTime ToDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetLoanDetailsBetweenDatesOnLoanId(LoanId, FromDate, ToDate, brCode);
        }

        public async Task<LoanLedgerVM> GetLoanLedger(decimal loanId, DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetLoanLedger(loanId, fromDate, toDate, brCode);
        }

        public async Task<LoanTransactionVM> GetLoanOBOnLoanId(decimal LoanId, DateTime FromDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetLoanOBOnLoanId(LoanId,FromDate, brCode);
        }

        public async Task<List<MemberDashBoardLoans>> GetMemberDashBoardLoans(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetMemberDashBoardLoans(memId, fromDate, toDate, brCode);
        }

        public async Task<List<MemberDashBoardTD>> GetMemberDashBoardTDs(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetMemberDashBoardTDs(memId, fromDate, toDate, brCode);
        }

        public async Task<MemberDetailsVM2> GetMemberDataDashBoard(decimal memId, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetMemberDataDashBoard(memId, brCode);
        }

        public async Task<List<MemberLedgerVM>> GetMemberLedger(decimal MemId, int TrnType, DateTime FromDate, DateTime ToDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetMemberLedger(MemId, TrnType, FromDate, ToDate,  brCode);
        }

        public async Task<TDLedgerVM> GetTDLedgerVM(decimal TDId, DateTime FromDate, DateTime ToDate, string brCode)
        {
            return await _unitOfWork.DashboardMember.GetTDLedgerVM(TDId, FromDate, ToDate, brCode);
        }
    }
}
