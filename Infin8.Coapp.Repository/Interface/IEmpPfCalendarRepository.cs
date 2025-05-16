using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IEmpPfCalendarRepository
    {
        Task<bool> AddEmployeePfCalendarAsync(Emp_PFCalcCalendar empPFCalcCalendar);
        Task<bool> EditEmployeePfCalendarAsync(Emp_PFCalcCalendar empPFCalcCalendar);
    }
}
