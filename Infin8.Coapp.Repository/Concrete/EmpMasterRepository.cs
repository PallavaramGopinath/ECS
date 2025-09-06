using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class EmpMasterRepository : Repository<Emp_Master>, IEmpMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public EmpMasterRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddEmployeeMasterAsync(Emp_Master empMaster)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Emp_Master.MaxAsync(x => x.Mem_Id);
                maxId++;
                empMaster.Mem_Id = maxId;
                await AddAsync(empMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee master not saved");
            }
            return result;
        }

        public async Task<bool> EditEmployeeMasterAsync(Emp_Master empMaster)
        {
            bool result = false;
            try
            {
                //termDepositFCTemplate.TDfc_Delete = true;
                await EditAsync(empMaster);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee master not deleted");
            }
            return result;
        }

        public async Task<List<DropdownItem>> GetPayGenInfoListAsync(int infoType)
        {
            List<DropdownItem> infolist = new List<DropdownItem>();
            try
            {
                infolist = await CSISContext.Pay_Gen_Info.Where(p => p.Pay_Info_Type == infoType)
                .Select(p => new DropdownItem
                {
                    Value = p.Pay_Info_Id.ToString(), // Convert Pay_Info_Id to string
                    Text = p.Pay_Info_Name
                }).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching payroll general information.");
            }
            return infolist;
        }

        public async Task<List<EmployeeMasterDto>> GetEmployeeMasterListAsync(string brCode)
        {
            List<EmployeeMasterDto> list = new List<EmployeeMasterDto>();
            try
            {
                var empList = await (from master in CSISContext.mem_master
                               join emp in CSISContext.Emp_Master on master.mem_id equals emp.Mem_Id
                               join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                               where master.membertype == 4 && master.isaccountclosed == false && master.memberdelete == false
                               && master.brcode == brCode
                               select new EmployeeMasterDto
                               {
                                   Mem_Id = master.mem_id,
                                   MemberNo = master.memberno,
                                   MemberName = master.membername,
                                   FatherName = master.fathername,
                                   Emp_Desgn = info.Pay_Info_Name,
                                   Emp_Desgn_Id = emp.Emp_Desgn_Id,
                                   Emp_Category_Id = emp.Emp_Category_Id,
                                   Emp_Grade_Id = emp.Emp_Grade_Id,
                                   Emp_Scale = emp.Emp_Scale
                               }).ToListAsync();
                if(empList != null && empList.Count >0 ) list = empList ;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching employee list.");
            }
            return list;
        }
    }
}
