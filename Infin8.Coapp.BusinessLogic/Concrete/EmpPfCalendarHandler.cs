using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class EmpPfCalendarHandler : IEmpPfCalendarHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public EmpPfCalendarHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddEmployeePfCalendarAsync(Emp_PFCalcCalendar empPFCalcCalendar)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeePFCalendar.AddEmployeePfCalendarAsync(empPFCalcCalendar);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF interest caluclation calendar not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeePfCalendarAsync(Emp_PFCalcCalendar empPFCalcCalendar)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.EmployeePFCalendar.EditEmployeePfCalendarAsync(empPFCalcCalendar);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF interest calculation calendar not deleted");
            }
            return result;
        }
    }
}
