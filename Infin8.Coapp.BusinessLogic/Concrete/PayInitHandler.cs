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
    public class PayInitHandler : IPayInitHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayInitHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Pay_Init> AddPayInitAsync(Pay_Init payInit)
        {
            //bool result = false;
            Pay_Init init = new();
            try
            {
                var result = await _unitOfWork.PayInit.AddPayInitAsync(payInit);
                await _unitOfWork.CompleteAsync();
                if(result != null) init = result ;
            }
            catch (Exception ex)
            {
                //result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Salary initialization not saved");
            }
            //return result;
            return init;
        }

        public async Task<bool> EditPayInitAsync(Pay_Init payInit)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayInit.EditPayInitAsync(payInit);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Salary initialization not deleted");
            }
            return result;
        }

       
        public async Task<decimal> GetPaySlipForDAArrears(DateTime fromDate, DateTime toDate, string description, string brCode)
        {
            return await _unitOfWork.PayInit.GetPaySlipForDAArrears(fromDate, toDate, description,brCode);
        }

        public async Task<List<DropdownItem>> GetPayIdList(decimal yearId, string payDes, string brCode)
        {
            return await _unitOfWork.PayInit.GetPayIdList(yearId, payDes, brCode);
        }

    }
}
