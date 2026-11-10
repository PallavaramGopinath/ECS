using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IRecurringDepositRepository
    {
        Task<List<Mem_Demand>> CalculateRecurringDepositDemand(decimal memId, DateOnly demandCalcDate, DateTime demandDate, string brCode);
    }
}
