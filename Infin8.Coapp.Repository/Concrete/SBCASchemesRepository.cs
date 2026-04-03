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

        public async Task<List<SBCA_Schemes>> AddSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode)
        {
            List<SBCA_Schemes> schemes = new();
            try
            {
                //var  maxId = await CSISContext.SBCA_Schemes.Where(x=> x.BrCode == brCode).MaxAsync(x => x.Scheme_Id);
                var maxId = await CSISContext.SBCA_Schemes
                .Where(x => x.BrCode == brCode)
                .Select(x => (int?)x.Scheme_Id)  // Cast to nullable int?
                .MaxAsync() ?? 0;
                maxId++;
                sbcaSchemes.Scheme_Id = maxId;
                await AddAsync(sbcaSchemes);
                var response = await CSISContext.SBCA_Schemes.Where(x => x.BrCode == brCode).ToListAsync();
                if (response != null && response.Count > 0)
                {
                    schemes = response.ToList();
                }
            }
            catch (Exception ex)
            {
                schemes = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding new SB Account scheme");
            }
            return schemes;
        }

        public async Task<List<SBCA_Schemes>> EditSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode)
        {
            List<SBCA_Schemes> schemes = new();
            try
            {
                await EditAsync(sbcaSchemes);
                var response = await CSISContext.SBCA_Schemes.Where(x => x.BrCode == brCode).ToListAsync();
                if (response != null && response.Count > 0)
                {
                    schemes = response.ToList();
                }
            }
            catch (Exception ex)
            {
                schemes = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying SB account scheme");
            }
            return schemes;
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

        public async Task<SBCA_Schemes> GetSBCAScheme(string brCode)
        {
            SBCA_Schemes sbcaScheme = new();
            try
            {
                var scheme = await CSISContext.SBCA_Schemes
                    .Where(x => x.BrCode == brCode)
                    .FirstOrDefaultAsync();
                if (scheme != null)
                {
                    sbcaScheme = scheme;
                }
            }
            catch (Exception)
            {
                sbcaScheme = new();
            }
            return sbcaScheme;
        }

        public async Task<List<SBCA_Schemes>> GetSBCASchemes(string brCode)
        {
            List<SBCA_Schemes> sbcaSchemes = new();
            try
            {
                var schemes = await CSISContext.SBCA_Schemes
                    .Where(x => x.BrCode == brCode)
                    .ToListAsync();
                if (schemes != null)
                {
                    sbcaSchemes = schemes.ToList();
                }
            }
            catch (Exception)
            {
                sbcaSchemes = new();
            }
            return sbcaSchemes;
        }
    }
}
