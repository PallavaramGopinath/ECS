using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ISBCASchemesRepository
    {
        Task<List<SBCA_Schemes>> AddSBCASchemesAsync(SBCA_Schemes sbcaSchemes,string brCode);
        Task<List<SBCA_Schemes>> EditSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode);
        Task<(decimal prlLedId, decimal intledId)> GetSBAccountLedgerIds(string brCode);
        Task<SBCA_Schemes> GetSBCAScheme(string brCode);
        Task<List<SBCA_Schemes>> GetSBCASchemes(string brCode);
    }
}
