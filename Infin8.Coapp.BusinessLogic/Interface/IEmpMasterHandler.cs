using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IEmpMasterHandler
    {
        Task<bool> AddEmployeeMasterAsync(Emp_Master empMaster);
        Task<bool> EditEmployeeMasterAsync(Emp_Master empMaster);
        Task<List<DropdownItem>> GetPayGenInfoListAsync(int infoType);
        Task<List<EmployeeMasterDto>> GetEmployeeMasterListAsync(string brCode);
    }
}
