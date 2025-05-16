using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsFinalAccountsHandler : IReportsFinalAccountsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public ReportsFinalAccountsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<rptFADividend>> GetDividendFA(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetDividendFA(fromDate, toDate, trnType, brCode);
        }

        public async Task<List<rptFAShareCapitalDevidend>> GetFAShareCapitalDividend(DateTime fromDate, DateTime todate, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetFAShareCapitalDividend(fromDate, todate, brCode);
        }

        public async Task<List<rptFAFDIntPaidPayable>> GetFDInterestPaidAndPayable(DateTime fromDate, DateTime toDate, string brCode)
        {
            return  await _unitOfWork.ReportsFinalAccounts.GetFDInterestPaidAndPayable(fromDate, toDate, brCode);
        }

        public async Task<List<rptFALedgerTrn>> GetLedgerOutstandingFA(DateTime fromDate, DateTime toDate, decimal yrId, int fnlId, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetLedgerOutstandingFA(fromDate,toDate , yrId, fnlId, brCode);    
        }

        public async Task<List<rptFALoanOutstanding>> GetLoanOutstandingForFA_HL(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetLoanOutstandingForFA_HL(fromDate,toDate , loanType, brCode);
        }

        public async Task<List<rptFALoanOutstanding2>> GetLoanOutstandingForFA_HL_WithRceiptDates(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetLoanOutstandingForFA_HL_WithRceiptDates(fromDate,toDate , loanType, brCode);
        }

        public async Task<List<rptFALoanOutstanding>> GetLoanOutstandingJLFDRD(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetLoanOutstandingJLFDRD(fromDate,toDate , loanType, brCode);
        }

        public async Task<List<rptMemberTrn>> GetRptMemberTrnSchedule(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetRptMemberTrnSchedule(fromDate,toDate , trnType, brCode);
        }

        public async Task<List<rptMemberSBAcTrn>> GetSBAccountTrn(DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.ReportsFinalAccounts.GetSBAccountTrn(fromDate,toDate , brCode);
        }

        public async Task<List<rptTermDepositOutstanding>> GetTermDepositOutstandingFA(DateTime fromDate, DateTime toDate, string TDType, string brCode)
        {
            return  await _unitOfWork.ReportsFinalAccounts.GetTermDepositOutstandingFA(fromDate,toDate, TDType, brCode);
        }
    }
}
