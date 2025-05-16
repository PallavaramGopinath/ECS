using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class EmpJobHistoryHandler : IEmpJobHistoryHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public EmpJobHistoryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddEmpJobHistoryAsync(Emp_Job_History empJobHistory)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeeJobHistory.AddEmpJobHistoryAsync(empJobHistory);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee job history not saved");
            }
            return result;
        }

        public async Task<bool> EditEmpJobHistoryAsync(Emp_Job_History empJobHistory)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeeJobHistory.EditEmpJobHistoryAsync(empJobHistory);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! employee job history not deleted");
            }
            return result;
        }
    }
}
