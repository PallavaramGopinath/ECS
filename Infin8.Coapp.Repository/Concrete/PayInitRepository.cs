using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayInitRepository : Repository<Pay_Init>, IPayInitRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayInitRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayInitAsync(Pay_Init payInit)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Init.MaxAsync(x => x.Pay_Id);
                maxId++;
                payInit.Pay_Id = maxId;
                await AddAsync(payInit);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Initialisation of payroll not saved");
            }
            return result;
        }

        public async Task<bool> EditPayInitAsync(Pay_Init payInit)
        {
            bool result = false;
            try
            {
                payInit.Pay_Delete = true;
                await EditAsync(payInit);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Initialisation of payroll not modified");
            }
            return result;
        }
    }
}
