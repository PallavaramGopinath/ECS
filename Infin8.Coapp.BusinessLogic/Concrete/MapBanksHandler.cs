using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MapBanksHandler : IMapBanksHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MapBanksHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMapBanksAsync(Map_Banks mapBanks)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MapBanks.AddMapBanksAsync(mapBanks);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Mapping bank accounts not saved");
            }
            return result;
        }

        public async Task<bool> DeleteMapBanksAsync(Map_Banks mapBanks)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MapBanks.DeleteMapBanksAsync(mapBanks);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Mapping bank account not deleted");
            }
            return result;
        }
    }
}
