using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayPFRoiTemplateHandler : IPayPFRoiTemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayPFRoiTemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayPFRoiTemplate.AddPayPFRoiTemplateAsync(payPFROITemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! PF rate of interest template not saved");
            }
            return result;
        }

        public async Task<bool> EditPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayPFRoiTemplate.EditPayPFRoiTemplateAsync(payPFROITemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! PF rate of interest template not deleted");
            }
            return result;
        }

        public async Task<List<Pay_PF_ROITemplate>> GetPayPFROITemplateListAsync()
        {
            return await _unitOfWork.PayPFRoiTemplate.GetPayPFROITemplateListAsync();
        }
    }
}
