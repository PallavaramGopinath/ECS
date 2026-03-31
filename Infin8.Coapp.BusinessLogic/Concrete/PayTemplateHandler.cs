using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayTemplateHandler : IPayTemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayTemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddPayTemplateAsync(Pay_Template payTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayTemplate.AddPayTemplateAsync(payTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay roll template not saved");
            }
            return result;
        }

        public async Task<bool> EditPayTemplateAsync(Pay_Template payTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayTemplate.EditPayTemplateAsync(payTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Pay roll template not deleted");
            }
            return result;
        }

        public async Task<Pay_Template> GetPayTemplateAsync()
        {
            return await _unitOfWork.PayTemplate.GetPayTemplateAsync();
        }

        public async Task<Pay_Template> GetPayTemplateAsync(string brCode)
        {
            return await _unitOfWork.PayTemplate.GetPayTemplateAsync(brCode);
        }
    }
}
