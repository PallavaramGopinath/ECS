using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class PayAttanceHandler : IPayAttanceHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public PayAttanceHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddPayAttanceAsync(Pay_Att payAtt)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayAttance.AddPayAttanceAsync(payAtt);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's attandance details not saved");
            }
            return result;
        }

        public async Task<bool> EditPayAttanceAsync(Pay_Att payAtt)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.PayAttance.EditPayAttanceAsync(payAtt);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee's attandance details not deleted");
            }
            return result;
        }

        public async Task<Pay_Att> GetPayAttanceByEmpId(decimal empId, decimal payId, string brCode)
        {
            return await _unitOfWork.PayAttance.GetPayAttanceByEmpId(empId, payId, brCode);
        }
    }
}
