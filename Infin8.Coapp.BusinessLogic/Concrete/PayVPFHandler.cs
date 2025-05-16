using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayVPFHandler : IPayVPFHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayVPFHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> AddPayVPFAsync(Pay_VPF payVPF)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayVPF.AddPayVPFAsync(payVPF);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's voluntry PF not saved");
            }
            return result;
        }

        public async Task<bool> EditPayVPFAsync(Pay_VPF payVPF)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayVPF.EditPayVPFAsync(payVPF);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's voluntry PF not deleted");
            }
            return result;
        }
    }
}
