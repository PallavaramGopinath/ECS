using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class MemDemandStatusHandler : IMemDemandStatusHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemDemandStatusHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<bool> AddMemberDemandStatus(List<Mem_Demand_Status> memDemandSatusList)
        {
            throw new NotImplementedException();
        }
    }
}
