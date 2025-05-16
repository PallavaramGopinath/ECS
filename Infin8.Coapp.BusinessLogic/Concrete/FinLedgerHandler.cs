using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinLedgerHandler : IFinLedgerHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinLedgerHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinLedgerAsync(Fin_Ledger finLedger)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedger.AddFinLedgerAsync(finLedger);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Ledger name not saved");
            }
            return result;
        }

        public async Task<bool> EditFinLedgerAsync(Fin_Ledger finLedger)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinLedger.EditFinLedgerAsync(finLedger);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Ledger name not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetLedgerItemsByFnlIdAsync(int fnlId,  string brCode)
        {
            decimal cashLedId = 0;
            cashLedId = await _unitOfWork.MapGeneral.GetCashLedgerIdAsync(brCode);
            return await _unitOfWork.FinLedger.GetLedgerItemsByFnlIdAsync(fnlId,cashLedId,brCode);
        }

        public async Task<List<DropdownItem>> GetLedgerItemsExceptBankLedgersAsyn(string brCode)
        {
            return await _unitOfWork.FinLedger.GetLedgerItemsExceptBankLedgersAsyn(brCode);
        }

        public async Task<List<DropdownItem>> GetLedgerListAsync(int fnlId, string brCode)
        {
            decimal cashLedId = 0 ;
            cashLedId =  await _unitOfWork.MapGeneral.GetCashLedgerIdAsync(brCode);
            return await _unitOfWork.FinLedger.GetLedgerListAsync(fnlId, cashLedId,brCode);
        }

        //public async Task<decimal> GetCashLedgerIdAsync()
        //{
        //    return await _unitOfWork.FinLedger.GetCashLedgerIdAsync();
        //}

        public async Task<List<Fin_Ledger>> GetLedgerListByGrpIdAsync(int grpId, string brCode)
        {
            return await _unitOfWork.FinLedger.GetLedgerListByGrpIdAsync(grpId,brCode);
        }
    }
}
