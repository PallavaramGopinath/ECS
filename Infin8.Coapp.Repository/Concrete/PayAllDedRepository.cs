using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayAllDedRepository : Repository<Pay_All_Ded>, IPayAllDedRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayAllDedRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayAllDedAsync(Pay_All_Ded payAllDed)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_All_Ded.MaxAsync(x => x.Pay_Ad_Id);
                maxId++;
                payAllDed.Pay_Ad_Id  = maxId;
                await AddAsync(payAllDed);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Employees allowance and deductions data not saved");
            }
            return result;
        }

        public async Task<bool> EditPayAllDedAsync(Pay_All_Ded payAllDed)
        {
            bool result = false;
            try
            {
                payAllDed.All_Delete  = true;
                await EditAsync(payAllDed);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Employees allowance and deductions data not modified");
            }
            return result;
        }
    }
}
