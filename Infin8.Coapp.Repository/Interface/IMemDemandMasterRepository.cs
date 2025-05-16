using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMemDemandMasterRepository
    {
        Task<bool> AddMemDemandMasterAsync(Mem_Demand_Master memDemandMaster);
        Task<bool> EditMemDemandMasterAsync(Mem_Demand_Master memDemandMaster);
    }
}
