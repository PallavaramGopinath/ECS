using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class DepositOptionsHandler :IDepositOptionsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DepositOptionsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
