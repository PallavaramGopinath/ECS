using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LoanSanctionRepository : Repository<Loan_Sanction>, ILoanSanctionRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanSanctionRepository(DbContext context) : base(context)
        {
        }
        public async Task<bool> AddLoanSanctionAsync(Loan_Sanction loanSanction)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Loan_Sanction.MaxAsync(x => x.LoanSanction_Id);
                maxId++;
                loanSanction.LoanSanction_Id = maxId;
                await AddAsync(loanSanction);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction not saved");
            }

            return result;
        }

        public async Task<bool> EditLoanSanctionAsync(Loan_Sanction loanSanction)
        {
            bool result = false;
            try
            {
                loanSanction.LoanSanction_Delete = true;
                await EditAsync(loanSanction);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction not modified");
            }
            return result;
        }
    }
}
