using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class MemDemandMasterRepository : Repository<Mem_Demand_Master>, IMemDemandMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemDemandMasterRepository(DbContext context) : base(context)
        {
        }

        public async  Task<bool> AddMemDemandMasterAsync(Mem_Demand_Master memDemandMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Demand_Master.MaxAsync(x => x.Demand_Id);
                maxId++;
                memDemandMaster.Demand_Id = maxId;
                await AddAsync(memDemandMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Member demand master not saved");
            }
            return result;
        }

        public async Task<bool> EditMemDemandMasterAsync(Mem_Demand_Master memDemandMaster)
        {
            bool result = false;
            try
            {
                memDemandMaster.DemandMaster_Delete = false;
                await EditAsync(memDemandMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Member demand master not modified");
            }
            return result;
        }
    }
}
