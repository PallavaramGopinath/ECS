using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class JLEligibleHandler : IJLEligibleHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public JLEligibleHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddJLLoanEligibleAsync(JL_LoanEligible jLLoanEligible)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLLoanEligible.AddJLEligibleAsync(jLLoanEligible);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan eligible not saved");
            }
            return result;
        }

        public async Task<bool> EditJLLoanEligibleAsync(JL_LoanEligible jLLoanEligible)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.JLLoanEligible.EditJLEligibleAsync(jLLoanEligible);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Jewel loan eligible not deleted");
            }
            return result;
        }

        public async Task<double> GetJLEligiblePercentageAsync(string brCode)
        {
            return await _unitOfWork.JLLoanEligible.GetJLEligiblePercentageAsync(brCode);
        }
    }
}
