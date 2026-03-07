using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IReportsLoanRepository
    {
        Task<List<rptLoanLedger>> GetLoanLedger(List<decimal> loanIdList, DateTime fromDate, DateTime toDate, string brCode);
        Task<List<rptLoanOutstanding>> GetLoanOutstanding(DateTime toDate, int loanType, string brCode);
        Task<List<rptLoanOutstanding>> GetLoanOutstandingWithAgewise(DateTime toDate, int loanType,string brCode);
        Task<List<rptLoanDCB>> GetLoanDCB(DateTime fromDate, DateTime toDate,string brCode);
        Task<List<rptLoanDisbursementHSIS>> GetLoanDisbursement(DateTime fromDate, DateTime toDate, int loanType, string brCode);
    }
}
