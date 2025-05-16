using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PaySlipLoanTrnHandler : IPaySlipLoanTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PaySlipLoanTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PaySlipLoanTrn.AddPaySlipLoanTrnAsync(paySlipLoanTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan deductions in payroll not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PaySlipLoanTrn.EditPaySlipLoanTrnAsync(paySlipLoanTrn);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan deductions in payroll not deleted");
            }
            return result;
        }
    }
}
