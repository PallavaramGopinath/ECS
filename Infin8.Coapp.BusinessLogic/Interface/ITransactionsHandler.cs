using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ITransactionsHandler
    {
        Task<List<DropdownItem>> GetTransactionAccounts(string accountStatus, string accountBelongTo);
        Task<List<DtoAccount_Transactions>> GetAllAccountsTransactions();
        Task<List<DropdownItem>> GetSuspenseLedgerItems(int suspeneType,string brCode);
        Task<List<DropdownItem>> GetShareCapitalLedgerItem(string brCode);
        Task<List<DropdownItem>> GetBankLedgerItems(string brCode);
        Task<List<DropdownItem>> GetLedgersExpectBankLedgerItems(string brCode);
        Task<string> GetComponentName(int accountId);
    }
}
