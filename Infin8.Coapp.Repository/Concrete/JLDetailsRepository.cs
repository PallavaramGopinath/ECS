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
    public class JLDetailsRepository: Repository<JL_Details>, IJLDetailsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public JLDetailsRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddJLDetailsAsync(JL_Details jlDetails)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.JL_Details.MaxAsync(x => x.JL_Id);
                maxId++;
                jlDetails.JL_Id = maxId;
                await AddAsync(jlDetails);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan details not saved");
            }

            return result;
        }

        public async Task<bool> EditJLDetailsAsync(JL_Details jlDetails)
        {
            bool result = false;
            try
            {
                jlDetails.JL_Delete = true;
                await EditAsync(jlDetails);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan details not deleted");
            }
            return result;
        }
        public async Task<JewelLoanMarketRate> GetMarketRateAndAdoptedRateAsync(string brCode)
        {
            //double marketRate = 0;
            //double adoptedRate = 0;
            JewelLoanMarketRate marketRate = new JewelLoanMarketRate();
            try
            {
                // Fetch the JL_Details record asynchronously
                var jldetails = await CSISContext.JL_Details
                    .Where(j => j.JL_Id == CSISContext.JL_Details
                        .Where(j => j.JL_Delete == false && j.BrCode == brCode )
                        .Max(j => j.JL_Id))
                    .FirstOrDefaultAsync();

                // Check if the record was found
                if (jldetails != null)
                {
                    marketRate.Market_Rate = jldetails.MarketRatePerGram;
                    marketRate.Govt_Rate = jldetails.RatePerGram;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching jewel's market rate and adopted rate.");
            }

            // Return the values as a tuple
            return marketRate;

        }

    }
}
