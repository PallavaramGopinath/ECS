using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class EmpQualificationRepository : Repository<Emp_Qualification>, IEmpQualificationRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public EmpQualificationRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddEmployeeQualificationAsyn(Emp_Qualification empQualification)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Emp_Qualification.MaxAsync(x => x.Qua_Id);
                maxId++;
                empQualification.Qua_Id = maxId;
                await AddAsync(empQualification);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee qualification not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeeQualificationAsyn(Emp_Qualification empQualification)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(empQualification);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee qualification not deleted");
            }
            return result;
        }
    }
}
