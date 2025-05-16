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
    public class MapGeneralHandler : IMapGeneralHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MapGeneralHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMapGeneralAsync(Map_General mapGeneral)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MapGeneral.AddMapGeneralAsync(mapGeneral);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Mapping business module to accounts module not saved");
            }
            return result;
        }

        public async Task<bool> EditMapGeneralAsync(Map_General mapGeneral)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MapGeneral.EditMapGeneralAsync(mapGeneral);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Mapping business module to accounts module not deleted");
            }
            return result;
        }

        public async Task<Map_General> GetMapGeneralAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetMapGeneralAsync(brCode);
        }

        public async Task<DropdownItem> GetCashLedgerAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetCashLedgerAsync(brCode);
        }

        public async Task<decimal> GetCashLedgerIdAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetCashLedgerIdAsync(brCode);
        }

        public async Task<decimal> GetDividendLedIdAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetDividendLedIdAsync(brCode); 
        }

        public async Task<(decimal fdLedId, decimal fdIntLedId, decimal fdExcessIntPaidLedId)> GetFdLedgerIdListAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetFdLedgerIdListAsync(brCode);
        }
        public async Task<(decimal rdLedId, decimal rdIntLedId, decimal rdPiLedId)> GetRDLedgerIdListAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetRDLedgerIdListAsync (brCode);
        }

        public async Task<double> GetShareCapitalPercentageAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetShareCapitalPercentageAsync (brCode);
        }

        public async Task<decimal> GetShareCapitalLedIdAsync(string brCode)
        {
            return await _unitOfWork.MapGeneral.GetShareCapitalLedIdAsync (brCode);
        }
    }
}
