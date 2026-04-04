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
        public async Task<List<Refer_Area>> AddArea(Refer_Area area)
        {
            List<Refer_Area> areas = new();
            decimal maxId = await CSISContext.Refer_Area.Where(x => x.BrCode == area.BrCode).MaxAsync(x => x.Area_Id);
            maxId++;
            area.Area_Id = maxId;
            await AddAsync(area);
            var response = await CSISContext.Refer_Area.Where(x => x.BrCode == area.BrCode).ToListAsync();
            if (response != null && response.Count > 0)
            {
                areas = response.ToList();
            }
            return areas;
        }
        public async Task<List<Refer_Area>> EditArea(Refer_Area area)
        { 
             List<Refer_Area> areas = new();
            await EditAsync(area);
            var response = await CSISContext.Refer_Area.Where(x => x.BrCode == area.BrCode).ToListAsync();
            if (response != null && response.Count > 0)
            {
                areas = response.ToList();
            }
            return areas;
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

        public async Task<List<Refer_Area>> GetAreas(string brCode)
        {
            List<Refer_Area> areas = new();
            try
            {
                var response = await CSISContext.Refer_Area.Where(r=> r.BrCode == brCode).ToListAsync ();
                if (response != null && response.Count > 0)
                {
                    areas = response.ToList();
                }
            }
            catch (Exception ex)
            {
                areas = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching area list");
            }
            return areas;
        }
    }
}

