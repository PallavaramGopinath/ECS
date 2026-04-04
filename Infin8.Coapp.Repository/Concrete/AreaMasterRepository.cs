using Infin8.Coapp.Models;
using Infin8.Coapp.Dto;
using Microsoft.EntityFrameworkCore;
namespace Infin8.Coapp.Repository
{
    public class AreaMasterRepository : Repository<Refer_Area>, IAreaMasterRepository
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
            await CSISContext.SaveChangesAsync();
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
                var response = await CSISContext.Refer_Area.Where(r => r.BrCode == brCode).ToListAsync();
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

        public async Task<List<DtoReferArea>> GetAreasWithTalukDistrictNames(string brCode)
        {
            List<DtoReferArea> areas = new();
            try
            {
                var response = await (from a in CSISContext.Refer_Area
                                      join t in CSISContext.Refer_Taluk on a.Taluk_Id equals t.Taluk_Id
                                      join d in CSISContext.Refer_District on a.District_Id equals d.District_Id
                                      where a.BrCode == brCode
                                      select new DtoReferArea
                                      {
                                          Area_Id = a.Area_Id,
                                          Area_Name = a.Area_Name,
                                          Area_Notes = a.Area_Notes,
                                          Taluk_Id = a.Taluk_Id,
                                          Taluk_Name = t.Taluk_Name,
                                          District_Id = a.District_Id,
                                          District_Name = d.District_Name,
                                          BrCode = a.BrCode,
                                          Area_Delete = a.Area_Delete
                                      }).ToListAsync();
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

        public async Task<List<Refer_Taluk>> GetTaluks(string brCode)
        {
            List<Refer_Taluk> taluks = new();
            try
            {
                var response = await CSISContext.Refer_Taluk.Where(r => r.BrCode == brCode).ToListAsync();
                if (response != null && response.Count > 0)
                {
                    taluks = response.ToList();
                }
            }
            catch (Exception ex)
            {
                taluks = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching taluk list");
            }
            return taluks;
        }

        public async Task<List<Refer_District>> GetDistricts(string brCode)
        {
            List<Refer_District> districts = new();
            try
            {
                var response = await CSISContext.Refer_District.Where(r => r.BrCode == brCode).ToListAsync();
                if (response != null && response.Count > 0)
                {
                    districts = response.ToList();
                }
            }
            catch (Exception ex)
            {
                districts = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching district list");
            }
            return districts;
        }
    }
}

