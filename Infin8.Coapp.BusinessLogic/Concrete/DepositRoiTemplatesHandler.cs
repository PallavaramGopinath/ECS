using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  class DepositRoiTemplatesHandler : IDepositRoiTemplatesHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public DepositRoiTemplatesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<(bool result, decimal depositRoiTemplateId, string depositRoiTemplateNo)> AddDepositRoiTemplatesAsync(Deposit_Roi_Templates depositRoiTemplates)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EditDepositRoiTemplatesAsync(Deposit_Roi_Templates depositRoiTemplates)
        {
            throw new NotImplementedException();
        }

        public Task<List<Deposit_Roi_Templates>> GetDepositRoiTemplatesAsync(string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
