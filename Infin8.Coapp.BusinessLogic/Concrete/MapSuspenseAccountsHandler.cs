using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MapSuspenseAccountsHandler : IMapSuspenseAccountsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MapSuspenseAccountsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMapSuspenseAccountsAsync(Map_SuspenseAccounts mapSuspensAccounts)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MapSuspenseAccounts.AddMapSuspenseAccountsAsync(mapSuspensAccounts);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Mapping Suspense accounts not saved");
            }
            return result;
        }

        public async Task<bool> EditMapSuspenseAccountsAsync(Map_SuspenseAccounts mapSuspensAccounts)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MapSuspenseAccounts.EditMapSuspenseAccountsAsync(mapSuspensAccounts);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Mapping Suspense accounts not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetSuspenseLedgerItemsBySupenseTypeAsync(int suspenseType, string brCode)
        {
            return await _unitOfWork.MapSuspenseAccounts.GetSuspenseLedgerItemsBySupenseTypeAsync(suspenseType, brCode);
        }

        public async Task<List<Map_SuspenseAccounts>> GetSuspenseLedgerAsync(string brCode)
        {
            return await _unitOfWork.MapSuspenseAccounts.GetSuspenseLedgerAsync(brCode);
        }

        public async Task<bool> UpdateMapSuspenseAccountsAsync(List<Map_SuspenseAccounts> mapSuspensAccounts)
        {
            return await _unitOfWork.MapSuspenseAccounts.UpdateMapSuspenseAccountsAsync(mapSuspensAccounts);
        }
    }
}
