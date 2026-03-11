using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMapSuspenseAccountsRepository
    {
        Task<bool> AddMapSuspenseAccountsAsync(Map_SuspenseAccounts mapSuspensAccounts);
        Task<bool> EditMapSuspenseAccountsAsync(Map_SuspenseAccounts mapSuspensAccounts);
        Task<List<DropdownItem>> GetSuspenseLedgerItemsBySupenseTypeAsync(int suspenseType,string brCode);
        Task<List<Map_SuspenseAccounts>> GetSuspenseLedgerAsync(string brCode);
        Task<bool> UpdateMapSuspenseAccountsAsync(List<Map_SuspenseAccounts> mapSuspensAccounts);
    }
}
