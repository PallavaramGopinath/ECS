using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class JLDailyMarketRateRepository : Repository<JL_DailyMarketRate>, IJLDailyMarketRateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public JLDailyMarketRateRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddDailyMarketRateAsync(JL_DailyMarketRate jLDailyMarketRate)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.JL_DailyMarketRate.MaxAsync(x => x.MarketRate_Id);
                maxId++;
                jLDailyMarketRate.MarketRate_Id = maxId;
                await AddAsync(jLDailyMarketRate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan daily market rate of interest not saved");
            }

            return result;
        }

        public async Task<bool> EditDailyMarketRateAsync(JL_DailyMarketRate jLDailyMarketRate)
        {
            bool result = false;
            try
            {
                jLDailyMarketRate.MarketRate_Delete = false;
                await EditAsync(jLDailyMarketRate);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan daily market rate not deleted");
            }
            return result;
        }

        public async Task<double> GetMarketRate()
        {
            double marketRate = 0;
            try
            {
                // 1. Find the maximum jl_Id among non-deleted records
                var maxId = CSISContext.JL_DailyMarketRate
                                   .Where(jl => jl.MarketRate_Delete == false) // Corresponds to WHERE JL_Delete = 0
                                   .Max(jl => jl.MarketRate_Id);          // Corresponds to max(jl_Id)

                // 2. Find the single record with that jl_Id
                marketRate = await  CSISContext.JL_DailyMarketRate
                                    .Where(jl => jl.MarketRate_Id == maxId)
                                    .Select(jl => jl.MarketRate)
                                    .FirstOrDefaultAsync(); // Corresponds to SELECT * ... WHERE JL_Id = ... .FirstOrDefault()
            }
            catch (Exception)
            {

                throw;
            }
            return marketRate;
        }
    }
}
