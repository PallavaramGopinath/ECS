using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PayAllDedMasterRepository : Repository<Pay_All_Ded_Master>, IPayAllDedMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PayAllDedMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddAllDedMasterAsync(Pay_All_Ded_Master payAllDedMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_All_Ded_Master.MaxAsync(x => x.All_Id);
                maxId++;
                payAllDedMaster.All_Id  = maxId;
                await AddAsync(payAllDedMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Allowance and deductions master not saved");
            }
            return result;
        }

        public async Task<bool> EditAllDedMasterAsync(Pay_All_Ded_Master payAllDedMaster)
        {
            bool result = false;
            try
            {
                payAllDedMaster.All_Delete = true;
                await EditAsync(payAllDedMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Allowance and deductions master not modified");
            }
            return result;
        }
    }
}
