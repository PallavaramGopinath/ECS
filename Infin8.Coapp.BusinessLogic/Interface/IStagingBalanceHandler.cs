using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IStagingBalanceHandler
    {
        Task<List<DtoAccountsBalance>> GetStagingBalance(decimal yrId, DateTime accountingDate, DateTime fromDate, DateTime toDate, decimal created_By, string brCode);
        Task<bool> AddStagingBalance(decimal yrId, DateTime accountingDate, decimal created_By, string brCode);
    }
}
