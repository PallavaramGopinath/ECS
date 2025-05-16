using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayBasicPayRepository  : Repository<Pay_BasicPay>, IPayBasicPayRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayBasicPayRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayBasicPayAsync(Pay_BasicPay payBasicPay)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_BasicPay.MaxAsync(x => x.BP_Id);
                maxId++;
                payBasicPay.BP_Id = maxId;
                await AddAsync(payBasicPay);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Basic pay data not saved");
            }
            return result;
        }

        public async Task<bool> EditPayBasicPayAsync(Pay_BasicPay payBasicPay)
        {
            bool result = false;
            try
            {
                payBasicPay.BP_Delete  = true;
                await EditAsync(payBasicPay);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Basic pay data not modified");
            }
            return result;
        }
    }
}
