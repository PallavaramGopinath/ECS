using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinVoucherBankHandler : IFinVoucherBankHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinVoucherBankHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinVoucherBankAsync(Fin_Voucher_Bank finVoucherBank)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherBank.AddFinVoucherBankAsync(finVoucherBank);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Bank transaction details not saved");
            }
            return result;
        }

        public async Task<bool> AddFinVoucherBankListAsync(List<Fin_Voucher_Bank> finVoucherBankList)
        {
            return await _unitOfWork.FinVoucherBank.AddFinVoucherBankListAsync(finVoucherBankList);
        }

        public async Task<bool> EditFinVoucherBankAsync(Fin_Voucher_Bank finVoucherBank)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherBank.EditFinVoucherBankAsync(finVoucherBank);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Bank transaction details not deleted");
            }
            return result;
        }
    }
}
