using Infin8.Coapp.Dto;
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
        public async Task<string> GetTransactedAccountNameFromAccount_Transactions(int accId)
        {
            return await _unitOfWork.Accounts.GetTransactedAccountNameFromAccount_Transactions(accId);
        }

        public async Task<decimal> GetCashLedgerId(string brCode)
        {
            return await _unitOfWork.Accounts.GetCashLedgerId(brCode);
        }
        public async Task<bool> IsBankLedger(decimal ledgerId, string brCode)
        {
            return await _unitOfWork.Accounts.IsBankLedger(ledgerId, brCode);
        }

        public async Task<double> GetLedgerBalance(decimal ledId, decimal yrId, DateTime upToDate, string brCode)
        {
            return await _unitOfWork.Accounts.GetLedgerBalance(ledId, yrId, upToDate,brCode);
        }

        public async Task<DtoLedgerBalance> GetLedgerBalanceWithFnlId(decimal ledId, decimal yrId, DateTime upToDate, string brCode)
        {
            return await _unitOfWork.Accounts.GetLedgerBalanceWithFnlId(ledId, yrId, upToDate,brCode);
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

        public async Task<bool> CreateNewFinancialYear(decimal yrId,DateTime fromDate, DateTime toDate, decimal created_By, string brCode)
        {
            bool result = false;
            try
            {
                _unitOfWork.BeginTransaction();
                var isAllDaysClosed = await _unitOfWork.Calendars.CanBeginDay(brCode);
                if (isAllDaysClosed == false)
                {
                    Console.Write("Some of dates are not closed");
                    result = false;
                    return result;
                }
                //var resultUpdt = await UpdateLedgerBalance(yrId, fromDate, toDate, brCode);
                var resultNewYear = await _unitOfWork.Accounts.CreateNewFinancialYear(yrId, fromDate, toDate, created_By, brCode);
                var resultNewCalendar = await _unitOfWork.Calendars.CreateNewCalendar(toDate,resultNewYear.Yr_Id ,created_By ,brCode);
                _unitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _unitOfWork.RollBack();
            }
            return result;
        }
    }
}
