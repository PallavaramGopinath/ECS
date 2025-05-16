using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    
    public class PaySlipHandler : IPaySlipHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PaySlipHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PaySlip.AddPaySlipAsync(paySlip);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay slip not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PaySlip.EditPaySlipAsync(paySlip);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay slip not deleted");
            }
            return result;
        }
    }
}
