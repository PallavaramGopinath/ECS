using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LoanSanctionHandler : ILoanSanctionHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanSanctionHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanSanction(Loan_Sanction loanSanction)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanSanction.AddLoanSanctionAsync(loanSanction);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanSanction(Loan_Sanction loanSanction)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanSanction.EditLoanSanctionAsync(loanSanction);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction not deleted");
            }
            return result;
        }
    }
}
