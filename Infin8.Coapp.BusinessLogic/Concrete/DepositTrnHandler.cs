using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class DepositTrnHandler : IDepositTrnHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DepositTrnHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<(bool result, decimal depositTrnId, string depositTrnNo)> AddDepositTrnAsync(Deposit_Trn depositTrn)
        {
            throw new NotImplementedException();
        }

        public async  Task<List<Mem_Demand>> CalculateDepositDemand(decimal memId, DateOnly demandCalcDate, DateOnly demandDate, double memBasicPay, string brCode)
        {
            return await _unitOfWork.DepositTrn.CalculateDepositDemand(memId,demandCalcDate, demandDate, memBasicPay, brCode);
        }

        public async Task<bool> EditDepositTrnAsync(Deposit_Trn depositTrn)
        {
            throw new NotImplementedException();
        }

        //public Task<List<Deposit_Trn>> GetDepositTrns(decimal memId, string brCode)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
