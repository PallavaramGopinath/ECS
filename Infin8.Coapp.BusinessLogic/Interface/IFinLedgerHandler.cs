using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IFinLedgerHandler
    {
        Task<bool> AddFinLedgerAsync(Fin_Ledger finLedger);
        Task<bool> EditFinLedgerAsync(Fin_Ledger finLedger);
        Task<List<DropdownItem>> GetLedgerListAsync(int fnlId, string brCode);
        Task<List<DropdownItem>> GetLedgerItemsExceptBankLedgersAsyn(string brCode);
        Task<List<Fin_Ledger>> GetLedgerListByGrpIdAsync(int grpId, string brCode);

        Task<List<DropdownItem>> GetLedgerItemsByFnlIdAsync(int fnlId, string brCode);
        Task<List<Fin_Ledger>> GetLedgerList10Async(string brCode);
        //Task<decimal> GetCashLedgerIdAsync();
    }
}
