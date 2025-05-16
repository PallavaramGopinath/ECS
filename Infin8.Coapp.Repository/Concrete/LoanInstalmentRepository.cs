
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    
    public class LoanInstalmentRepository: Repository<Loan_Inst>, ILoanInstalmentRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanInstalmentRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLoanInstalmentAsync(Loan_Inst loanInst)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Loan_Inst.MaxAsync(x => x.Inst_Id);
                maxId++;
                loanInst.Inst_Id = maxId;
                await AddAsync(loanInst);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Instalment not saved");
            }

            return result;
        }

        public  async Task<bool> EditLoanInstalmentAsync(Loan_Inst loanInst)
        {
            bool result = false;
            try
            {
                await EditAsync(loanInst);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan Instalment not modified");
            }
            return result;
        }
    }
}
