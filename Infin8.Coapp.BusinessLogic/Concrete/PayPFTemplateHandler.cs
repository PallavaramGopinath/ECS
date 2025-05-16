using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayPFTemplateHandler : IPayPFTemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayPFTemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddPayPFTemplateAsync(Pay_PF_Template payPFTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayPFTemplate.AddPayPFTemplateAsync(payPFTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! PF template not saved");
            }
            return result;
        }

        public async Task<bool> EditPayPFTemplateAsync(Pay_PF_Template payPFTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayPFTemplate.EditPayPFTemplateAsync(payPFTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pf template not deleted");
            }
            return result;
        }

        public async Task<List<Pay_PF_Template>> GetPayPFTemplateListAsync()
        {
            return await _unitOfWork.PayPFTemplate.GetPayPFTemplateListAsync();
        }
    }
}
