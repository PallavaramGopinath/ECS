using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    internal class SBCASchemesRepository : Repository<SBCA_Schemes>, ISBCASchemesRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public SBCASchemesRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddSBCASchemesAsync(SBCA_Schemes sbcaSchemes,string brCode)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.SBCA_Schemes.Where(x=> x.BrCode == brCode).MaxAsync(x => x.Scheme_Id);
                maxId++;
                sbcaSchemes.Scheme_Id = maxId;
                await AddAsync(sbcaSchemes);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new SB Account scheme");
            }
            return result;
        }

        public async Task<bool> EditSBCASchemesAsync(SBCA_Schemes sbcaSchemes,string brCode)
        {
            bool result = false;
            try
            {
                await EditAsync(sbcaSchemes);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying SB account scheme");
            }
            return result;
        }

        public async Task<(decimal prlLedId, decimal intledId)> GetSBAccountLedgerIds(string brCode)
        {
            decimal prlLedId = 0;
            decimal intledId = 0;
            try
            {
                var sbScheme = await CSISContext.SBCA_Schemes.Where(x => x.Scheme_Id == 7).FirstOrDefaultAsync();
                if (sbScheme != null) 
                {
                    prlLedId = sbScheme.SBCA_Led_Id;
                    intledId = sbScheme.SBCA_Int_Led_Id;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching sb account led id, interest led id");
            }
            return (prlLedId, intledId);    
        }
    }
}
