using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IMemCollectionPriorityHandler
    {
        Task<List<Mem_CollectionPriority>> AddMemCollectionPriorityAsync(Mem_CollectionPriority memCollectionPriority);
        Task<List<Mem_CollectionPriority>> EditMemCollectionPriorityAsync(Mem_CollectionPriority memCollectionPriority);
        Task<List<Mem_CollectionPriority>> GetMemCollectionPriorityAsync(string brCode);
    }
}
