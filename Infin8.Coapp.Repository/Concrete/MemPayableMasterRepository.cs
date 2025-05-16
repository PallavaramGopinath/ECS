using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class MemPayableMasterRepository : Repository<Mem_Payable_Master>, IMemPayableMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemPayableMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMemPayableMasterAsync(Mem_Payable_Master memPayableMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Mem_Payable_Master.MaxAsync(x => x.PbleMaster_Id);
                maxId++;
                memPayableMaster.PbleMaster_Id = maxId;
                await AddAsync(memPayableMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend calculation master not saved");
            }
            return result;
        }

        public async Task<bool> EditMemPayableMasterAsync(Mem_Payable_Master memPayableMaster)
        {
            bool result = false;
            try
            {
                memPayableMaster.Master_Delete = true;
                await EditAsync(memPayableMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Dividend calculation master not modified");
            }
            return result;
        }
    }
}
