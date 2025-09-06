using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayAttanceRepository : Repository<Pay_Att>, IPayAttanceRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayAttanceRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddPayAttanceAsync(Pay_Att payAtt)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Att.MaxAsync(x => x.Att_Id);
                maxId++;
                payAtt.Att_Id = maxId;
                await AddAsync(payAtt);
                await CSISContext.SaveChangesAsync(); // Save changes to reflect in the database
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Attance of employee details not saved");
            }
            return result;
        }

        public async Task<bool> EditPayAttanceAsync(Pay_Att payAtt)
        {
            bool result = false;
            try
            {
                payAtt.Att_Delete  = true;
                await EditAsync(payAtt);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Attance of employee details not modified");
            }
            return result;
        }

        public async Task<Pay_Att> GetPayAttanceByEmpId(decimal empId, decimal payId, string brCode)
        {
            Pay_Att att = new();
            var result =  await CSISContext.Pay_Att.FirstOrDefaultAsync(x => x.Mem_Id == empId && x.Pay_Id == payId && x.BrCode == brCode && x.Att_Delete == false);
            if(result != null)
            {
                att = result;
            }
            else
            {
                att = new Pay_Att();
                att.Mem_Id = 0;
                throw new InvalidOperationException("No Data Found");
            }
            return att;
        }
    }
}
