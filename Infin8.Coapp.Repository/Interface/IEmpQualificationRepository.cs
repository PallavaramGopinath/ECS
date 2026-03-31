using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IEmpQualificationRepository
    {
        Task<bool>AddEmployeeQualificationAsyn(Emp_Qualification empQualification);
        Task<bool> AddEmployeeQualificationListAsync(List<Emp_Qualification> empQualifications);
        Task<bool> EditEmployeeQualificationAsyn(Emp_Qualification empQualification);
    }
}
