using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.CodeAnalysis.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class RecurringDepositHandler : IRecurringDepositHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public RecurringDepositHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Mem_Demand>> CalculateRecurringDepositDemand(decimal memId, DateOnly demandCalcDate, DateTime demandDate, string brCode)
        {
           return await _unitOfWork.RecurringDeposit.CalculateRecurringDepositDemand(memId, demandCalcDate, demandDate, brCode);
        }
    }
}
