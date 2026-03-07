using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IFinLedgerRepository
    {
        Task<decimal> AddFinLedgerAsync(Fin_Ledger finLedger);
        Task<bool> EditFinLedgerAsync(Fin_Ledger finLedger);
        Task<List<DropdownItem>> GetLedgerListAsync(int fnlId,decimal cashLedId,string brCode );
        Task<Fin_Ledger> GetLedgerByIdAsync(decimal Id, string brCode);
        Task<List<Fin_Ledger>> GetLedgerListByGrpIdAsync(int grpId,string brCode);
        Task<List<DropdownItem>> GetLedgerItemsExceptBankLedgersAsyn(string brCode);
        Task<List<DropdownItem>> GetLedgerItemsByFnlIdAsync(int fnlId,decimal cashLedId,string brCode);
        Task<List<Fin_Ledger>> GetLedgerList10Async(string brCode);
        Task<List<Fin_Ledger>> GetLedgerListAsync(string brCode);

        //Task<decimal> GetCashLedgerIdAsync();
    }
}
