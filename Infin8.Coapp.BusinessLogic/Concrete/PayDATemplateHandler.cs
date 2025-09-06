using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayDATemplateHandler : IPayDATemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayDATemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddPayDATemplateAsync(Pay_DA_Template payDaTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayDATemplate.AddPayDATemplateAsync(payDaTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dearness allowance template not saved");
            }
            return result;
        }

        public async Task<bool> EditPayDATemplateAsync(Pay_DA_Template payDaTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayDATemplate.EditPayDATemplateAsync(payDaTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dearness allowance template not deleted");
            }
            return result;
        }

        public async Task<List<Pay_DA_Template>> GetPayDATemplateListAsync(string status)
        {
            return await _unitOfWork.PayDATemplate.GetPayDATemplateListAsync(status);
        }
        public async Task<Pay_DA_Template> GetPayDATemplate(DateTime wef, string status, string brCode)
        {
            return await  _unitOfWork.PayDATemplate.GetPayDATemplate(wef, status, brCode);
        }
    }
}
