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

        public async Task<Mem_Payable_Master> GetMemPayableMasterByPbleMasterId(decimal pbleMasterId, string brCode)
        {
            Mem_Payable_Master master = new();

            var result = await CSISContext.Mem_Payable_Master.Where(x => x.PbleMaster_Id == pbleMasterId && x.BrCode == brCode && x.Master_Delete == false).FirstAsync();
            if (result != null && result.PbleMaster_Id > 0)
                master = result;
            return master;
        }
        public async Task<Mem_Payable_Master> GetDividendLastCalculatedData(int pbleType, string status, string brCode)
        {
            Mem_Payable_Master master = new();
            try
            {
                //    var latestDate = CSISContext.Mem_Payable_Master
                //.Where(m => !m.Master_Delete && m.Master_Status == status && m.PbleType == pbleType)
                //.Max(m => m.FromDate);
                var latestDate = CSISContext.Mem_Payable_Master
                .Where(m => !m.Master_Delete && m.Master_Status == status && m.PbleType == pbleType)
                .Select(m => (DateTime?)m.FromDate)  // Cast to nullable DateTime?
                .Max() ?? default(DateTime);

                var query = await CSISContext.Mem_Payable_Master
                    .Where(m => !m.Master_Delete
                             && m.Master_Status == status
                             && m.PbleType == pbleType
                             && m.FromDate == latestDate)
                    .Select(m => new Mem_Payable_Master
                    {
                        FromDate = m.FromDate,
                        ToDate = m.ToDate,
                        ROI_Pble = m.ROI_Pble,
                        ROI_Trnble = m.ROI_Trnble,
                    }).FirstOrDefaultAsync();
                if (query != null && query.ROI_Pble > 0)
                    master = query;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.Message + " Dividend calculation master not retrieved");
            }
            return master;
        }

        public async Task<List<Mem_Payable_Master>> GetCalculatedDataList(int pbleType, string status, string brCode)
        {
            List<Mem_Payable_Master> masterList = new();
            var result = await CSISContext.Mem_Payable_Master
                .Where(x => x.PbleType == pbleType && x.BrCode == brCode && x.Master_Delete == false)
                .OrderBy(x => x.FromDate).ToListAsync();
            if (result != null && result.Any()) masterList = result;
            return masterList;
        }


    }
}
