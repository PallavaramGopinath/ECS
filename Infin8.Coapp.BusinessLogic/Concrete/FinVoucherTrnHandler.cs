using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinVoucherTrnHandler : IFinVoucherTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinVoucherTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinVoucherTrnAsync(Fin_Voucher_Trn finVoucherTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnAsync(finVoucherTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payment voucher details not saved");
            }
            return result;
        }

        public async Task<bool> AddFinVoucherTrnList(List<Fin_Voucher_Trn> finVoucherTrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherTrn.AddFinVoucherTrnList(finVoucherTrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payment voucher details not saved");
            }
            return result;
        }

        public async Task<bool> EditFinVoucherTrnAsync(Fin_Voucher_Trn finVoucherTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherTrn.EditFinVoucherTrnAsync(finVoucherTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payment voucher details not deleted");
            }
            return result;
        }

        public async Task<double> GetLedgerBalanceByLedIdAsync(decimal ledId, decimal yearId, string brCode)
        {
            decimal cashLedId = 0;
            cashLedId = await _unitOfWork.MapGeneral.GetCashLedgerIdAsync(brCode);
            return await _unitOfWork.FinVoucherTrn.GetLedgerBalanceByLedIdAsync(ledId, cashLedId, yearId);

        }

        public async Task<DtoVoucher> GetTransactionById(decimal vocId, string brCode)
        {
            return await _unitOfWork.FinVoucherTrn.GetTransactionById(vocId, brCode);
        }

        public async Task<DtoVoucher> GetTransactionByNo(string rptNo, string pmtNo, decimal yrId)
        {
            return await _unitOfWork.FinVoucherTrn.GetTransactionByNo(rptNo, pmtNo, yrId);
        }
    }
}
