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
    public  class MapSuspenseAccountsRepository : Repository<Map_SuspenseAccounts>, IMapSuspenseAccountsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MapSuspenseAccountsRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMapSuspenseAccountsAsync(Map_SuspenseAccounts mapSuspensAccounts)
        {
            bool result = false;
            try
            {
                await AddAsync(mapSuspensAccounts);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while Mapping of suspense accounts");
            }
            return result;
        }

        public async Task<bool> EditMapSuspenseAccountsAsync(Map_SuspenseAccounts mapSuspensAccounts)
        {
            bool result = false;
            try
            {
                await EditAsync(mapSuspensAccounts);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying Mapping of suspense accounts");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetSuspenseLedgerItemsBySupenseTypeAsync(int suspenseType,string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            try
            {
                var suspenseList = await (from mapSus in CSISContext.Map_SuspenseAccounts
                                          join ledger in CSISContext.Fin_Ledger
                                              on mapSus.Led_Id equals ledger.Led_Id
                                          where mapSus.Sus_Type == suspenseType && mapSus.BrCode == brCode && ledger.BrCode == brCode
                                          select new DropdownItem
                                          {
                                              Value = mapSus.Led_Id.ToString(),
                                              Text = ledger.Led_Name
                                          }).ToListAsync();
                if(suspenseList != null && suspenseList.Count > 0) list = suspenseList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching member suspense ledger items");
            }
            return list;
        }
    }
}
