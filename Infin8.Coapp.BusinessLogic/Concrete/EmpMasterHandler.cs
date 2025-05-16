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
    public class EmpMasterHandler : IEmpMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public EmpMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddEmployeeMasterAsync(Emp_Master empMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeeMaster.AddEmployeeMasterAsync(empMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee master not saved");
            }
            return result;
        }

        public async  Task<bool> EditEmployeeMasterAsync(Emp_Master empMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeeMaster.EditEmployeeMasterAsync(empMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee master not deleted");
            }
            return result;
        }

        public async Task<List<EmployeeMasterDto>> GetEmployeeMasterListAsync()
        {
            return await _unitOfWork.EmployeeMaster.GetEmployeeMasterListAsync();
        }

        public async Task<List<DropdownItem>> GetPayGenInfoListAsync(int infoType)
        {
            return await _unitOfWork.EmployeeMaster.GetPayGenInfoListAsync(infoType);
        }
    }
}
