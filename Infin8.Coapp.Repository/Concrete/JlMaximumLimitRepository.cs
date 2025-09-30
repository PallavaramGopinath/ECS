using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class JlMaximumLimitRepository : Repository<JL_Max_Limit>, IJLMaximumLimitRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public JlMaximumLimitRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddJLMaximumLimitAsync(JL_Max_Limit jLMaxLimit)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.JL_Max_Limit.MaxAsync(x => x.JL_Max_Id);
                maxId++;
                jLMaxLimit.JL_Max_Id = maxId;
                await AddAsync(jLMaxLimit);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan maximum limit not saved");
            }
            return result;
        }

        public async Task<bool> EditJLMaximumLimitAsync(JL_Max_Limit jLMaxLimit)
        {
            bool result = false;
            try
            {
                jLMaxLimit.JL_Delete = true;
                await EditAsync(jLMaxLimit);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan maximum loan limit not deleted");
            }
            return result;
        }

        public async Task<List<JL_Max_Limit>> GetJLMaximumLimitListAsync()
        {
            List<JL_Max_Limit> list = new List<JL_Max_Limit>();
            try
            {
                var maxLimitList = await CSISContext.JL_Max_Limit.Where(x => x.JL_Delete == false).ToListAsync();
                if (maxLimitList != null && maxLimitList.Count > 0) list = maxLimitList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An errpr occurred while fething Jewel loan maximum limit List");
            }
            return list;
        }

        public async Task<double> GetJLMaximumLimitAsync(DateTime wef, string brCode)
        {
            double maximumLimit = 0;
            try
            {
                double jlMaxLimit = await CSISContext.JL_Max_Limit
                .Where(j => j.JL_Max_Id == CSISContext.JL_Max_Limit
                    .Where(j => j.JL_Delete == false && j.WithEffectFrom <= wef && j.BrCode == brCode)
                    .Max(j => j.JL_Max_Id))
                .Select(j => j.Max_Loan_Limit)
                .FirstOrDefaultAsync();
                if(jlMaxLimit > 0) maximumLimit = jlMaxLimit;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An errpr occurred while fething Jewel loan maximum limit amount");
            }
            return maximumLimit;
        }
    }
}
