using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IMapGeneralHandler
    {
        Task<bool> AddMapGeneralAsync(Map_General mapGeneral);
        Task<bool> EditMapGeneralAsync(Map_General mapGeneral);
        Task<Map_General> GetMapGeneralAsync(string brCode);
        Task<DropdownItem> GetCashLedgerAsync(string brCode);
        Task<decimal> GetCashLedgerIdAsync(string brCode);
        Task<decimal> GetShareCapitalLedIdAsync(string brCode);
        Task<decimal> GetDividendLedIdAsync(string brCode);
        Task<(decimal fdLedId, decimal fdIntLedId, decimal fdExcessIntPaidLedId)> GetFdLedgerIdListAsync(string brCode);
        Task<(decimal rdLedId, decimal rdIntLedId, decimal rdPiLedId)> GetRDLedgerIdListAsync(string brCode);
        Task<double> GetShareCapitalPercentageAsync(string brCode);
    }
}
