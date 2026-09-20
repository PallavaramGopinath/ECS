using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IMemDemandHandler
    {
        Task<bool> AddMemDemandAsync(List<Mem_Demand> memDemands);
    }
}
