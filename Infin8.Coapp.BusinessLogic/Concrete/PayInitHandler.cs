using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayInitHandler : IPayInitHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayInitHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayInitAsync(Pay_Init payInit)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayInit.AddPayInitAsync(payInit);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Salary initialization not saved");
            }
            return result;
        }

        public async Task<bool> EditPayInitAsync(Pay_Init payInit)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayInit.EditPayInitAsync(payInit);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Salary initialization not deleted");
            }
            return result;
        }
    }
}
