using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class JLDailyMarketRateHandler : IJLDailyMarketRateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public JLDailyMarketRateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddDailyMarketRateAsync(JL_DailyMarketRate jLDailyMarketRate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLDailyMarketRate.AddDailyMarketRateAsync(jLDailyMarketRate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan daily market rate not saved");
            }
            return result;
        }

        public async Task<bool> EditDailyMarketRateAsync(JL_DailyMarketRate jLDailyMarketRate)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLDailyMarketRate.EditDailyMarketRateAsync(jLDailyMarketRate);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan daily market rate not deleted");
            }
            return result;
        }

        public async Task<double> GetMarketRate()
        {
            double marketRate = 0;
            try
            {
                marketRate  = await _unitOfWork.JLDailyMarketRate.GetMarketRate();
                
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! while fetching market rate for Jewels");
            }
            return marketRate;
        }
    }
}
