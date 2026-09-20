using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    
    public  class MemDemandStatusRepository : Repository<Mem_Demand_Status>, IMemDemandStatusRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemDemandStatusRepository(DbContext context) : base(context)
        {

        }

        public Task<bool> AddMemberDemandStatus(List<Mem_Demand_Status> memDemandSatusList)
        {
            throw new NotImplementedException();
        }
    }
}
