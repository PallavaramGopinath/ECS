using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MemAddressHandler : IMemAddressHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemAddressHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddMemAddressAsync(Mem_Address memAddress)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemAddress.AddMemAddressAsync(memAddress);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Member address not saved");
            }
            return result;
        }

        public async Task<bool> EditMemAddressAsync(Mem_Address memAddress)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemAddress.EditMemAddressAsync(memAddress);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Member address not deleted");
            }
            return result;
        }
    }
}
