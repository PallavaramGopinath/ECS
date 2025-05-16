using Infin8.Coapp.Repository;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LoanSchemeGroupHandler : ILoanSchemeGroupHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanSchemeGroupHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanSchemeGroup.AddLoanSchemeGroupAsync(loanSchemeGrp);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan scheme group not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanSchemeGroup.EditLoanSchemeGroupAsync(loanSchemeGrp);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan scheme group not deleted");
            }
            return result;
        }
    }
}
