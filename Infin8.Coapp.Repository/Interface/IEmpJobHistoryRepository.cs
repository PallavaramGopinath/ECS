using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IEmpJobHistoryRepository
    {
        Task<bool>AddEmpJobHistoryAsync(Emp_Job_History empJobHistory);
        Task<bool> EditEmpJobHistoryAsync(Emp_Job_History empJobHistory);
    }
}
