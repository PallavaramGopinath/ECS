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

        public async Task<List<Pay_PF_Template>> AddPayPFTemplateAsync(Pay_PF_Template payPFTemplate)
        {
            List<Pay_PF_Template> list = new();
            try
            {
                var result = await _unitOfWork.PayPFTemplate.AddPayPFTemplateAsync(payPFTemplate);
                await _unitOfWork.CompleteAsync();
                if(result != null && result.Count > 0)
                {
                    list = result;
                }
            }
            catch (Exception)
            {
                list = new();
                //throw new InvalidOperationException(ex.Message + " Something went wrong! PF template not saved");
            }
            return list;
        }

        public async Task<List<Pay_PF_Template>> EditPayPFTemplateAsync(Pay_PF_Template payPFTemplate)
        {
            List<Pay_PF_Template> list = new();
            try
            {
                var result = await _unitOfWork.PayPFTemplate.EditPayPFTemplateAsync(payPFTemplate);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0)
                {
                    list = result;
                }
            }
            catch (Exception)
            {
                list = new();
                //throw new InvalidOperationException(ex.Message + " Something went wrong! Pf template not deleted");
            }
            return list;
        }

        public async Task<List<Pay_PF_Template>> GetPayPFTemplateListAsync(string brCode)
        {
            return await _unitOfWork.PayPFTemplate.GetPayPFTemplateListAsync(brCode);
        }
    }
}
