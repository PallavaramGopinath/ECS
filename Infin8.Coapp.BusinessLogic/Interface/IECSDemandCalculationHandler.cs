using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IECSDemandCalculationHandler
    {
        Task<List<Mem_Demand>> CalculateECSDemandAsync(DateTime demandCalculateDate, DateTime dueDate, int societyType, string brCode);
    }
}
