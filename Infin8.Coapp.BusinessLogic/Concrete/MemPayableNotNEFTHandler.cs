using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class MemPayableNotNEFTHandler : IMemPayableNotNEFTHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public MemPayableNotNEFTHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddMemPayableNotNEFTAsync(Mem_Payable_NotNEFT memPayableNotNEFT)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPayableNotNEFT.AddMemPayableNotNEFTAsync(memPayableNotNEFT);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Dividend not payable by NEFT list not saved");
            }
            return result;
        }

        public async Task<bool> DeleteMemPayableNotNEFTAsync(Mem_Payable_NotNEFT memPayableNotNEFT)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.MemPayableNotNEFT.DeleteMemPayableNotNEFTAsync(memPayableNotNEFT);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! dividend not payable not deleted");
            }
            return result;
        }
    }
}
