using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class EmpPFRepository : Repository<Emp_Pf>, IEmpPFRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public EmpPFRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddEmployeePFAsync(Emp_Pf empPf)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Emp_Pf.MaxAsync(x => x.Pf_Id);
                maxId++;
                empPf.Pf_Id = maxId;
                await AddAsync(empPf);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeePFAsync(Emp_Pf empPf)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(empPf);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee PF not deleted");
            }
            return result;
        }
    }
}
