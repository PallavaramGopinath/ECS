using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  class DepositRoiTemplatesRepository :Repository<DepositRoiTemplatesRepository>, IDepositRoiTemplatesRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public DepositRoiTemplatesRepository(DbContext context) : base(context)
        {
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
