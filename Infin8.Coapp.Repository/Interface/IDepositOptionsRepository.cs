using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public  interface IDepositOptionsRepository
    {
        Task<(bool result, decimal depositOptionId, string depositOptionNo)> AddDepositOptionsAsync(Deposit_Options depositOptions);
        Task<bool> EditDepositOptionsAsync(Deposit_Options depositOptions);
        Task<List<Deposit_Options>> GetDepositOptions(decimal memId, string brCode);
    }
}
