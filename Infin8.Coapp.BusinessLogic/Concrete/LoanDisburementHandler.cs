using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanDisburementHandler : ILoanDisburementHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanDisburementHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanDisbursementAsync(Loan_Disb loanDisb)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanDisbursement.AddLoanDisbursementAsync(loanDisb);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result=false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan disbursement not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanDisbursementAsync(Loan_Disb loanDisb)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanDisbursement.EditLoanDisbursementAsync(loanDisb);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan disbursement not deleted");
            }
            return result;
        }
    }
}
