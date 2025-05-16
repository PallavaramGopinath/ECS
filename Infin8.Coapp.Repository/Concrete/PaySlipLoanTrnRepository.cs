using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PaySlipLoanTrnRepository : Repository<Pay_Slip_Loan_Trn>, IPaySlipLoanTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PaySlipLoanTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Slip_Loan_Trn.MaxAsync(x => x.Tr_Id);
                maxId++;
                paySlipLoanTrn.Tr_Id = maxId;
                await AddAsync(paySlipLoanTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Staff loan recovery in payroll not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipLoanTrnAsync(Pay_Slip_Loan_Trn paySlipLoanTrn)
        {
            bool result = false;
            try
            {
                paySlipLoanTrn.LoanTr_Delete = true;
                await EditAsync(paySlipLoanTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Staff loan recovery in payroll not modified");
            }
            return result;
        }
    }
}
