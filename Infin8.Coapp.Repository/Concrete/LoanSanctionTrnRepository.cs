using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  class LoanSanctionTrnRepository : Repository<Loan_Sanction_Trn>, ILoanSanctionTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanSanctionTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLoanSanctionTrnAsync(List<Loan_Sanction_Trn> loanSanctionTrnList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var sanction in loanSanctionTrnList) 
                {
                    maxId = await CSISContext.Loan_Sanction_Trn.MaxAsync(x => x.LoanSanctionTr_Id);
                    maxId++;
                    sanction.LoanSanctionTr_Id = maxId;
                    await AddAsync(sanction);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanction trn not saved");
            }
            return result;
        }

        public async Task<bool> EditLoanSanctionTrnAsync(List<Loan_Sanction_Trn> loanSanctionTrnList)
        {
            bool result = false;
            try
            {
                foreach(var sanction in loanSanctionTrnList)
                {
                    sanction.LoanSanctionTr_Delete=true;
                    await AddAsync(sanction);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan sanctin trn not deleted");
            }
            return result;
        }
    }
}
