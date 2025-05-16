using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IFinLedgerSubGroupHandler
    {
        Task<bool> AddFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp);
        Task<bool> EditFinLedgerSubGroupAsync(Fin_Ledger_SubGrp finLedgerSubGrp);
        Task<List<DropdownItem>> GetFinLedgerGroupItemsAsync();
    }
}
