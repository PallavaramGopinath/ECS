using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IEmpMasterRepository
    {
        Task<bool>AddEmployeeMasterAsync(Emp_Master empMaster);
        Task<bool> EditEmployeeMasterAsync(Emp_Master empMaster);
        Task<List<DropdownItem>> GetPayGenInfoListAsync(int infoType);
        Task<List<EmployeeMasterDto>> GetEmployeeMasterListAsync(string brCode);
        Task<EmployeeMasterDto> GetEmployeeMasterById(decimal empId, string brCode);
        Task<List<DtoEmployeeExit>> GetEmployeeExitListAsync(string brCode);
        Task<List<DtoEmployeeExit>> AddSeparationEmployeeAsync(DtoEmployeeExit emp);
        Task<List<DtoEmployeeExit>> RevertSeparationEmployeeAsync(DtoEmployeeExit emp);
    }
}
