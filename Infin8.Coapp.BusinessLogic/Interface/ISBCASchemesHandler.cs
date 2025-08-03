using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ISBCASchemesHandler
    {
        Task<bool> AddSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode);
        Task<bool> EditSBCASchemesAsync(SBCA_Schemes sbcaSchemes, string brCode);
        Task<(decimal prlLedId, decimal intledId)> GetSBAccountLedgerIds(string brCode);
        Task<SBCA_Schemes> GetSBCAScheme(string brCode);
    }
}
