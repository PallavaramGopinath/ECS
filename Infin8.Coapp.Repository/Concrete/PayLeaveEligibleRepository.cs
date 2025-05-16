using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayLeaveEligibleRepository : Repository<Pay_Leave_Eligible>, IPayLeaveEligibleRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayLeaveEligibleRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayLeaveEligibleAsync(Pay_Leave_Eligible payLeaveEligible)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Leave_Eligible.MaxAsync(x => x.Lv_elg_Id);
                maxId++;
                payLeaveEligible.Lv_elg_Id = maxId;
                await AddAsync(payLeaveEligible);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Leave eligible data not saved");
            }
            return result;
        }

        public async Task<bool> EditPayLeaveEligibleAsync(Pay_Leave_Eligible payLeaveEligible)
        {
            bool result = false;
            try
            {
                payLeaveEligible.Lv_Delete = true;
                await EditAsync(payLeaveEligible);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Leave eligible data not modified");
            }
            return result;
        }
    }
}
