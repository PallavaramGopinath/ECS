using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class MemDemandHandler : IMemDemandHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemDemandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<bool> AddMemDemandAsync(List<Mem_Demand> memDemands)
        {
            throw new NotImplementedException();
        }
    }
}
