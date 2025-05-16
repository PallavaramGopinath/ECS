using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IEmpQualificationHandler
    {
        Task<bool> AddEmployeeQualificationAsyn(Emp_Qualification empQualification);
        Task<bool> EditEmployeeQualificationAsyn(Emp_Qualification empQualification);
    }
}
