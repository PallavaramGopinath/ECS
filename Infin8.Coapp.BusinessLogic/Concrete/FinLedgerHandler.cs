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
                List<Fin_Ledger_Trn> trnList = new();
                List<Fin_Yr_Master> yearList = await _unitOfWork.FinYearMaster.GetFinancialYearList(finLedger.BrCode!);

                decimal ledId = await _unitOfWork.FinLedger.AddFinLedgerAsync(finLedger);

                foreach (var yr in yearList)
                {
                    Fin_Ledger_Trn ledTrn = new Fin_Ledger_Trn();
                    ledTrn.Trn_Id = 0;
                    ledTrn.Led_Id = ledId;
                    ledTrn.OB_Amt = 0;
                    ledTrn.Tot_Rpt_Amt = 0;
                    ledTrn.Tot_Pmt_Amt = 0;
                    ledTrn.CB_Amt = 0;
                    ledTrn.Usr_Id = finLedger.Usr_Id;
                    ledTrn.Yr_Id = yr.Yr_Id;
                    ledTrn.BrCode = finLedger.BrCode;
                    ledTrn.LedgerTrn_Delete = false;
                    trnList.Add(ledTrn);
                }
                _unitOfWork.BeginTransaction();
                await _unitOfWork.FinLedgerTrn.AddFinLedgerTrnListAsync(trnList);
                await _unitOfWork.CompleteAsync();
                _unitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Ledger name not saved");
                _unitOfWork.RollBack();
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

        public async Task<Fin_Ledger> GetLedgerByIdAsync(decimal Id, string brCode)
        {
            return await _unitOfWork.FinLedger.GetLedgerByIdAsync(Id, brCode);
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

        public async Task<List<Fin_Ledger>> GetLedgerList10Async(string brCode)
        {
            return await _unitOfWork.FinLedger.GetLedgerList10Async(brCode);
        }

        public async Task<List<Fin_Ledger>> GetLedgerListAsync(string brCode)
        {
            return await _unitOfWork.FinLedger.GetLedgerListAsync(brCode);
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

        public async Task<List<FinLedgerVM>> GetLedgerVMListAsync(string brCode)
        {
            return await _unitOfWork.FinLedger.GetLedgerVMListAsync(brCode);
        }
    }
}
