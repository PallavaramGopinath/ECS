using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayRetirementHandler : IPayRetirementHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayRetirementHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayRetirementAsync(Pay_Retirement payRetirement)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayRetirement.AddPayRetirementAsync(payRetirement);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee retirement not saved");
            }
            return result;
        }

        public async Task<bool> EditPayRetirementAsync(Pay_Retirement payRetirement)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayRetirement.EditPayRetirementAsync(payRetirement);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay retirement not deleted");
            }
            return result;
        }
    }
}
