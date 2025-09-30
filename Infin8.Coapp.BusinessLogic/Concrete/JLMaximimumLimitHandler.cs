using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class JLMaximimumLimitHandler : IJLMaximimumLimitHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public JLMaximimumLimitHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddJLMaximumLimitAsync(JL_Max_Limit jLMaxLimit)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLMaximumLimit.AddJLMaximumLimitAsync(jLMaxLimit);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan maximum limit not saved");
            }
            return result;
        }

        public async Task<bool> EditJLMaximumLimitAsync(JL_Max_Limit jLMaxLimit)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLMaximumLimit.EditJLMaximumLimitAsync(jLMaxLimit);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan maximum limit not deleted");
            }
            return result;
        }

        public async Task<double> GetJLMaximumLimitAsync(DateTime wef, string brCode)
        {
            return await _unitOfWork.JLMaximumLimit.GetJLMaximumLimitAsync(wef,brCode);
        }

        public async Task<List<JL_Max_Limit>> GetJLMaximumLimitListAsync()
        {
            return await _unitOfWork.JLMaximumLimit.GetJLMaximumLimitListAsync();
        }
    }
}
