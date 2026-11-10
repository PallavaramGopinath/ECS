using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    internal class MemCollectionPriorityRepository : Repository<Mem_CollectionPriority>, IMemCollectionPriorityRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public MemCollectionPriorityRepository(DbContext context) : base(context)
        {
        }

        public Task<List<Mem_CollectionPriority>> AddMemCollectionPriorityAsync(Mem_CollectionPriority memCollectionPriority)
        {
            throw new NotImplementedException();
        }

        public Task<List<Mem_CollectionPriority>> EditMemCollectionPriorityAsync(Mem_CollectionPriority memCollectionPriority)
        {
            throw new NotImplementedException();
        }

        public Task<List<Mem_CollectionPriority>> GetMemCollectionPriorityAsync(string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
