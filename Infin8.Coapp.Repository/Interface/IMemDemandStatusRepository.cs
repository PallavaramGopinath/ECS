using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IMemDemandStatusRepository
    {
        Task<bool> AddMemberDemandStatus(List<Mem_Demand_Status> memDemandSatusList);
    }
}
