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
        public async Task<bool> AddReference(Refer_Data referData,string brCode)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Refer_Data.Where(x=> x.BrCode == brCode).MaxAsync(x => x.ReferId);
                maxId++;
                referData.ReferId = maxId;
                await AddAsync(referData);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new reference name");
            }

            return result;
        }
        public async Task<bool> EditReference(Refer_Data referData)
        {
            bool result = false;
            try
            {
                await EditAsync(referData);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying reference data");
            }
            return result;
        }
    }
}
