using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IEmpJobHistoryHandler
    {
        Task<bool> AddEmpJobHistoryAsync(Emp_Job_History empJobHistory);
        Task<bool> EditEmpJobHistoryAsync(Emp_Job_History empJobHistory);
    }
}
