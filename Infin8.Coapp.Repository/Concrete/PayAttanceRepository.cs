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
    }
}
