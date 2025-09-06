using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class PaySlipRepository :Repository<Pay_Slip>, IPaySlipRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PaySlipRepository(DbContext context) : base(context)
        {
        }

        public async  Task<bool> AddPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Slip.MaxAsync(x => x.PaySlip_Id);
                maxId++;
                paySlip.PaySlip_Id = maxId;
                await AddAsync(paySlip);
                await CSISContext.SaveChangesAsync(); // Save changes to reflect in the database
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Pay slip not saved");
            }
            return result;
        }

        public async Task<bool> EditPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                paySlip.Pay_Delete = true;
                await EditAsync(paySlip);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Pay slip not modified");
            }
            return result;
        }

        public async Task<DtoPaySlip> GetPaySlipById(decimal payId, decimal memId,string brCode)
        {
            DtoPaySlip dtoPaySlip = new DtoPaySlip();
            try
            {

                var query = await (from slip in CSISContext.Pay_Slip
                                   join emp in CSISContext.Emp_Master on slip.Mem_Id equals emp.Mem_Id
                                   join mem in CSISContext.mem_master on slip.Mem_Id equals mem.mem_id
                                   join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                                   join init in CSISContext.Pay_Init on slip.Pay_Id equals init.Pay_Id
                                   join att in CSISContext.Pay_Att on new { slip.Pay_Id, emp.Mem_Id } equals new { att.Pay_Id, att.Mem_Id }
                                   where emp.Mem_Id == memId && slip.Pay_Id == payId && slip.Pay_Delete == false 
                                   && slip.BrCode == brCode && emp.BrCode == brCode && mem.brcode == brCode && info.BrCode == brCode && init.BrCode == brCode
                                   && mem.memberdelete == false && mem.isaccountclosed == false
                                   select new DtoPaySlip
                                   {
                                       Pay_Id = slip.Pay_Id,
                                       Employee_Id = mem.mem_id,
                                       Employee_No = mem.memberno,
                                       Employee_Name = mem.membername,
                                       Designation = info.Pay_Info_Name,
                                       Pay_Month = init.Pay_Month,
                                       Pay_Year = init.Pay_Year,
                                       HeadQuarters = att.Att_HQ,
                                       Camp = att.Att_CAMP,
                                       Holiday = att.Att_HD,
                                       CasualLeave = att.Att_CL,
                                       MedicalLeave = att.Att_ML,
                                       EarnedLeave = att.Att_EL,
                                       LossOfPay = att.Att_LOP,
                                       GrossPay = slip.Pay_Tot_Allowance,
                                       TotalDeductions = slip.Pay_Tot_Deductions,
                                       NetPay = slip.Pay_Net 
                                   }).FirstAsync();
                if (query != null) dtoPaySlip = query;

                var compnent = await (from trn in CSISContext.Pay_Slip_Trn
                                      join comp in CSISContext.Pay_Components on trn.Component_Id equals comp.Component_Id
                                      where trn.Mem_Id == memId && trn.Pay_Id == payId && trn.PayTr_Delete == false
                                      && trn.BrCode == brCode && comp.BrCode == brCode
                                      select new DtoPayComponentAssignments
                                      {
                                          Employee_Id = trn.Mem_Id,
                                          Component_Id = trn.Component_Id,
                                          Component_Name = comp.Component_Name,
                                          Component_Code = comp.Component_Code,
                                          Component_Type = trn.Pay_Component_Type,
                                          Led_Id = trn.Led_Id,
                                          Display_Order = comp.Display_Order,
                                          Current_Value = trn.Pay_Component_Type == 1 ? trn.Allowance_Amt : trn.Deduction_Amt,
                                      }).ToListAsync();
                if (compnent != null && compnent.Any()) dtoPaySlip.ComponentAssignments = compnent;

                var loantrn = await (from trn in CSISContext.Pay_Slip_Trn
                                     join loan in CSISContext.Pay_Slip_Loan_Trn
                                         on new { trn.Loan_Id, trn.Pay_Id } equals new { loan.Loan_Id, loan.Pay_Id }
                                     join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                                     join schemes in CSISContext.Loan_Schemes on master.Scheme_Id equals schemes.Scheme_Id
                                     where trn.Mem_Id == memId && trn.Pay_Id == payId && trn.PayTr_Delete == false 
                                     && trn.BrCode == brCode && loan.BrCode == brCode && master.BrCode == brCode && schemes.BrCode == brCode    
                                     && loan.LoanTr_Delete == false
                                     select new PayLoanBalanceVM
                                     {
                                         Loan_Id = trn.Loan_Id,
                                         Loan_No = master.Loan_No,
                                         San_Amt = master.San_Amt,
                                         San_Date = master.San_Date,
                                         Scheme_Name = schemes.Scheme_Name,
                                         PrlRecovery = loan.Prl_Coll,
                                         IntRecovery = loan.Int_Coll
                                     }).ToListAsync();
                if (loantrn != null && loantrn.Any()) dtoPaySlip.LoanList = loantrn;

                var suspensetrn = await (from trn in CSISContext.Pay_Slip_Trn
                                         join ledger in CSISContext.Fin_Ledger
                                             on trn.Led_Id equals ledger.Led_Id
                                         where trn.Mem_Id == memId && trn.Pay_Id == payId && trn.PayTr_Delete == false 
                                         && trn.BrCode == brCode && ledger.BrCode == brCode
                                         && trn.Pay_Component_Type == 5
                                         select new MemberTransactionVM
                                         {
                                             LedId = trn.Led_Id,
                                             LedName = ledger.Led_Name,
                                             Balance = trn.Deduction_Amt
                                         }).ToListAsync();
                if(suspensetrn != null && suspensetrn.Any()) dtoPaySlip.SuspeneDueToList = suspensetrn; 
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dtoPaySlip;
        }

        public async Task<Pay_Slip> GetPaySlipByMemId(decimal payId, decimal memId, string brCode)
        {
            Pay_Slip paySlip = new Pay_Slip();
            try
            {
                var result = await CSISContext.Pay_Slip.Where(x => x.Pay_Id == payId && x.Mem_Id == memId && x.Pay_Delete == false && x.BrCode == brCode).FirstOrDefaultAsync();
                if (result != null) paySlip = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return paySlip;
        }
        public async Task<List<DtoEmployeeLastPayInfo>> GetEmployeeLastPayInfo()
        {
            List<DtoEmployeeLastPayInfo> list = new List<DtoEmployeeLastPayInfo>();
            try
            {
                //var latestPayId = CSISContext.Pay_Slip
                //    .Where(ps => ps.Pmt == true)
                //    .Max(ps => ps.Pay_Id);

                //var result = await (from emp in CSISContext.Emp_Master
                //              join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                //              join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                //              join slip in CSISContext.Pay_Slip on emp.Mem_Id equals slip.Mem_Id
                //              join init in CSISContext.Pay_Init on slip.Pay_Id equals init.Pay_Id
                //              where mem.memberdelete == false
                //                    && mem.isaccountclosed == false
                //                    && slip.Pay_Id == latestPayId // Use the pre-fetched value
                //              select new DtoEmployeeLastPayInfo
                //              {
                //                  Employee_Id = emp.Mem_Id,
                //                  Employee_No = mem.memberno,
                //                  Employee_Name = mem.membername,
                //                  Employee_Designation  = info.Pay_Info_Name, // Aliased in projection
                //                  Payment_Status = "P",        // Constant value
                //                  Pay_Month  = init.Pay_Month,
                //                  Pay_Year = init.Pay_Year
                //              }).ToListAsync();

                //    SELECT DISTINCT ON(emp.mem_id)
                //    slip.pay_id as pay_Id,
                // slip.pmt,
                //    emp.mem_id,
                //    mem.memberno,
                //    mem.membername,
                //    info.pay_info_name,
                // init.pay_month,init.pay_year
                //FROM emp_master emp
                //INNER JOIN mem_master mem ON emp.mem_id = mem.mem_id
                //INNER JOIN pay_gen_info info ON emp.Emp_Desgn_Id = info.Pay_Info_Id
                //INNER JOIN pay_slip slip ON emp.mem_id = slip.mem_id
                //INNER JOIN pay_init init ON slip.pay_id = init.pay_id
                //WHERE slip.pay_delete = false
                //  AND mem.memberdelete = false
                //  AND mem.isaccountclosed = false
                //ORDER BY emp.mem_id, slip.pay_id DESC;

                //var result = await (from emp in CSISContext.Emp_Master
                //                    join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                //                    join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                //                    join slip in CSISContext.Pay_Slip on emp.Mem_Id equals slip.Mem_Id
                //                    join init in CSISContext.Pay_Init on slip.Pay_Id equals init.Pay_Id
                //                    where slip.Pay_Delete == false
                //                          && mem.memberdelete == false
                //                          && mem.isaccountclosed == false
                //                    orderby emp.Mem_Id, slip.Pay_Id descending

                //var maxPayIds = CSISContext.Pay_Slip
                //.Where(s => s.Pay_Delete == false)
                //.GroupBy(s => s.Mem_Id)
                //.Select(g => new { Mem_Id = g.Key, Max_Pay_Id = g.Max(s => s.Pay_Id) })
                //.ToList();

                //var result = (from maxPay in maxPayIds
                //             join slip in CSISContext.Pay_Slip on
                //             new { maxPay.Mem_Id, Pay_Id = maxPay.Max_Pay_Id } equals
                //             new { slip.Mem_Id, pay_id = slip.Pay_Id }
                //             join emp in CSISContext.Emp_Master on slip.Mem_Id equals emp.Mem_Id
                //             join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                //             join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                //             join init in CSISContext.Pay_Init on slip.pay_id equals init.Pay_Id
                //             where mem.memberdelete == false && mem.isaccountclosed == false
                //             select new DtoEmployeeLastPayInfo
                //             {
                //                 Pay_Id = slip.pay_id,
                //                 Pmt = slip.pmt,
                //                 Employee_Id = emp.mem_id,
                //                 Employee_No = mem.memberno,
                //                 Employee_Name = mem.membername,
                //                 Employee_Designation = info.Pay_Info_Name,
                //                 Pay_Month = init.Pay_Month,
                //                 Pay_Year = init.Pay_Year
                //             }).ToList();

                var maxPayIds = await (CSISContext.Pay_Slip
                .Where(s => s.Pay_Delete == false)
                .GroupBy(s => s.Mem_Id)
                .Select(g => new { Mem_Id = g.Key, Max_Pay_Id = g.Max(s => s.Pay_Id) }))
                .ToListAsync();

                var result =   (from maxPay in maxPayIds
                             join slip in CSISContext.Pay_Slip on maxPay.Mem_Id equals slip.Mem_Id
                             where slip.Pay_Id == maxPay.Max_Pay_Id  // Add the second condition in WHERE clause
                             join emp in CSISContext.Emp_Master on slip.Mem_Id equals emp.Mem_Id
                             join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                             join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                             join init in CSISContext.Pay_Init on slip.Pay_Id equals init.Pay_Id
                             where mem.memberdelete == false && mem.isaccountclosed == false
                             select new DtoEmployeeLastPayInfo
                             {
                                 Pay_Id = slip.Pay_Id,
                                 Pmt = slip.Pmt,
                                 Employee_Id = emp.Mem_Id,
                                 Employee_No = mem.memberno,
                                 Employee_Name = mem.membername,
                                 Employee_Designation = info.Pay_Info_Name,
                                 Pay_Month = init.Pay_Month,
                                 Pay_Year = init.Pay_Year,
                                 Payment_Status = slip.Pmt == true ? "Salary Paid" : "Salary Generated", // Assuming 'P' for paid and 'U' for unpaid
                             }).ToList();

                if (result != null && result.Any()) list = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }

        public async Task<List<DtoPayComponentAssignments>> GetPayComponentAssignmentsByEmployeeId(decimal empId, string brCode)
        {
            List<DtoPayComponentAssignments> list = new List<DtoPayComponentAssignments>(); 
            try
            {
                var result = await (from assig in CSISContext.Pay_Component_Assignments
                                    join component in CSISContext.Pay_Components
                                        on assig.Component_Id equals component.Component_Id
                                    where assig.Employee_Id == empId // Use 'L' for long integer
                                          && assig.Is_Active == true
                                          && assig.BrCode == brCode
                                          && component.BrCode == brCode
                                    orderby component.Component_Type, component.Display_Order
                                    select new DtoPayComponentAssignments
                                    {
                                        Id = assig.Id,
                                        Employee_Id = assig.Employee_Id,
                                        Component_Id = assig.Component_Id,
                                        Component_Name = component.Component_Name,
                                        Component_Code = component.Component_Code,
                                        Assigned_Value = assig.Assigned_Value,
                                        Current_Value = assig.Assigned_Value, // The constant value 0
                                        Component_Type = component.Component_Type,
                                        Led_Id = component.Led_Id,
                                        Calculation_Method = component.Calculation_Method,
                                        Calculation_Basis = component.Calculation_Basis,
                                        Maximum_Amount = assig.Maximum_Amount,
                                        Percentage = assig.Percentage,
                                        Is_DA_Applicable = component.Is_DA_Applicable,
                                        Is_Overridden = assig.Is_Overridden, // Assuming this is from the assignment, not the component
                                        Display_Order = component.Display_Order
                                    }).ToListAsync();

                

                if (result != null && result.Any()) list = result.ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return  list;
        }

        public async Task<bool> Find_PaySlipInit(int payMonth, int payYear, string payDes, string brCode)
        {
            var count  = await CSISContext.Pay_Init.Where(x => x.Pay_Month == payMonth && x.Pay_Year == payYear && x.Pay_Des == payDes && x.Pay_Delete == false && x.BrCode == brCode).CountAsync();
            if(count > 0)  return true; else return false;
        }

        public async Task<bool> IsPreviousPaySlipInitialised(int payMonth, int payYear, string payDes, string brCode)
        {
            DateTime previousDate;
            previousDate = Utilities.AddMonths(new DateTime(payYear, payMonth, 1), -1);
            var count = await  CSISContext.Pay_Init.Where(x => x.Pay_Month == previousDate.Month && x.Pay_Year == previousDate.Year && x.Pay_Des == payDes && x.Pay_Delete == false).CountAsync();
            if (count > 0) return true; else return false;
        }

        public async Task<bool> IsPaySlipGenerated(decimal payId, decimal empId, string brCode)
        {
            var count = await CSISContext.Pay_Slip.Where(x => x.Pay_Id == payId && x.Mem_Id == empId && x.Pay_Delete == false && x.BrCode == brCode ).CountAsync();
            if (count > 0) return true; else return false;
        }

        public async Task<decimal> GetPaySlipId(int payMonth, int payYear,string payDes, string brCode)
        {
            decimal paySlipId = 0;
            try
            {
                var result = await  CSISContext.Pay_Init.Where(x => x.Pay_Month == payMonth && x.Pay_Year == payYear && x.Pay_Des == payDes && x.Pay_Delete == false).Select(x => x.Pay_Id).FirstOrDefaultAsync();
                if (result > 0) paySlipId = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return paySlipId;
        }

        public Task<List<DropdownItem>> GetPaySlipListForSalaryPayment(string payDescription, string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>();
            string paymentDetails = "";
            try
            {
                switch (payDescription)
                {
                    case "P":
                        paymentDetails = ": Salary";
                        break;
                    case "A":
                        paymentDetails = ": Salary Arrears";
                        break;
                    case "D":
                        paymentDetails = ": DA Arrears";
                        break;
                    case "S":
                        paymentDetails = ": SLS";
                        break;
                    case "B":
                        paymentDetails = ": Bonus";
                        break;
                    case "E":
                        paymentDetails = ": Ex-gratia";
                        break;

                }
                var result = (from ps in CSISContext.Pay_Slip
                              join pi in CSISContext.Pay_Init on ps.Pay_Id equals pi.Pay_Id
                              where pi.Pay_Des == payDescription
                                 && pi.Pay_Delete == false
                                 && ps.Pmt == false
                                 && ps.Pay_Delete == false
                                 && ps.BrCode == brCode 
                                 && pi.BrCode == brCode
                              select new DropdownItem
                              {
                                  Value = ps.Pay_Id.ToString(),
                                  Text = (new DateTime(pi.Pay_Year, pi.Pay_Month, 1).ToString("MMMM") +
                                         " " + pi.Pay_Year.ToString() + paymentDetails)
                              })
              .Distinct()
              .ToList();
                if(result != null && result.Any())
                {
                    foreach (var item in result)
                    {
                        DropdownItem dto = new DropdownItem();
                        dto.Value = item.Value;
                        dto.Text = item.Text;
                        list.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return Task.FromResult(list);
        }

        public async Task<List<DropdownItem>> GetEmploeeNamesForSalaryPayment(decimal payId,string brCode)
        {
            List<DropdownItem> list = new List<DropdownItem>(); 
            try
            {
                var result = await (from ps in CSISContext.Pay_Slip
                              join pi in CSISContext.Pay_Init on ps.Pay_Id equals pi.Pay_Id
                              join em in CSISContext.Emp_Master on ps.Mem_Id equals em.Mem_Id
                              join pgi in CSISContext.Pay_Gen_Info on em.Emp_Desgn_Id equals pgi.Pay_Info_Id
                              join mm in CSISContext.mem_master on ps.Mem_Id equals mm.mem_id
                              where ps.Pay_Id == payId
                                 && ps.Voc_Id == 0
                                 && ps.Pmt == false
                                 && ps.Pay_Delete == false
                                 && pi.Pay_Delete == false
                                 && ps.BrCode == brCode 
                                 && pi.BrCode == brCode 
                                 && pgi.BrCode == brCode
                              select new DropdownItem
                              {
                                  Value = mm.mem_id.ToString(),
                                  Text = mm.memberno + " : " + mm.membername + " : " + pgi.Pay_Info_Name
                              })
              .ToListAsync();
                if (result != null && result.Any())
                {
                    foreach (var item in result)
                    {
                        DropdownItem dto = new DropdownItem();
                        dto.Value = item.Value;
                        dto.Text = item.Text;
                        list.Add(dto);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }

        public async Task<DropdownItem> GetEmploeeNameForSalaryPayment(decimal empId, decimal payId, string brCode)
        {
            DropdownItem emp = new();
            try
            {
                var result = await (from ps in CSISContext.Pay_Slip
                                   join pi in CSISContext.Pay_Init on ps.Pay_Id equals pi.Pay_Id
                                   join em in CSISContext.Emp_Master on ps.Mem_Id equals em.Mem_Id
                                   join pgi in CSISContext.Pay_Gen_Info on em.Emp_Desgn_Id equals pgi.Pay_Info_Id
                                   join mm in CSISContext.mem_master on ps.Mem_Id equals mm.mem_id
                                   where ps.Pay_Id == payId
                                      && ps.Mem_Id == empId
                                      && em.Mem_Id == empId
                                      && mm.mem_id == empId
                                      && ps.Voc_Id == 0
                                      && ps.Pmt == false
                                      && ps.Pay_Delete == false
                                      && pi.Pay_Delete == false
                                      && ps.BrCode == brCode
                                      && pi.BrCode == brCode
                                      && pgi.BrCode == brCode
                                   select new DropdownItem
                                   {
                                       Value = mm.mem_id.ToString(),
                                       Text = mm.memberno + " : " + mm.membername + " : " + pgi.Pay_Info_Name
                                   }).FirstAsync();

                if (result != null) emp  = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return emp;
        }

        public async Task<List<Pay_Slip>> GetPaySlipForPayment(List<decimal> empIdList, decimal payId, string brCode)
        {
            List<Pay_Slip> list = new List<Pay_Slip>();
            try
            {
                var result  = await ( CSISContext.Pay_Slip.Where(x => empIdList.Contains(x.Mem_Id) && x.Pay_Id == payId && x.Pay_Delete == false && x.BrCode == brCode ).ToListAsync());
                if (result != null && result.Any()) list = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }

        
    }
}
