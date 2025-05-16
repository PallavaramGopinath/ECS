using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class ReferenceConstituencyRepository : Repository<Refer_Constituency>, IReferenceConstituencyRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReferenceConstituencyRepository(CSISContext context) : base(context)
        {
        }

        public async Task<bool> AddConstituency(Refer_Constituency refer_Constituency)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.Refer_Constituency.MaxAsync(x => x.id);
                maxId++;
                refer_Constituency.id = maxId;
                await AddAsync(refer_Constituency);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new constituency");
            }

            return result;
        }

        public async Task<bool> EditConstituency(Refer_Constituency refer_Constituency)
        {
            bool result = false;
            try
            {
                await EditAsync(refer_Constituency);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying constituency");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetConstituencyItems()
        {
            List<DropdownItem> items = new List<DropdownItem>();
            try
            {
                var data = await(from r in CSISContext.Refer_Constituency
                                 select new DropdownItem
                                 {
                                     Value = r.id.ToString(),
                                     Text = r.constituency
                                 }).ToListAsync();
                if (data != null)
                {
                    items = data.ToList();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching constituency");
            }
            return items;
        }
    }
}
