using Infin8.Coapp.Dto;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TransactionsHandler : ITransactionsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TransactionsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<List<DropdownItem>> GetTransactionAccounts(string accountStatus, string accountBelongTo)
        {
            return await _unitOfWork.TransactionsRepository.GetTransactionAccounts(accountStatus, accountBelongTo);
        }
        public async Task<List<DropdownItem>> GetSuspenseLedgerItems(int suspeneType,string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetSuspenseLedgerItems(suspeneType,brCode);
        }

        public async Task<List<DropdownItem>> GetShareCapitalLedgerItem(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetShareCapitalLedgerItem(brCode);
        }

        public async Task<List<DropdownItem>> GetBankLedgerItems(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetBankLedgerItems(brCode);
        }

        public async Task<List<DropdownItem>> GetLedgersExpectBankLedgerItems(string brCode)
        {
            return await _unitOfWork.TransactionsRepository.GetLedgersExpectBankLedgerItems(brCode);
        }

        public async Task<string> GetComponentName(int accountId)
        {
            return await _unitOfWork.TransactionsRepository.GetComponentName(accountId);
        }
    }
}
