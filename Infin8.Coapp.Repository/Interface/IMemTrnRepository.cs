using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMemTrnRepository
    {
        Task<bool> AddMemTrnAsync(Mem_Trn memTrn);
        Task<bool> AddMemTrnListAsync(List<Mem_Trn> memTrnList);
        Task<bool> EditMemTrnAsync(Mem_Trn memTrn,string brCode);
        Task<DtoSBAccountBalanceWithIds> GetSBAccountBalanceWithIds(decimal memId, string brCode);
        Task<double> GetSBAccountBalanceByAccId(decimal accId, string brCode);
        Task<double> GetSBAccountInterestBalanceByAccId(decimal accId,string brCode);
        Task<double> GetMemTrnByMemIdAndLedIdAsync(decimal memId, decimal ledId,string brCode);
        Task<double> GetmemTrnTotalSuspenseAmount(decimal memId, int trnType,string brCode);
        Task<List<MemberTransactionVM>> GetMemberTrnBalanceList(decimal MemId, int TrnType, string brCode);
        Task<List<DividendOrIntOnTDPaymentVM>> GetDividendPayableListAsync(decimal memId, DateTime asOnDate, string brCode);
    }
}
