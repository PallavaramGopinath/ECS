using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinVoucherHandler : IFinVoucherHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public FinVoucherHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddFinVoucherAsync(Fin_Voucher finVouocher)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucher.AddFinVoucherAsync(finVouocher); 
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payment voucher not saved");
            }
            return result;
        }

        public async Task<bool> EditFinVoucherAsync(Fin_Voucher finVouocher)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinVoucher.EditFinVoucherAsync(finVouocher);
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
    }
}
