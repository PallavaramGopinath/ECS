using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LoanDisbursementRepository : Repository<Loan_Disb>, ILoanDisbursementRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanDisbursementRepository(DbContext context) : base(context)
        {
        }
        
        public async Task<bool> AddLoanDisbursementAsync(Loan_Disb loanDisb)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Loan_Disb.MaxAsync(x => x.Disb_Id);
                maxId++;
                loanDisb.Disb_Id = maxId;
                await AddAsync(loanDisb);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Disburement not saved");
            }

            return result;
        }

        public async Task<bool> EditLoanDisbursementAsync(Loan_Disb loanDisb)
        {
            bool result = false;
            try
            {
                await EditAsync(loanDisb);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Disbursement not modified");
            }
            return result;
        }
    }
}
