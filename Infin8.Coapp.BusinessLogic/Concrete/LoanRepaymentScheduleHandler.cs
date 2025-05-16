using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class LoanRepaymentScheduleHandler : ILoanRepaymentScheduleHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LoanRepaymentScheduleHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLoanRepaymentScheduleAsync(List<Loan_Repayment_Schedule> loanRepaymentScheduleList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanRepaymentSchedule.AddLoanRepaymentScheduleAsync(loanRepaymentScheduleList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan repayment schedule not saved");
            }
            return result;
        }

        public async  Task<bool> EditLoanRepaymentScheduleAsync(List<Loan_Repayment_Schedule> loanRepaymentScheduleList)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.LoanRepaymentSchedule.EditLoanRepaymentScheduleAsync(loanRepaymentScheduleList);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan repayment schedule not deleted");
            }
            return result;
        }
    }
}
