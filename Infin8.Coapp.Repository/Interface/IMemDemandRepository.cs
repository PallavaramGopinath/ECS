using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IMemDemandRepository
    {
        Task<bool>AddMemDemandAsync(List<Mem_Demand> memDemands);
    }
}
