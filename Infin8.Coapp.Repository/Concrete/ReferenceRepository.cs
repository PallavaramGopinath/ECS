using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;

namespace Infin8.Coapp.Repository
{
    public class ReferenceRepository : Repository<Refer_Data>, IReferenceRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReferenceRepository(CSISContext context) : base(context)
        {
        }

        public async Task<List<DropdownItem>> GetReferenceItems(int refType, string brCode, bool factoryRec)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = await  (from r in CSISContext.Refer_Data
                           where r.ReferType == refType & r.BrCode == brCode && r.ReferFactoryRecovery == factoryRec
                           select new DropdownItem
                           {
                               Value = r.ReferId.ToString(),
                               Text = r.ReferName
                           }).ToListAsync();
                if (data != null)
                {
                    items = data.ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return items;
        }
        public async Task<List<Refer_Data>> AddReference(Refer_Data referData)
        {
            List<Refer_Data> references = new();
            try
            {
                decimal maxId = await CSISContext.Refer_Data.Where(x=> x.BrCode == referData.BrCode ).MaxAsync(x => x.ReferId);
                maxId++;
                referData.ReferId = maxId;
                await AddAsync(referData);
                var response = await CSISContext.Refer_Data.Where(x => x.BrCode == referData.BrCode ).ToListAsync();
                if (response != null && response.Count >0)
                {
                    references = response.ToList();
                }
            }
            catch (Exception ex)
            {
                references = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new reference name");
            }
            return references;
        }
        public async Task<List<Refer_Data>> EditReference(Refer_Data referData)
        {
            List<Refer_Data> references = new();
            try
            {
                await EditAsync(referData);
                var response = await CSISContext.Refer_Data.Where(x => x.BrCode == referData.BrCode ).ToListAsync();
                if (response != null && response.Count > 0)
                {
                    references = response.ToList();
                }
            }
            catch (Exception ex)
            {
                references = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying reference data");
            }
            return references;
        }

        public async Task<List<Refer_Data>> GetReferences(string brCode)
        {
            List<Refer_Data> references = new();
            try
            {
                var response = await CSISContext.Refer_Data.Where(x => x.BrCode == brCode).ToListAsync();
                if (response != null && response.Count > 0)
                {
                    references = response.ToList();
                }
            }
            catch (Exception)
            {
                references = new();
                throw;
            }
            return references;
        }
    }
}
