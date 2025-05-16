using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class EmpPFHandler : IEmpPFHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public EmpPFHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddEmployeePFAsync(Emp_Pf empPf)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeePF.AddEmployeePFAsync(empPf);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF details not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeePFAsync(Emp_Pf empPf)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeePF.EditEmployeePFAsync(empPf);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF details not deleted");
            }
            return result;
        }
    }
}
