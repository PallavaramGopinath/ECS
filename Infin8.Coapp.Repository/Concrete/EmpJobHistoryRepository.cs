using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class EmpJobHistoryRepository: Repository<Emp_Job_History>, IEmpJobHistoryRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public EmpJobHistoryRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddEmpJobHistoryAsync(Emp_Job_History empJobHistory)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Emp_Job_History.MaxAsync(x => x.Job_Id);
                maxId++;
                empJobHistory.Job_Id = maxId;
                await AddAsync(empJobHistory);
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
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(empJobHistory);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee job history not deleted");
            }
            return result;
        }
    }
}
