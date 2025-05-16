using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IFinLedgerGroupRepository
    {
        Task<bool> AddFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup);
        Task<bool> EditFinLedgerGroupAsync(Fin_Ledger_Grp finLedgerGroup);
        Task<List<DropdownItem>> GetFinLedgerGroupItemsAsync();
        Task<List<Fin_Ledger_Grp>> GetFinLedgerGroupListAsync(int fnlId);
    }
}
