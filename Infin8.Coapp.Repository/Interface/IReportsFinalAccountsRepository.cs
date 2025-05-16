using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IReportsFinalAccountsRepository
    {
        Task<List<rptMemberTrn>> GetRptMemberTrnSchedule(DateTime fromDate, DateTime toDate, int trnType,string brCode);
        Task<List<rptFALoanOutstanding>> GetLoanOutstandingForFA_HL(DateTime fromDate, DateTime toDate, int loanType, string brCode);
        Task<List<rptFALoanOutstanding>> GetLoanOutstandingJLFDRD(DateTime fromDate, DateTime toDate, int loanType,string brCode);
        Task<List<rptTermDepositOutstanding>> GetTermDepositOutstandingFA(DateTime fromDate, DateTime toDate, string TDType, string brCode);
        Task<List<rptFADividend>> GetDividendFA(DateTime fromDate, DateTime toDate, int trnType, string brCode);
        Task<List<rptFALedgerTrn>> GetLedgerOutstandingFA(DateTime fromDate, DateTime toDate, decimal yrId, int fnlId, string brCode);
        Task<List<rptFAFDIntPaidPayable>> GetFDInterestPaidAndPayable(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptFALoanOutstanding2>> GetLoanOutstandingForFA_HL_WithRceiptDates(DateTime fromDate, DateTime toDate, int loanType, string brCode);
        Task<List<rptFAShareCapitalDevidend>> GetFAShareCapitalDividend(DateTime fromDate, DateTime todate, string brCode);
        Task<List<rptMemberSBAcTrn>> GetSBAccountTrn(DateTime fromDate, DateTime toDate, string brCode);
    }
}
