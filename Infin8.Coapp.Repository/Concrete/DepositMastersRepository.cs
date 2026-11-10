using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public class DepositMastersRepository : Repository<DepositMastersRepository>, IDepositMastersRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public DepositMastersRepository(DbContext context) : base(context)
        {
            
        }
        public Task<(bool result, decimal depositId, string depositNo)> AddDepositMasterAsync(Deposit_Masters depositMaster)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditDepositMasterAsync(Deposit_Masters depositMaster)
        {
            throw new NotImplementedException();
        }

        public Task<List<Deposit_Masters>> GetDepositMasters()
        {
            throw new NotImplementedException();
        }
    }
}
