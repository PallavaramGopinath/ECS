using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository 
{
    public class MapBanksRepository : Repository<Map_Banks>, IMapBanksRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MapBanksRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddMapBanksAsync(Map_Banks mapBanks)
        {
            bool result = false;
            try
            {
                await AddAsync(mapBanks);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Mapping Bank ledger not saved");
            }

            return result;
        }

        public async Task<bool> DeleteMapBanksAsync(Map_Banks mapBanks)
        {
            bool result = false;
            try
            {
                await DeleteAsync(mapBanks);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Bank mapping ledger not deleted");
            }
            return result;
        }
    }
}
