using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReportsLoanHandler
    {
        //Task<List<rptLoanLedger>> GetLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate);
        //Task<List<rptLoanOutstanding>> GetLoanOutstandingWithAgewise(DateTime toDate, int loanType);
        //Task<List<rptLoanDCB>> GetLoanDCB(DateTime fromDate, DateTime toDate);
        //Task<List<rptLoanDisbursementHSIS>> GetLoanDisbursement(DateTime fromDate, DateTime toDate, int loanType);
        Task<List<rptLoanLedger>> GetLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate);
        Task<List<rptLoanOutstanding>> GetLoanOutstandingWithAgewise(DateTime toDate, int loanType, string brCode);
        Task<List<rptLoanDCB>> GetLoanDCB(DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptLoanDisbursementHSIS>> GetLoanDisbursement(DateTime fromDate, DateTime toDate, int loanType, string brCode);
    }
}
