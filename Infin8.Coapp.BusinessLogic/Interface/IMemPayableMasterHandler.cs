using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IMemPayableMasterHandler
    {
        Task<bool> AddMemPayableMasterAsync(Mem_Payable_Master memPayableMaster);
        Task<bool> EditMemPayableMasterAsync(Mem_Payable_Master memPayableMaster);
        Task<Mem_Payable_Master> GetDividendLastCalculatedData(int pbleType, string status, string brCode);
    }
}
