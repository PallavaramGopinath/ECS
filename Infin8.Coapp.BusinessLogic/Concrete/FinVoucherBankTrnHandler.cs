using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinVoucherBankTrnHandler : IFinVoucherBankTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinVoucherBankTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinVoucherBankTrAsync(Fin_Voucher_Bank_Trn finVoucherBankTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherBankTrn.AddFinVoucherBankTrAsync(finVoucherBankTrn);
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

        public async Task<bool> EditFinVoucherBankTrAsync(Fin_Voucher_Bank_Trn finVoucherBankTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucherBankTrn.EditFinVoucherBankTrAsync(finVoucherBankTrn);
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
