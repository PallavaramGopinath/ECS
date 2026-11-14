using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  class DepositTrnRepository(DbContext context) : Repository<DepositTrnRepository>(context), IDepositTrnRepositoty
    {
        public CSISContext CSISContext => (CSISContext)Context;

        public Task<(bool result, decimal depositTrnId, string depositTrnNo)> AddDepositTrnAsync(Deposit_Trn depositTrn)
        {
            throw new NotImplementedException();
        }
        public Task<bool> EditDepositTrnAsync(Deposit_Trn depositTrn)
        {
            throw new NotImplementedException();
        }
        public Task<List<Deposit_Trn>> GetDepositTrns(decimal memId, string brCode)
        {
            throw new NotImplementedException();
        }

        //public Task<List<Mem_Demand>> GetDepositDemand(DateTime dueDate, string brCode)
        //{
        //    throw new NotImplementedException();
        //}

    }
}
