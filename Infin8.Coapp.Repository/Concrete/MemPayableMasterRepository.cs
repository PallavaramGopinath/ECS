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

        public async Task<Mem_Payable_Master> GetDividendLastCalculatedData(int pbleType, string status, string brCode)
        {
            Mem_Payable_Master master = new();
            var latestDate = CSISContext.Mem_Payable_Master
            .Where(m => !m.Master_Delete && m.Master_Status == status && m.PbleType == pbleType)
            .Max(m => m.FromDate);

            var query = await  CSISContext.Mem_Payable_Master
                .Where(m => !m.Master_Delete
                         && m.Master_Status == status
                         && m.PbleType == pbleType
                         && m.FromDate == latestDate)
                .Select(m => new Mem_Payable_Master 
                {   
                    FromDate =  m.FromDate, 
                    ToDate =  m.ToDate, 
                    ROI_Pble =  m.ROI_Pble, 
                    ROI_Trnble = m.ROI_Trnble,
                }).FirstOrDefaultAsync ();
            if (query != null && query.ROI_Pble > 0)
                master = query;
            return master;
        }
    }
}
