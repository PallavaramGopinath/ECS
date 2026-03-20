using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IFinLedgerGroupHandler
    {
        Task<bool> AddFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup);
        Task<bool> EditFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup);
        Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(int fnlId, string brCode);
        Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(string brCode);
        Task<List<FinBal>> GetGroupLedgerBalance(decimal yrId, string brCode);
    }
}
