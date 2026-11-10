using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class MemCollectionPriorityHandler : IMemCollectionPriorityHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemCollectionPriorityHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
