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
    public class LoanMasterRepository : Repository<Loan_Master>, ILoanMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanMasterRepository(DbContext context) : base(context)
        {
        }
        public async Task<bool> AddLoanMasterAsync(Loan_Master loanMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Loan_Master.MaxAsync(x => x.Loan_Id);
                maxId++;
                loanMaster.Loan_Id = maxId;
                await AddAsync(loanMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Master not saved");
            }

            return result;
        }

        public async Task<bool> EditLoanMasterAsync(Loan_Master loanMaster)
        {
            bool result = false;
            try
            {
                await EditAsync(loanMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Master not modified");
            }
            return result;
        }

        public async Task<bool> IsLoanSchemeReferedInLoanMaster(int schemeId)
        {
            bool result = false;
            try
            {
                int count = await CSISContext.Loan_Master.Where(x => x.Scheme_Id == schemeId && x.Loan_Delete == false).CountAsync();
                if (count > 0) result  = true;
                else result = false;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Master not modified");
            }
            return result;
        }
    }
}
