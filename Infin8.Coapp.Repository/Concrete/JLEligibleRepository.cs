using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class JLEligibleRepository: Repository<JL_LoanEligible>, IJLEligibleRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public JLEligibleRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddJLEligibleAsync(JL_LoanEligible jLLoanEligible)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.JL_LoanEligible.MaxAsync(x => x.Id);
                maxId++;
                jLLoanEligible.Id = maxId;
                await AddAsync(jLLoanEligible);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan eligible not saved");
            }

            return result;
        }

        public async Task<bool> EditJLEligibleAsync(JL_LoanEligible jLLoanEligible)
        {
            bool result = false;
            try
            {
                jLLoanEligible.Eligible_Delete = false;
                await EditAsync(jLLoanEligible);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan eligible not deleted");
            }
            return result;
        }

        public async Task<double> GetJLEligiblePercentageAsync()
        {
            double eligiblePercentage = 0;
            try
            {
                var maxWef = await  CSISContext.JL_LoanEligible.MaxAsync(le => le.Wef);
                var percentage = await  CSISContext.JL_LoanEligible
                       .Where(le => le.Wef == maxWef)
                       .Select(le => le.EligiblePercentage)
                       .FirstOrDefaultAsync();
                if(percentage >0) eligiblePercentage=percentage;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching jewel loan eligible percentage");
            }
            return eligiblePercentage;
        }
    }
}
