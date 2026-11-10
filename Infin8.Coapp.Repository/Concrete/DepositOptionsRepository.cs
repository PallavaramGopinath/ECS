using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  class DepositOptionsRepository : Repository<DepositOptionsRepository>, IDepositOptionsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public DepositOptionsRepository(DbContext context) : base(context)
        {
        }
        public Task<(bool result, decimal depositOptionId, string depositOptionNo)> AddDepositOptionsAsync(Deposit_Options depositOptions)
        {
            throw new NotImplementedException();
        }
        public Task<bool> EditDepositOptionsAsync(Deposit_Options depositOptions)
        {
            throw new NotImplementedException();
        }
        public Task<List<Deposit_Options>> GetDepositOptions(decimal memId, string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
