using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class TermDepositMemberRepository : Repository<TermDeposit_Members>, ITermDepositMemberRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositMemberRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositMemberAsync(TermDeposit_Members termDepositMember)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_Members.MaxAsync(x => x.TDMem_Id);
                maxId++;
                termDepositMember.TDMem_Id = maxId;
                await AddAsync(termDepositMember);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while saving Term deposit member");
            }
            return result;
        }

        public async Task<bool> EditTermDepositMemberAsync(TermDeposit_Members termDepositMember)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(termDepositMember);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error accurred while deleting Term deposit member");
            }
            return result;
        }
    }
}
