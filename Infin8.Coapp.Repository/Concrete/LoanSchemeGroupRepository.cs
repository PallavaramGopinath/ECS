using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LoanSchemeGroupRepository :Repository<Loan_Schemes_Group>, ILoanSchemeGroupRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanSchemeGroupRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.Loan_Schemes_Group.MaxAsync(x => x.Grp_Id);
                maxId++;
                loanSchemeGrp.Grp_Id = maxId;
                await AddAsync(loanSchemeGrp);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan scheme group not saved");
            }

            return result;
        }

        public async Task<bool> EditLoanSchemeGroupAsync(Loan_Schemes_Group loanSchemeGrp)
        {
            bool result = false;
            try
            {
                await EditAsync(loanSchemeGrp);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan scheme group not modified");
            }
            return result;
        }
    }
}
