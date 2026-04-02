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

        public async Task<List<EmployeeMasterDto>> GetEmployeeMasterListAsync(string brCode)
        {
            return await _unitOfWork.EmployeeMaster.GetEmployeeMasterListAsync(brCode);
        }
        public async Task<EmployeeMasterDto> GetEmployeeMasterById(decimal empId, string brCode)
        {
            return await _unitOfWork.EmployeeMaster.GetEmployeeMasterById(empId, brCode);
        }
        public Task<List<DropdownItem>> GetPayGenInfoListAsync(int infoType)
        {
            throw new NotImplementedException();
        }

        public async Task<List<DtoEmployeeExit>> GetEmployeeExitListAsync(string brCode)
        {
            return await _unitOfWork.EmployeeMaster.GetEmployeeExitListAsync(brCode);
        }
    }
}
