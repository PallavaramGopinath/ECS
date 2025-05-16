using Infin8.Coapp.Models;
using Infin8.Coapp.Dto;
using Microsoft.EntityFrameworkCore;
namespace Infin8.Coapp.Repository
{
    public  class AreaMasterRepository : Repository<Refer_Area>, IAreaMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public AreaMasterRepository(CSISContext context) : base(context)
        {
        }
        public bool AddArea(Refer_Area area)
        {
            Add(area);
            return true;
        }
        public bool EditArea(Refer_Area area)
        {
            Edit(area);
            return true;
        }
        public async Task<List<DropdownItem>> GetAreaItems(string brCode)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = await (from r in CSISContext.Refer_Area
                           where r.BrCode == brCode
                           select new DropdownItem
                           {
                               Value = r.Area_Id.ToString(),
                               Text = r.Area_Name
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
    }
}

