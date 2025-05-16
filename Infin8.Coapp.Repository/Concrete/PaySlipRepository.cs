using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PaySlipRepository :Repository<Pay_Slip>, IPaySlipRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PaySlipRepository(DbContext context) : base(context)
        {
        }

        public async  Task<bool> AddPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Slip.MaxAsync(x => x.Pay_Id);
                maxId++;
                paySlip.Pay_Id = maxId;
                await AddAsync(paySlip);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Pay slip not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                paySlip.Pay_Delete = true;
                await EditAsync(paySlip);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Pay slip not modified");
            }
            return result;
        }
    }
}
