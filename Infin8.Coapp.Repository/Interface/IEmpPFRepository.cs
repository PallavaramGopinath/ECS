using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IEmpPFRepository
    {
        Task<bool> AddEmployeePFAsync(Emp_Pf empPf);
        Task<bool> EditEmployeePFAsync(Emp_Pf empPf);
    }
}
