using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IReportsTermDepositsRepository
    {
        Task<List<rptTermDepositRegister>> GetTermDepositRegister(List<decimal> TDIdList, DateTime fromDate, DateTime toDate, string TDSchemeType);
        Task<List<rptFDOutstanding>> GetTermDepositOutstanding(DateTime asOnDate, string TDSchemeType,string brCode);
        Task<List<rptFDOutstanding>> GetTermDepositOutstandingIndividual(decimal memId, DateTime asOnDate, string TDSchemeType);
        Task<List<rptTermDepositPayable>> GetTermDepositPayable(DateTime asOnDate, string brCode);
        Task<List<rptTermDepositPayable>> GetTermDepositMaturityPayable(DateTime asOnDate, string tdSchemeType, string brCode);
        Task<rptFDBond> GetFDBondPreprinted(decimal vocId);
        Task<List<rptTDNewBetweenDates>> GetTermDepositReceivedDuringPeriod(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode);
        Task<List<rptTDRefundBetweenDates>> GetFDRefundBetweenDated(DateTime fromDate, DateTime toDate, string TDSchemeType,string brCode);

    }
}
