using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IFinLedgerSubGroupRepository
    {
        Task<bool> AddFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp);
        Task<bool> EditFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp);
    }
}
