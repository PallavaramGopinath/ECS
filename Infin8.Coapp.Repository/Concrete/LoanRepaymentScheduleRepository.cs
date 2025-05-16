using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LoanRepaymentScheduleRepository : Repository<Loan_Repayment_Schedule>, ILoanRepaymentScheduleRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanRepaymentScheduleRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLoanRepaymentScheduleAsync(List<Loan_Repayment_Schedule> loanRepaymentScheduleList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach(var loan in loanRepaymentScheduleList)
                {
                    maxId = await CSISContext.Loan_Repayment_Schedule.MaxAsync(x => x.Dem_Id);
                    maxId++;
                    loan.Dem_Id = maxId;
                    await AddAsync(loan);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan repayment schedule list not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanRepaymentScheduleAsync(List<Loan_Repayment_Schedule> loanRepaymentScheduleList)
        {
            bool result = false;
            try
            {
                foreach (var loan in loanRepaymentScheduleList)
                {
                    loan.Demand_Delete = false;
                    await EditAsync(loan);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan repayment schedule list not modified");
            }
            return result;
        }
    }
}
