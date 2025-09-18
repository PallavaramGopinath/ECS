using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    internal class LoanMemberRepository : Repository<Loan_Members>, ILoanMemberRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LoanMemberRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddLoanMemberAsync(List<Loan_Members> loanMemberList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var member in loanMemberList) 
                {
                    //maxId = await CSISContext.Loan_Members.MaxAsync(x => x.LoanMem_Id);
                    maxId = await CSISContext.Loan_Members.AnyAsync()
                   ? await CSISContext.Loan_Members.MaxAsync(sh => sh.LoanMem_Id)
                   : 0;
                    maxId++;
                    member.LoanMem_Id = maxId;
                    await AddAsync(member);
                    await CSISContext.SaveChangesAsync();
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan member not saved");
            }
            
            return result;
        }

        public async Task<bool> EditLoanMemberAsync(List<Loan_Members> loanMemberList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                /// delete the existing members
                foreach(var member in loanMemberList)
                {
                    member.Mem_Delete = true;
                    await EditAsync(member);
                }
                /// add new list of members
                foreach (var member in loanMemberList)
                {
                    maxId = await CSISContext.Loan_Members.MaxAsync(x => x.LoanMem_Id);
                    member.LoanMem_Id = maxId;
                    await AddAsync(member);
                }
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Loan members not modified");
            }
            return result;
        }
    }
}
