using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IDepositMastersHandler
    {
        Task<(bool result, decimal depositId, string depositNo)> AddDepositMasterAsync(Deposit_Masters depositMaster);
        Task<bool> EditDepositMasterAsync(Deposit_Masters depositMaster);
        Task<List<Deposit_Masters>> GetDepositMasters();
    }
}
