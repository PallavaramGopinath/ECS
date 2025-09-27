using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IStagingBalanceRepository
    {
        Task<List<DtoAccountsBalance>> GetStagingBalance(decimal yrId, DateTime accountingDate, decimal created_By, string brCode);
        Task<bool> AddStagingBalance(decimal yrId, DateTime accountingDate, decimal created_By, string brCode);
    }
}
