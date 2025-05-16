using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayAllDedMasterHandler : IPayAllDedMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayAllDedMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddAllDedMasterAsync(Pay_All_Ded_Master payAllDedMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayAllDedMaster.AddAllDedMasterAsync(payAllDedMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's allowance & deduction master not saved");
            }
            return result;
        }

        public async Task<bool> EditAllDedMasterAsync(Pay_All_Ded_Master payAllDedMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayAllDedMaster.EditAllDedMasterAsync(payAllDedMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's allowance & deduction master not deleted");
            }
            return result;
        }
    }
}
