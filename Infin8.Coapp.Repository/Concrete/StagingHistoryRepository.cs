using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class StagingHistoryRepository : Repository<Staging_History>, IStagingHistoryRepository 
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public StagingHistoryRepository(DbContext context) : base(context)
        {
                
        }

        public async Task<bool> AddStagingHistoryList(List<Staging_History> stagingHistoryList)
        {
            bool result = false;
            decimal maxId = 0;
            try
            {
                foreach (var staging in stagingHistoryList)
                {
                    // Step 2: Get the maximum ID from staging_history to generate new IDs
                    maxId = await CSISContext.Staging_History.AnyAsync()
                        ? await CSISContext.Staging_History.MaxAsync(sh => sh.Id)
                        : 0;
                    maxId++;
                    staging.Id = maxId;
                    await AddAsync(staging);
                    CSISContext.SaveChanges();
                    result = true;
                }

            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return result;
        }
    }
}
