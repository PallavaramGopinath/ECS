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
        Task<bool> AddFinLedgerAsync(Fin_Ledger finLedger);
        Task<bool> EditFinLedgerAsync(Fin_Ledger finLedger);
        Task<List<DropdownItem>> GetLedgerListAsync(int fnlId,decimal cashLedId,string brCode );
        Task<List<Fin_Ledger>> GetLedgerListByGrpIdAsync(int grpId,string brCode);
        Task<List<DropdownItem>> GetLedgerItemsExceptBankLedgersAsyn(string brCode);
        Task<List<DropdownItem>> GetLedgerItemsByFnlIdAsync(int fnlId,decimal cashLedId,string brCode);


        //Task<decimal> GetCashLedgerIdAsync();
    }
}
