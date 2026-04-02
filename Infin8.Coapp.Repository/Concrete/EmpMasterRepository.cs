using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
                //decimal maxId = await CSISContext.Emp_Master.MaxAsync(x => x.Mem_Id);
                //maxId++;
                //empMaster.Mem_Id = maxId;
                await AddAsync(empMaster);
                CSISContext.SaveChanges();
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

        public async Task<EmployeeMasterDto> GetEmployeeMasterById(decimal empId, string brCode)
        {
            EmployeeMasterDto employee = new();
            try
            {
                var result = await (from master in CSISContext.mem_master
                                     join emp in CSISContext.Emp_Master on master.mem_id equals emp.Mem_Id
                                     join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                                     where master.mem_id == empId && master.membertype == 4 
                                     && master.isaccountclosed == false && master.memberdelete == false
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
                                         Emp_Scale = emp.Emp_Scale,
                                         Nominee_Name = master.nomineename ,
                                         NomineeAge = master.nomineeage ,
                                         Nominee_Relationship = master.nomineerelationship ,
                                     }).FirstOrDefaultAsync();
                if (result != null && result.Mem_Id > 0) employee = result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching employee data.");
            }
            return employee;
        }

        public async Task<List<DtoEmployeeExit>> GetEmployeeExitListAsync(string brCode)
        {
            List<DtoEmployeeExit> list = new List<DtoEmployeeExit>();
            try
            {
                var empList = await (from master in CSISContext.mem_master
                                     join emp in CSISContext.Emp_Master on master.mem_id equals emp.Mem_Id
                                     where master.membertype == 4
                                     && master.brcode == brCode
                                     select new DtoEmployeeExit
                                     {
                                         EmployeeId = master.mem_id,
                                         EmployeeNo = master.memberno,
                                         EmployeeName = master.membername,
                                         Designation = emp.Emp_Desgn_Id > 0 ? CSISContext.Pay_Gen_Info.Where(p => p.Pay_Info_Id == emp.Emp_Desgn_Id).Select(p => p.Pay_Info_Name).FirstOrDefault() : string.Empty,
                                         DateOfJoin = master.doj,
                                         DateOfExit = master.accountcloseddate,
                                         ReasonForExit = master.inactivestatus,
                                         IsActive = master.memberdelete,
                                         BrCode = master.brcode
                                     }).ToListAsync();
                if (empList != null && empList.Count > 0) list = empList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching employee exit list.");
            }
            return list;
        }

        public async Task<List<DtoEmployeeExit>> AddSeparationEmployeeAsync(DtoEmployeeExit  emp)
        {
            List<DtoEmployeeExit> list = new();
            mem_master memMaster = new();
            try
            {
                var memResult = await CSISContext.mem_master.Where(m => m.mem_id == emp.EmployeeId).FirstOrDefaultAsync();
                if(memResult != null && memResult.mem_id > 0)
                {
                    memResult.accountcloseddate = emp.DateOfExit;
                    memResult.inactivestatus = emp.ReasonForExit;
                    memResult.memberdelete = true;
                    CSISContext.mem_master.Update(memResult);
                    CSISContext.SaveChanges();
                    var empList = await GetEmployeeExitListAsync(emp.BrCode!);
                    if (empList != null && empList.Count > 0) list = empList;
                }
            }
            catch (Exception ex)
            {
                list = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee master not deleted");
            }
            return list;
        }

        public async Task<List<DtoEmployeeExit>> RevertSeparationEmployeeAsync(DtoEmployeeExit emp)
        {
            List<DtoEmployeeExit> list = new();
            mem_master memMaster = new();
            try
            {
                var memResult = await CSISContext.mem_master.Where(m => m.mem_id == emp.EmployeeId).FirstOrDefaultAsync();
                if (memResult != null && memResult.mem_id > 0)
                {
                    memResult.accountcloseddate = null;
                    memResult.inactivestatus = null;
                    memResult.memberdelete = false;
                    CSISContext.mem_master.Update(memResult);
                    CSISContext.SaveChanges();
                    var empList = await GetEmployeeExitListAsync(emp.BrCode!);
                    if (empList != null && empList.Count > 0) list = empList;
                }
                
            }
            catch (Exception ex)
            {
                list = new();
                throw new InvalidOperationException(ex.Message + " Something went wrong! Employee master not deleted");
            }
            return list;
        }
    }
}
