using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanSanctionTrnHandler : ILoanSanctionTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanSanctionTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanSanctionTrnAsync(List<Loan_Sanction_Trn> loanSanctionTrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanSanctionTrn.AddLoanSanctionTrnAsync(loanSanctionTrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction trn list not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanSanctionTrnAsync(List<Loan_Sanction_Trn> loanSanctionTrnList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanSanctionTrn.EditLoanSanctionTrnAsync(loanSanctionTrnList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction trn list not deleted");
            }
            return result;
        }
    }
}
