using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    internal interface IEmpPFHandler
    {
        Task<bool> AddEmployeePFAsync(Emp_Pf empPf);
        Task<bool> EditEmployeePFAsync(Emp_Pf empPf);
        Task<bool> CalculatePFInterestYearEnd(List<EmployeeMasterDto> empList, DateTime fromDate, DateTime toDate, decimal usrId,
                decimal Yrid, string brCode);
    }
}
