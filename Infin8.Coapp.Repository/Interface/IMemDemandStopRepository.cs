using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IMemDemandStopRepository
    {
        Task<List<Mem_Demand_Stop>> AddMemDemandAsync(List<Mem_Demand_Stop> memDemandStopList);
        Task<Mem_Demand_Stop> EditMemDemandAsync(Mem_Demand_Stop memDemandStop);
        Task<List<Mem_Demand_Stop>> GetMemDemandStopListAsync( DateOnly dueDate, string brCode);
    }
}
