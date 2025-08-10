using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IReportsAccountRepository
    {
        Task<List<rptLedgerNameList>> GetLedgerNameList(string brCode);
        Task<List<rptDayBook2>> GetChittaBook(string fromDate, string toDate, decimal yrId,string brCode);
        Task<List<rptDayBook2>> GetDayBook(string fromDate, string toDate, decimal yrId, string brCode);
        Task<List<rptFinGeneralLedger>> GetGeneralLedger(DateTime fromDate, DateTime toDate, List<decimal> glLedIdList, decimal yrId,string brCode);
        Task<List<rptFinReceiptAndCharges>> GetReceiptAndCharges(DateTime fromDate, DateTime toDate, decimal yrId, string brCode);
        Task<List<rptFinBalanceSheet>> GetBalanceSheet(DateTime fromDate, DateTime toDate, decimal yrId, string brCode);
        Task<List<rptFinLossAndProfit>> GetLossAndProfit(DateTime fromDate, DateTime toDate, decimal yrId,string brCode);
    }
}
