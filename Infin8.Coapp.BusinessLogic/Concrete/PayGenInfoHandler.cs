using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayGenInfoHandler : IPayGenInfoHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayGenInfoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayGenInfoAsync(Pay_Gen_Info payGenInfo)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayGenInfo.AddPayGenInfoAsync(payGenInfo);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payroll Info data not saved");
            }
            return result;
        }

        public async Task<bool> EditPayGenInfoAsync(Pay_Gen_Info payGenInfo)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayGenInfo.EditPayGenInfoAsync(payGenInfo);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Payroll Info data not deleted");
            }
            return result;
        }
    }
}
