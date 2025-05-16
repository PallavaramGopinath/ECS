using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayRetirementRepository : Repository<Pay_Retirement> ,IPayRetirementRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayRetirementRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayRetirementAsync(Pay_Retirement payRetirement)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Retirement.MaxAsync(x => x.Ret_Id);
                maxId++;
                payRetirement.Ret_Id = maxId;
                await AddAsync(payRetirement);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Employee retirement data not saved");
            }
            return result;
        }

        public async Task<bool> EditPayRetirementAsync(Pay_Retirement payRetirement)
        {
            bool result = false;
            try
            {
                payRetirement.Ret_Delete = true;
                await EditAsync(payRetirement);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Employee retirement data  not modified");
            }
            return result;
        }
    }
}
