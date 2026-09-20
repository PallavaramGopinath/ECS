using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  class MemDemandStopRepository : Repository<Mem_Demand_Stop>, IMemDemandStopRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemDemandStopRepository(DbContext context) : base(context)
        {
        }

        public Task<List<Mem_Demand_Stop>> AddMemDemandAsync(List<Mem_Demand_Stop> memDemandStopList)
        {
            throw new NotImplementedException();
        }

        public Task<Mem_Demand_Stop> EditMemDemandAsync(Mem_Demand_Stop memDemandStop)
        {
            throw new NotImplementedException();
        }

        public Task<List<Mem_Demand_Stop>> GetMemDemandStopListAsync(DateOnly dueDate, string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
