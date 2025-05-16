using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanROITemplateHandler : ILoanROITemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanROITemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanROIAsync(Loan_Roi_Template roiTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanROITemplate.AddLoanROIAsync(roiTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest template not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanROIAsync(Loan_Roi_Template roiTemplate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanROITemplate.EditLoanROIAsync(roiTemplate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interst template not deleted");
            }
            return result;
        }

        public  async Task<List<Loan_Roi_Template>> GetLoanRateOfInterestListAsync(int schemeId)
        {
            return await  _unitOfWork.LoanROITemplate.GetLoanRateOfInterestListAsync(schemeId);
        }

        public async Task<LoanROIAndPIVM> GetLoanROIAndPIFromTemplateAsync(int schemeId, string agency, DateTime wef)
        {
            return await _unitOfWork.LoanROITemplate.GetLoanROIAndPIFromTemplateAsync(schemeId, agency, wef);
        }
    }
}
