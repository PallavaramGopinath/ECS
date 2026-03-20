using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IFinLedgerTrnRepository
    {
        Task<bool> AddFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn);
        Task<bool> EditFinLedgerTrnAsync(Fin_Ledger_Trn finLedgerTrn);
        Task<bool> AddFinLedgerTrnListAsync(List<Fin_Ledger_Trn> finLedgerTrnList);
        Task<bool> UpdateLedgerBalance(decimal yrId, DateTime fromDate, DateTime toDate, string brCode);
        Task<List<FinBal>> GetGeneralLedgerBalance(int grpId,decimal yrId, string brCode);
    }
}
