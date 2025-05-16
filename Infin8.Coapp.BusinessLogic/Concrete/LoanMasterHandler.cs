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
    public class LoanMasterHandler : ILoanMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanMasterAsync(Loan_Master loanMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanMaster.AddLoanMasterAsync(loanMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan master not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanMasterAsync(Loan_Master loanMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanMaster.AddLoanMasterAsync(loanMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan master not deleted");
            }
            return result;
        }

        public async Task<bool> IsLoanSchemeReferedInLoanMaster(int schemeId)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanMaster.IsLoanSchemeReferedInLoanMaster(schemeId);
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! to find this scheme refered in loan master");
            }
            return result;
        }
    }
}
