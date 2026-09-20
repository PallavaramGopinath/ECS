using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IDepositTrnRepositoty
    {
        Task<(bool result, decimal depositTrnId, string depositTrnNo)> AddDepositTrnAsync(Deposit_Trn depositTrn);
        Task<bool> EditDepositTrnAsync(Deposit_Trn depositTrn);
        Task<List<Deposit_Trn>> GetDepositTrns(decimal memId, string brCode);

        //Task<List<Mem_Demand>> GetDepositDemand(DateTime dueDate,string brCode);

        Task<List<Mem_Demand>> CalculateDepositDemand(decimal memId, DateOnly demandCalcDate, DateOnly demandDate, double memBasicPay, string brCode);
    }
}
