using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class AccountsHandler : IAccountsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public AccountsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddAccountsTransaction(Account_Transactions accountTransaction)
        {
            return await _unitOfWork.Accounts.AddAccountsTransaction(accountTransaction);
        }

        public async Task<bool> EditAccountsTransaction(Account_Transactions accountTransaction)
        {
            return await _unitOfWork.Accounts.EditAccountsTransaction(accountTransaction);
        }

        public async Task<decimal> GetCashLedgerId(string brCode)
        {
            return await _unitOfWork.Accounts.GetCashLedgerId(brCode);
        }

        public async Task<double> GetLedgerBalance(decimal ledId, decimal yrId, DateTime upToDate)
        {
            return await _unitOfWork.Accounts.GetLedgerBalance(ledId, yrId, upToDate);
        }

        public string GetLedgerNameByLedId(decimal ledId)
        {
            return _unitOfWork.Accounts.GetLedgerNameByLedId(ledId);
        }

        public async Task<(double OBAmount, double CBAmount)> GetLedgerOBAndCBAmount(decimal ledId, decimal yrId, DateTime toDate, string brCode)
        {
            return await _unitOfWork.Accounts.GetLedgerOBAndCBAmount(ledId, yrId, toDate, brCode);
        }

        public async Task<double> GetPreviousPayment(decimal ledId, decimal yrId, DateTime dateUpto)
        {
            return await _unitOfWork.Accounts.GetPreviousPayment(ledId, yrId, dateUpto);
        }

        public async Task<double> GetPreviousReceipt(decimal ledId, decimal yrId, DateTime dateUpto)
        {
            return await _unitOfWork.Accounts.GetPreviousReceipt(ledId, yrId, dateUpto);
        }

        public async Task<bool> UpdateLedgerBalance(decimal yrId, DateTime fromDate, DateTime toDate, string brCode)
        {
            return await _unitOfWork.Accounts.UpdateLedgerBalance(yrId, fromDate, toDate, brCode);
        }

    }
}
