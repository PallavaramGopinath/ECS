using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IDepositRoiTemplatesHandler
    {
        Task<(bool result, decimal depositRoiTemplateId, string depositRoiTemplateNo)> AddDepositRoiTemplatesAsync(Deposit_Roi_Templates depositRoiTemplates);
        Task<bool> EditDepositRoiTemplatesAsync(Deposit_Roi_Templates depositRoiTemplates);
        Task<List<Deposit_Roi_Templates>> GetDepositRoiTemplatesAsync(string brCode);
    }
}
