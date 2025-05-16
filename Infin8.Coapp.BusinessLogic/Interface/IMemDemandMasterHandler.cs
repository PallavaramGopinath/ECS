using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IMemDemandMasterHandler
    {
        Task<bool> AddMemDemandMasterAsync(Mem_Demand_Master memDemandMaster);
        Task<bool> EditMemDemandMasterAsync(Mem_Demand_Master memDemandMaster);
    }
}
