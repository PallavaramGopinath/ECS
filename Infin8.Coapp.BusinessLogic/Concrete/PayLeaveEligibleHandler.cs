using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayLeaveEligibleHandler : IPayLeaveEligibleHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayLeaveEligibleHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayLeaveEligibleAsync(Pay_Leave_Eligible payLeaveEligible)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayLeaveEligible.AddPayLeaveEligibleAsync(payLeaveEligible);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Leave eligible master not saved");
            }
            return result;
        }

        public async Task<bool> EditPayLeaveEligibleAsync(Pay_Leave_Eligible payLeaveEligible)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayLeaveEligible.EditPayLeaveEligibleAsync(payLeaveEligible);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Leave eligible master  not deleted");
            }
            return result;
        }
    }
}
