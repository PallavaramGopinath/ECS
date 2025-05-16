using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LoanROIHandler : ILoanROIHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanROIHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanROIAsync(Loan_Roi loanRoi)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanROI.AddLoanROIAsync(loanRoi);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest not saved");
            }
            return result;
        }

        public async Task<bool> AddLoanRoiListAsync(List<Loan_Roi> loanRoiList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanROI.AddLoanRoiListAsync(loanRoiList);   
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest list not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanROIAsync(Loan_Roi loanRoi)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanROI.EditLoanROIAsync(loanRoi);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interes not deleted");
            }
            return result;
        }

        public async Task<bool> EditLoanRoiListAsync(List<Loan_Roi> loanRoiList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanROI.EditLoanRoiListAsync(loanRoiList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan rate of interest list not deleted");
            }
            return result;
        }
    }
}
