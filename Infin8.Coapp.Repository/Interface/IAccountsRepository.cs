using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IAccountsRepository
    {
        Task<bool> AddAccountsTransaction(Account_Transactions accountTransaction);
        Task<bool> EditAccountsTransaction(Account_Transactions accountTransaction);
        Task<string> GetTransactedAccountNameFromAccount_Transactions(int accId);
        Task<decimal> GetCashLedgerId(string brCode);
        Task<(double OBAmount, double CBAmount)> GetLedgerOBAndCBAmount(decimal ledId, decimal yrId, DateTime toDate,string brCode);
        Task<double> GetPreviousReceipt(decimal ledId, decimal yrId, DateTime dateUpto);
        Task<double> GetPreviousPayment(decimal ledId, decimal yrId, DateTime dateUpto);
        Task<double> GetLedgerBalance(decimal ledId, decimal yrId, DateTime upToDate,string brCode);
        Task<DtoLedgerBalance> GetLedgerBalanceWithFnlId(decimal ledId, decimal yrId, DateTime upToDate, string brCode);
        string GetLedgerNameByLedId(decimal ledId);
        Task<bool> UpdateLedgerBalance(decimal yrId, DateTime fromDate, DateTime toDate,string brCode);
    }
}
