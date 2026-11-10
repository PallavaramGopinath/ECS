using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public class MemDemandRepository : Repository<Mem_Demand>, IMemDemandRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemDemandRepository(DbContext context) : base(context)
        {
        }
        public Task<bool> AddMemDemandAsync(List<Mem_Demand> memDemands)
        {
            throw new NotImplementedException();
        }
    }
}
