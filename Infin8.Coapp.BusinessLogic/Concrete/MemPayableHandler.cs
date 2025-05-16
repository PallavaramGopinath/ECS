using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MemPayableHandler : IMemPayableHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemPayableHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemPayableAsync(Mem_Payable memPayable)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPayable.AddMemPayableAsync(memPayable);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dividend calculation not saved");
            }
            return result;
        }

        public async Task<bool> EditMemPayableAsync(Mem_Payable memPayable)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPayable.EditMemPayableAsync(memPayable);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dividend calcuation not deleted");
            }
            return result;
        }
    }
}
