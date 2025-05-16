using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanInstalmentHandler : ILoanInstalmentHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanInstalmentHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanInstalmentAsync(Loan_Inst loanInst)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanInstalment.AddLoanInstalmentAsync(loanInst);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan instalment not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanInstalmentAsync(Loan_Inst loanInst)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanInstalment.EditLoanInstalmentAsync(loanInst);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan instalment not deleted");
            }
            return result;
        }
    }
}
