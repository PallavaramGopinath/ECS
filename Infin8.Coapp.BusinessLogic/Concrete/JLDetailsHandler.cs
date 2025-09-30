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
    public class JLDetailsHandler : IJLDetailsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public JLDetailsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddJLDetailsAsync(JL_Details jlDetails)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLDetails.AddJLDetailsAsync(jlDetails);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan details not saved");
            }
            return result;
        }

        public async Task<bool> EditJLDetailsAsync(JL_Details jlDetails)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLDetails.EditJLDetailsAsync(jlDetails);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan details not deleted");
            }
            return result;
        }

        public async Task<JewelLoanMarketRate> GetMarketRateAndAdoptedRateAsync(string brCode)
        {
            return await _unitOfWork.JLDetails.GetMarketRateAndAdoptedRateAsync(brCode);
        }
    }
}
