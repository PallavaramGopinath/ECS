using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class EmpPfCalendarRepository : Repository<Emp_PFCalcCalendar>, IEmpPfCalendarRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public EmpPfCalendarRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddEmployeePfCalendarAsync(Emp_PFCalcCalendar empPFCalcCalendar)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Emp_PFCalcCalendar.MaxAsync(x => x.PFCalc_Id);
                maxId++;
                empPFCalcCalendar.PFCalc_Id  = maxId;
                await AddAsync(empPFCalcCalendar);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF calculation calendar not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeePfCalendarAsync(Emp_PFCalcCalendar empPFCalcCalendar)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(empPFCalcCalendar);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF calculation calendar not deleted");
            }
            return result;
        }
    }
}
