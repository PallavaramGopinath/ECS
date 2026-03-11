using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IMapBanksHandler
    {
        Task<bool> AddMapBanksAsync(Map_Banks mapBanks);
        Task<bool> DeleteMapBanksAsync(Map_Banks mapBanks);
        Task<List<Map_Banks>> GetMapBanksListAsync(string brCode);
        Task<bool> UpdateMapBankAccountsAsync(List<Map_Banks> mapBankAccounts);
    }
}
