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
        Task<List<DropdownItem>> GetAllLedgerItems(string brCode);
        Task<List<DropdownItem>> GetSuspenseLedgerItems(int suspeneType,string brCode);
        Task<List<DropdownItem>> GetShareCapitalLedgerItem(string brCode);
        Task<List<DropdownItem>> GetBankLedgerItems(string brCode);
        Task<List<DropdownItem>> GetLedgersExpectBankLedgerItems(string brCode);
        Task<string> GetComponentName(int accountId);
        Task<string> GetViewComponentName(int accountId);
        Task<bool> SaveTransaction(decimal stagingId,string vocMode,decimal Checked_By, decimal yrId); /// vocMode member transaction, staff transaction 
        Task<bool> SaveAccountTransaction(decimal stagingId, string vocMode, decimal Checked_By, decimal yrId);
        Task<bool> RejectTransaction(decimal stagingId, decimal Checked_By);
        Task<List<DtoTransaction>> GetStagingDataByStagingId(decimal stagingId);
        DtoTransactionRptPmtNos GetReceiptAndPaymentNo(double cashReceipt, double cashPayment, double adjReceipt, double adjPayment, bool IsChequeOnly, decimal yrId);
        Task<bool> IsChequeOnly(decimal stagingId);
        Task<string> GetTransactionStatusByAccId(int accId);
    }
}
