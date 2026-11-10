using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public class DepositMastersHandler : IDepositMastersHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DepositMastersHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
