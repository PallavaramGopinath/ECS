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
        public async Task<List<Pay_PF_ROITemplate>> AddPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate)
        {
            List<Pay_PF_ROITemplate> roiList = new();
            try
            {
                var result = await _unitOfWork.PayPFRoiTemplate.AddPayPFRoiTemplateAsync(payPFROITemplate);
                await _unitOfWork.CompleteAsync();
                if (result != null && result.Count > 0) roiList = result.ToList();
            }
            catch (Exception ex)
            {
                roiList = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! PF rate of interest template not saved");
            }
            return roiList;
        }

        public async Task<List<Pay_PF_ROITemplate>> EditPayPFRoiTemplateAsync(Pay_PF_ROITemplate payPFROITemplate)
        {
            List<Pay_PF_ROITemplate> roiList = new();
            try
            {
                var result = await _unitOfWork.PayPFRoiTemplate.EditPayPFRoiTemplateAsync(payPFROITemplate);
                if(result != null && result.Count > 0) roiList = result.ToList();
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                roiList = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! PF rate of interest template not deleted");
            }
            return roiList;
        }

        public async Task<List<Pay_PF_ROITemplate>> GetPayPFROITemplateListAsync(string brCode)
        {
            return await _unitOfWork.PayPFRoiTemplate.GetPayPFROITemplateListAsync(brCode);
        }
        public async Task<Pay_PF_ROITemplate> GetPayPFRoiTemplateByDate(DateTime salaryDate, string brCode)
        {
            return await _unitOfWork.PayPFRoiTemplate.GetPayPFRoiTemplateByDate(salaryDate, brCode);
        }
    }
}
