using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayBasicPayHandler : IPayBasicPayHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayBasicPayHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayBasicPayAsync(Pay_BasicPay payBasicPay)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayBasicPay.AddPayBasicPayAsync(payBasicPay);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's new basic pay not saved");
            }
            return result;
        }

        public async Task<bool> EditPayBasicPayAsync(Pay_BasicPay payBasicPay)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayBasicPay.EditPayBasicPayAsync(payBasicPay);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's new basic pay not deleted");
            }
            return result;
        }
    }
}
