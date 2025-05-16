using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayAllDedHandler : IPayAllDedHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayAllDedHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddPayAllDedAsync(Pay_All_Ded payAllDed)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayAllDed.AddPayAllDedAsync(payAllDed);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's allowance & deduction details not saved");
            }
            return result;
        }

        public async Task<bool> EditPayAllDedAsync(Pay_All_Ded payAllDed)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayAllDed.EditPayAllDedAsync(payAllDed);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's allowance & deduction details not deleted");
            }
            return result;
        }
    }
}
