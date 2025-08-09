using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsJewelLoanHandler : IReportsJewelLoanHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public ReportsJewelLoanHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<rptJewelLoanOverdueList>> GetJewelLoanAboveLimitList(DateTime asOnDate, double limitAmt,string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanAboveLimitList(asOnDate, limitAmt,brCode );
        }

        public async Task<List<rptJewelLoanIssueRegister>> GetJewelLoanIssueRegisterList(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanIssueRegisterList(fromDate, toDate, brCode);
        }

        public async Task<List<rptJewelLoanClaim>> GetJewelLoanClaim(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanClaim(fromDate, toDate, brCode);
        }
        public async Task<List<rptJewelLoanLedgerToMember>> GetJewelLoanLedgerToMembers(List<decimal> loanIdList, DateTime fromDate, DateTime toDate)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanLedgerToMembers(loanIdList, fromDate, toDate);
        }

        public async Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingForMember(decimal memId, DateTime asOnDate)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanOutstandingForMember(memId, asOnDate);
        }

        public async Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingList(DateTime asOnDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanOutstandingList(asOnDate,brCode );
        }

        public async Task<List<rptJewelLoanOutstandingSchemeWiseList>> GetJewelLoanOutstandingSchemeWiseList(DateTime asOnDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanOutstandingSchemeWiseList(asOnDate, brCode);
        }

        public async Task<List<rptJewelLoanOverdueList>> GetJewelLoanOverdueList(DateTime asOnDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanOverdueList(asOnDate, brCode);
        }

        public async Task<List<rptJewelLoanOverdueSchemeWiseList>> GetJewelLoanOverdueSchemeWiseList(DateTime asOnDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanOverdueSchemeWiseList(asOnDate, brCode);
        }

        public async Task<List<rptJewelLoanRedemptionList>> GetJewelLoanRedemptionList(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanRedemptionList(fromDate, toDate, brCode);
        }
        public async Task<List<rptJewelLoanRedemption>> GetJewelLoanRedemption(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanRedemption(fromDate, toDate, brCode);
        }
        public async Task<List<rptJewelLoanStockRegisterList>> GetJewelLoanStockRegisteList(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanStockRegisteList(fromDate,toDate,loanType,brCode );
        }

        public async Task<List<rptJewelLoanVerificationList>> GetJewelLoanVerificationList(DateTime asOnDate, string brCode)
        {
            return await _unitOfWork.ReportsJewelLoan.GetJewelLoanVerificationList(asOnDate, brCode);
        }

        public async Task<List<rptJewelLoanLedgerMain>> Print_JewelLoanLedgerMain(List<decimal> loanIdList)
        {
            return await _unitOfWork.ReportsJewelLoan.Print_JewelLoanLedgerMain(loanIdList);
        }

        
    }
}
