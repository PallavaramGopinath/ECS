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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infin8.Coapp.Repository
{
    public class PaySlipRepository : Repository<Pay_Slip>, IPaySlipRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public PaySlipRepository(DbContext context) : base(context)
        {
        }
        public async Task<bool> AddPaySlipAsync(Pay_Slip paySlip)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Pay_Slip.MaxAsync(x => x.PaySlip_Id);
                maxId++;
                paySlip.PaySlip_Id = maxId;
                await AddAsync(paySlip);
                CSISContext.SaveChanges();
                //await CSISContext.SaveChangesAsync(); // Save changes to reflect in the database
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
        public async Task<bool> UpdatePaySlipForPayment(decimal empId, decimal payId, decimal vocId, DateTime trnDate)
        {
            bool result = false;
            try
            {
                var paySlips = await (CSISContext.Pay_Slip
                .Where(x => x.Mem_Id == empId && x.Pay_Id == payId))
                .ToListAsync();

                foreach (var paySlip in paySlips)
                {
                    paySlip.Pmt = true;
                    paySlip.Pmt_Date = trnDate;
                    paySlip.Voc_Id = vocId;
                }
                CSISContext.SaveChanges();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        public async Task<DtoPaySlip> GetPaySlipById(decimal payId, decimal memId, string brCode)
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
                if (suspensetrn != null && suspensetrn.Any()) dtoPaySlip.SuspeneDueToList = suspensetrn;
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
        public async Task<List<DtoEmployeeLastPayInfo>> GetEmployeeLastPayInfo(string brCode)
        {
            List<DtoEmployeeLastPayInfo> list = new List<DtoEmployeeLastPayInfo>();
            try
            {
                #region commented
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

                //var maxPayIds = await (CSISContext.Pay_Slip
                //.Where(s => s.Pay_Delete == false)
                //.GroupBy(s => s.Mem_Id)
                //.Select(g => new { Mem_Id = g.Key, Max_Pay_Id = g.Max(s => s.Pay_Id) }))
                //.ToListAsync();

                //var result = (from maxPay in maxPayIds
                //              join slip in CSISContext.Pay_Slip on maxPay.Mem_Id equals slip.Mem_Id
                //              where slip.Pay_Id == maxPay.Max_Pay_Id  // Add the second condition in WHERE clause
                //              join emp in CSISContext.Emp_Master on slip.Mem_Id equals emp.Mem_Id
                //              join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                //              join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                //              join init in CSISContext.Pay_Init on slip.Pay_Id equals init.Pay_Id
                //              where mem.memberdelete == false && mem.isaccountclosed == false
                //              select new DtoEmployeeLastPayInfo
                //              {
                //                  Pay_Id = slip.Pay_Id,
                //                  Pmt = slip.Pmt,
                //                  Employee_Id = emp.Mem_Id,
                //                  Employee_No = mem.memberno,
                //                  Employee_Name = mem.membername,
                //                  Employee_Designation = info.Pay_Info_Name,
                //                  Pay_Month = init.Pay_Month,
                //                  Pay_Year = init.Pay_Year,
                //                  Payment_Status = slip.Pmt == true ? "Salary Paid" : "Salary Generated", // Assuming 'P' for paid and 'U' for unpaid
                //              }).ToList();
                #endregion 

                var query = await (from emp in CSISContext.Emp_Master
                                   join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                                   join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                                   join slip in CSISContext.Pay_Slip on emp.Mem_Id equals slip.Mem_Id
                                   join init in CSISContext.Pay_Init on slip.Pay_Id equals init.Pay_Id
                                   where slip.Pay_Delete == false
                                         && mem.memberdelete == false
                                         && mem.isaccountclosed == false
                                         && init.Pay_Des == "P"
                                         && emp.BrCode == brCode
                                         && mem.brcode == brCode
                                         && info.BrCode == brCode
                                         && slip.BrCode == brCode
                                         && init.BrCode == brCode
                                   orderby emp.Mem_Id, slip.Pay_Id descending
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
                                   })

              .ToListAsync();
                //.DistinctBy(x => x.Employee_Id) // Requires EF Core 6.0+
                var result = query.DistinctBy(x => x.Employee_Id).ToList();
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
            return list;
        }
        public async Task<bool> Find_PaySlipInit(int payMonth, int payYear, string payDes, string brCode)
        {
            var count = await CSISContext.Pay_Init.Where(x => x.Pay_Month == payMonth && x.Pay_Year == payYear && x.Pay_Des == payDes && x.Pay_Delete == false && x.BrCode == brCode).CountAsync();
            if (count > 0) return true; else return false;
        }
        public async Task<bool> IsPreviousPaySlipInitialised(int payMonth, int payYear, string payDes, string brCode)
        {
            DateTime previousDate;
            previousDate = Utilities.AddMonths(new DateTime(payYear, payMonth, 1), -1);
            var count = await CSISContext.Pay_Init.Where(x => x.Pay_Month == previousDate.Month && x.Pay_Year == previousDate.Year && x.Pay_Des == payDes && x.Pay_Delete == false).CountAsync();
            if (count > 0) return true; else return false;
        }
        public async Task<bool> IsPaySlipGenerated(decimal payId, decimal empId, string brCode)
        {
            var count = await CSISContext.Pay_Slip.Where(x => x.Pay_Id == payId && x.Mem_Id == empId && x.Pay_Delete == false && x.BrCode == brCode).CountAsync();
            if (count > 0) return true; else return false;
        }
        public async Task<decimal> GetPaySlipId(int payMonth, int payYear, string payDes, string brCode)
        {
            decimal paySlipId = 0;
            try
            {
                var result = await CSISContext.Pay_Init.Where(x => x.Pay_Month == payMonth && x.Pay_Year == payYear && x.Pay_Des == payDes && x.Pay_Delete == false).Select(x => x.Pay_Id).FirstOrDefaultAsync();
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
                Console.Write(ex.Message);
            }
            return Task.FromResult(list);
        }

        public Task<List<DropdownItem>> GetPaySlipListForSalaryPaymentByEmpId(decimal EmpId, string payDescription, string brCode)
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
                                 && ps.Mem_Id == EmpId
                              select new DropdownItem
                              {
                                  Value = ps.Pay_Id.ToString(),
                                  Text = (new DateTime(pi.Pay_Year, pi.Pay_Month, 1).ToString("MMMM") +
                                         " " + pi.Pay_Year.ToString() + paymentDetails)
                              })
              .Distinct()
              .ToList();
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
                Console.Write(ex.Message);
            }
            return Task.FromResult(list);
        }
        public async Task<List<DropdownItem>> GetEmploeeNamesForSalaryPayment(decimal payId, string brCode)
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
                                    }).FirstOrDefaultAsync();

                if (result != null) emp = result;
                
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
                var result = await (CSISContext.Pay_Slip.Where(x => empIdList.Contains(x.Mem_Id) && x.Pay_Id == payId && x.Pay_Delete == false && x.BrCode == brCode).ToListAsync());
                if (result != null && result.Any()) list = result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }

        public async Task<List<DtoPayDAArrearsView>> GetPayDAArrearsViews(List<decimal> empIdList, decimal payId, string brCode)
        {
            List<DtoPayDAArrearsView> list = new();
            try
            {
                var result = await (from slip in CSISContext.Pay_Slip
                                    join emp in CSISContext.Emp_Master on slip.Mem_Id equals emp.Mem_Id
                                    join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                                    join master in CSISContext.mem_master on slip.Mem_Id equals master.mem_id
                                    where empIdList.Contains(slip.Mem_Id)
                                          && slip.Pay_Id == payId
                                          && slip.Pay_Delete == false
                                          && slip.BrCode == brCode
                                          && emp.BrCode == brCode
                                          && master.brcode == brCode
                                    select new DtoPayDAArrearsView
                                    {
                                        Pay_Id = slip.Pay_Id,
                                        Employee_Id = slip.Mem_Id,
                                        Employee_Name = master.membername,
                                        Designation = info.Pay_Info_Name,
                                        DAArrears = slip.Pay_DA_Earned,
                                        PF = slip.Pay_PF,
                                        NetPay = slip.Pay_Net
                                    }).ToListAsync();
                if (result != null && result.Any()) list = result.ToList();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return list;
        }

        #region PF data

        public async Task<bool> UpdateInterestOnPF(decimal empId)
        {
            string errorMessage = "";
            bool result = true;
            //string sql = "";
            decimal PFId = 0;
            double PFOS = 0;
            double VPFOS = 0;
            double SPFOS = 0;
            double PFProduct = 0;
            double SPFProduct = 0;
            int NoDays = 0;
            double PFInt = 0;
            double SPFInt = 0;
            double PFIntTot = 0;
            double SPFIntTot = 0;
            DateTime FromDate;
            List<Emp_Pf> empPfList = new();
            try
            {
                CSISContext.Emp_Pf
                .Where(e => e.Mem_Id == empId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(e => e.No_Of_Days, 0)
                    .SetProperty(e => e.Pf_Product, 0)
                    .SetProperty(e => e.Bpf_Product, 0)
                    .SetProperty(e => e.Int_Calc_Upto, (DateTime?)null)
                    .SetProperty(e => e.Epf_Interest, 0)
                    .SetProperty(e => e.Bpf_Interest, 0));
                CSISContext.SaveChanges();

                //var empPfResult = await CSISContext.Emp_Pf.Where(x => x.Mem_Id == empId && x.Pf_Delete == false).OrderBy(x => new { x.Pf_Date, x.Status, x.SlNo }).ToListAsync();
                var empPfResult = await CSISContext.Emp_Pf
                .Where(x => x.Mem_Id == empId && x.Pf_Delete == false)
                .OrderBy(x => x.Pf_Date)
                .ThenBy(x => x.Status)
                .ThenBy(x => x.SlNo)
                .ToListAsync();
                if (empPfResult != null)
                {
                    empPfList = empPfResult.ToList();
                    foreach (var single in empPfList)
                    {
                        PFId = single.Pf_Id!;
                        PFOS += single.Pf_Subscription - single.Pf_Withdrawn;
                        VPFOS += single.Vpf_Contribution - single.Vpf_Withdrawn;
                        SPFOS += single.Bpf_Contribution - single.Bpf_Withdrawn;
                        FromDate = (DateTime)single.Pf_Date!;
                        if (single.Int_Calc_Upto != null)
                        {
                            NoDays = Utilities.GetNoOfDays((DateTime)single.Pf_Date, FromDate);
                            if (single.Status == "CB")
                                NoDays += 1;
                            if (single.Status == "OB")
                                NoDays -= 1;
                            if (NoDays > 0)
                            {
                                PFProduct = (PFOS + VPFOS) * NoDays;
                                SPFProduct = SPFOS * NoDays;
                                if (single.Status == "CB")
                                {
                                    PFInt = Calc_Int_On_PF(PFOS + VPFOS, FromDate, ((DateTime)single.Pf_Date).AddDays(1), out errorMessage);
                                    if (errorMessage.Length > 0)
                                    {
                                        return false;
                                    }
                                    SPFInt = Calc_Int_On_PF(SPFOS, FromDate, ((DateTime)single.Pf_Date).AddDays(1), out errorMessage);
                                    if (errorMessage.Length > 0)
                                    {
                                        return false;
                                    }
                                }
                                else if (single.Status == "OB")
                                {
                                    PFInt = Calc_Int_On_PF(PFOS + VPFOS, FromDate, ((DateTime)single.Pf_Date).AddDays(-1), out errorMessage);
                                    if (errorMessage.Length > 0)
                                    {
                                        return false;
                                    }
                                    SPFInt = Calc_Int_On_PF(SPFOS, FromDate, ((DateTime)single.Pf_Date).AddDays(-1), out errorMessage);
                                    if (errorMessage.Length > 0)
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    PFInt = Calc_Int_On_PF(PFOS + VPFOS, FromDate, (DateTime)single.Pf_Date, out errorMessage);
                                    if (errorMessage.Length > 0)
                                    {
                                        return false;
                                    }
                                    SPFInt = Calc_Int_On_PF(SPFOS, FromDate, (DateTime)single.Pf_Date, out errorMessage);
                                    if (errorMessage.Length > 0)
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                NoDays = 0;
                                PFProduct = 0;
                                SPFProduct = 0;
                                PFInt = 0;
                                SPFInt = 0;
                            }
                            PFIntTot += PFInt;
                            SPFIntTot += SPFInt;
                            if (((DateTime)single.Pf_Date).Month == 4 && single.Status == "OB")
                            {
                                CSISContext.Emp_Pf
                                .Where(e => e.Pf_Id == PFId)
                                .ExecuteUpdate(setters => setters
                                    .SetProperty(e => e.Pf_Subscription, PFIntTot)
                                    .SetProperty(e => e.Epf_Int_Withdrawn, PFIntTot)
                                    .SetProperty(e => e.Pf_Balance, PFOS)
                                    .SetProperty(e => e.Bpf_Contribution, SPFOS)
                                    .SetProperty(e => e.Bpf_Int_Withdrawn, SPFIntTot)
                                    .SetProperty(e => e.Bpf_Balance, SPFOS));
                                CSISContext.SaveChanges();

                                //sql = @"Update emp_pf set pf_subscription = @pfIntTot , 
                                //epf_Int_withdrawn = @pfIntTot, 
                                //pf_balance = @pfOs, 
                                //bpf_contribution = @spfOs, 
                                //bpf_Int_withdrawn = @spfIntTot, 
                                //bpf_balance = @spfOs 
                                //where pf_id = @pfId";
                                //context.Database.ExecuteSqlCommand(sql
                                //    , new SqlParameter("@pfIntTot", PFIntTot)
                                //    , new SqlParameter("@pfOs", PFOS)
                                //    , new SqlParameter("@spfOs", SPFOS)
                                //    , new SqlParameter("@spfIntTot", SPFIntTot)
                                //    , new SqlParameter("@pfId", PFId));
                            }

                            CSISContext.Emp_Pf
                                .Where(e => e.Pf_Id == PFId)
                                .ExecuteUpdate(setters => setters
                                    .SetProperty(e => e.No_Of_Days, NoDays)
                                    .SetProperty(e => e.Pf_Product, PFProduct)
                                    .SetProperty(e => e.Epf_Interest, PFInt)
                                    .SetProperty(e => e.Bpf_Product, SPFProduct)
                                    .SetProperty(e => e.Bpf_Interest, SPFInt));
                            CSISContext.SaveChanges();

                            //sql = @"Update emp_pf set no_of_days = @noDays, pf_Product = @pfProduct, epf_Interest = @pfInt
                            //, bpf_product = @spfProduct, bpf_Interest = @spfInt where pf_id = @pfId";
                            //context.Database.ExecuteSqlCommand(sql
                            //    , new SqlParameter("@noDays", NoDays)
                            //    , new SqlParameter("@pfProduct", PFProduct)
                            //    , new SqlParameter("@pfInt", PFInt)
                            //    , new SqlParameter("@spfProduct", SPFProduct)
                            //    , new SqlParameter("@spfInt", SPFInt)
                            //    , new SqlParameter("@spfId", PFId));
                        }
                        else
                        {
                            PFIntTot += single.Epf_Interest - single.Pf_Int_Withdrawn;
                            SPFIntTot += single.Bpf_Interest - single.Bpf_Int_Withdrawn;
                        }
                        FromDate = (DateTime)single.Pf_Date;
                        PFOS += single.Pf_Subscription - single.Pf_Withdrawn;
                        VPFOS += single.Vpf_Contribution - single.Vpf_Withdrawn;
                        SPFOS += single.Vpf_Contribution - single.Bpf_Withdrawn;

                        CSISContext.Emp_Pf
                                .Where(e => e.Pf_Id == PFId)
                                .ExecuteUpdate(setters => setters
                                    .SetProperty(e => e.Pf_Balance, PFOS)
                                    .SetProperty(e => e.Vpf_Balance, VPFOS)
                                    .SetProperty(e => e.Bpf_Balance, SPFOS)
                                    .SetProperty(e => e.Int_Calc_Upto, single.Pf_Date));
                        CSISContext.SaveChanges();

                        //    sql = @"UPDATE Emp_pf set pf_balance = @pfOS, vpf_balance = @vpfOS, bpf_balance = @spfOs, int_calc_upto = @pfDate WHERE PF_Id = @pfId";
                        //    context.Database.ExecuteSqlCommand(sql
                        //        , new SqlParameter("@pfOs", PFOS)
                        //        , new SqlParameter("@vpfOs", VPFOS)
                        //        , new SqlParameter("@spfOs", SPFOS)
                        //        , new SqlParameter("@pfDate", single.Pf_Date)
                        //        , new SqlParameter("@pfId", PFId));
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                errorMessage = ex.Message;
            }
            return result;
        }

        public double Calc_Int_On_PF(double PFAmt, DateTime FromDate, DateTime ToDate, out string errorMessage)
        {
            double intCalcAmt = 0;
            double roi = 0;
            DateTime tmpToDate;
            int noOfDays = 0;
            double interest = 0;
            List<RateOfInterestVM> roilist = new();
            errorMessage = "";
            try
            {
                roi = GetPFROI(FromDate);

                if (errorMessage.Length > 0)
                {
                    intCalcAmt = 0;
                    return intCalcAmt;
                }
                roilist = GetPFROI(FromDate, ToDate);
                if (errorMessage.Length > 0)
                {
                    intCalcAmt = 0;
                    return intCalcAmt;
                }
                foreach (RateOfInterestVM single in roilist)
                {
                    tmpToDate = single.Roi_Wef;
                    //intCalcAmt += Utilities.Calculate_Interest_RoundOffTwoDigits(PFAmt, roi, (int)(tmpToDate - FromDate).TotalDays);
                    noOfDays = (int)(tmpToDate - FromDate).TotalDays;
                    if (noOfDays <= 0) noOfDays = 0;
                    intCalcAmt += interest = Math.Round((PFAmt * (roi / 100)) / 365 * noOfDays, 2);
                    roi = single.Roi;
                    FromDate = single.Roi_Wef;
                }
                //intCalcAmt += Utilities.Calculate_Interest_RoundOffTwoDigits(PFAmt, roi, (int)(ToDate - FromDate).TotalDays);
                noOfDays = (int)(ToDate - FromDate).TotalDays;
                if (noOfDays <= 0) noOfDays = 0;
                intCalcAmt += interest = Math.Round((PFAmt * (roi / 100)) / 365 * noOfDays, 2);

            }
            catch (Exception ex)
            {
                intCalcAmt = 0;
                errorMessage = ex.Message;
            }
            return intCalcAmt;
        }

        public double GetPFROI(DateTime FromDate)
        {
            double? roi = 0;
            try
            {
                roi = CSISContext.Pay_PF_ROITemplate
                .Where(r => r.Roi_Delete == false && r.Roi_Wef <= FromDate)
                .OrderByDescending(r => r.Roi_Wef)
                .Select(r => r.Roi)
                .FirstOrDefault();
            }
            catch (Exception ex)
            {
                roi = 0;
            }
            double.TryParse(roi.ToString(), out double result);
            return result;

        }

        public List<RateOfInterestVM> GetPFROI(DateTime FromDate, DateTime ToDate)
        {
            List<RateOfInterestVM> roiList = new List<RateOfInterestVM>();
            try
            {
                var result = CSISContext.Pay_PF_ROITemplate.Where(x => x.Roi_Wef <= FromDate && x.Roi_Wef >= ToDate && x.Roi_Delete == false).ToList();
                if (result != null && result.Any())
                {
                    foreach (var roi in result)
                    {
                        RateOfInterestVM rateVM = new()
                        {
                            Roi = roi.Roi,
                            Roi_Wef = (DateTime)roi.Roi_Wef!
                        };
                        roiList.Add(rateVM);
                    }
                }
                // roiList = context.Database.SqlQuery<RateOfInterestVM>(@"SELECT roi FROM Pay_PF_ROITemplate 
                // WHERE roi_delete = 0 and 
                // roi_wef <= @fromDate and 
                // roi_wef >= @toDate"
                //, new SqlParameter("fromdate", FromDate)
                //, new SqlParameter("@toDate", ToDate)).ToList();
            }
            catch (Exception ex)
            {
                roiList = null;
            }
            return roiList!;
        }
        public async Task<DtoPayPFData> GetPFBalance(decimal empId, DateTime AsOnDate, string brCode)
        {
            DtoPayPFData pfData = new();
            try
            {
                var result = await UpdateInterestOnPF(empId);
                if (result == false)
                {
                    pfData._isError = true;
                    pfData.errorMessage = "Error in updating pf balance";
                    return pfData;
                }

                var pfbalance = CSISContext.Emp_Pf.Where(x => x.Mem_Id == empId && x.Pf_Delete == false && x.Pf_Date <= AsOnDate && x.BrCode == brCode )
                    .GroupBy(x => x.Mem_Id)
                    .Select(g => new
                    {
                        pf_subscription = g.Sum(x => x.Pf_Subscription),
                        pf_withdrawn = g.Sum(x => x.Pf_Withdrawn),
                        vpf_contribution = g.Sum(x => x.Vpf_Contribution),
                        vpf_withdrawn = g.Sum(x => x.Vpf_Withdrawn),
                        bpf_contribution = g.Sum(x => x.Bpf_Contribution),
                        bpf_withdrawn = g.Sum(x => x.Bpf_Withdrawn),
                        epf_Interest = g.Sum(x => x.Epf_Interest),
                        epf_Int_withdrawn = g.Sum(x => x.Epf_Int_Withdrawn),
                        bpf_Interest = g.Sum(x => x.Bpf_Interest),
                        bpf_Int_withdrawn = g.Sum(x => x.Bpf_Int_Withdrawn),
                        last_Int_Calc_Date = g.Max(x => x.Last_Int_ApplicationDate)
                    }).FirstOrDefault();
                if (pfbalance != null && pfbalance.bpf_contribution > 0)
                {

                    pfData.Employee_Id = empId;
                    pfData.PFBalance = pfbalance.pf_subscription - pfbalance.pf_withdrawn;
                    pfData.VPFWithdrawn = pfbalance.vpf_contribution - pfbalance.vpf_withdrawn ;
                    pfData.EmployersPFBalance = pfbalance.bpf_contribution - pfbalance.bpf_withdrawn ;
                    pfData.InterestOnPF = pfbalance.epf_Interest - pfbalance.epf_Int_withdrawn;
                    pfData.InterestOnEmployersPF  = pfbalance.bpf_Interest - pfbalance.bpf_Int_withdrawn;
                    pfData.InterestOnPFCalculated = 0;
                    pfData.InterestOnEmployersPFCalculated = 0;
                    pfData.InterestAppliledDate = pfbalance.last_Int_Calc_Date;
                    pfData.PFReceived = 0;
                    pfData.VPFReceived = 0;
                    pfData.EmployersPFReceived = 0;
                    pfData.PFWithdrawn = 0;
                    pfData.VPFWithdrawn = 0;
                    pfData.EmployersPFWithdrawn = 0;
                    pfData.InterestOnPFWithdrawn = 0;
                    pfData.InterestOnEmployersPFWithdrawn = 0;
                }
            }
            catch (Exception ex)
            {
                pfData._isError = true;
                pfData.errorMessage = ex.Message;
            }
            return pfData;
        }

        #endregion

        #region SLS
        public async Task<List<DtoSLSComponent>> GetSLSData(decimal empId, string brCode)
        {
            List<DtoSLSComponent> slsList = new();
            try
            {
                #region commented
                //string sql = @"SELECT PAY_slip.Pay_Id,  Pay_Slip.Pay_Basic, Pay_Slip.Pay_PP, Pay_Slip.Pay_GradePay, Pay_Slip.Pay_DA_Percent, Pay_Slip_Trn.All_Id, 
                //        Pay_Slip_Trn.Allowance_Amt,  Pay_Components.Component_Name ,Pay_Components.Component_Code, Pay_init.DA_Id
                //        FROM Pay_Slip
                //        INNER JOIN Pay_init ON Pay_Slip.Pay_Id = Pay_init.Pay_Id
                //        INNER JOIN Pay_Slip_Trn ON Pay_Slip.Mem_Id = Pay_Slip_Trn.Mem_id AND Pay_Slip.Pay_Id = Pay_Slip_Trn.Pay_Id
                //        LEFT JOIN Pay_Components ON Pay_Slip_Trn.All_Id = Pay_Components.Component_id
                //        WHERE Pay_Slip.Pay_Id = (SELECT Max(Pay_Slip.Pay_Id) AS MaxOfPay_Id FROM Pay_Slip INNER JOIN Pay_init ON Pay_Slip.Pay_Id = Pay_init.Pay_Id
                //                                    where Pay_Slip.Pmt = true AND Pay_Slip.Pay_Delete = false AND Pay_Slip.Mem_Id = 110010008784 AND Pay_init.pay_des = 'P')
                //        AND Pay_Slip.Pmt = true AND Pay_Slip.Pay_Delete = false AND Pay_Slip.Mem_Id = 110010008784
                //        AND Pay_init.pay_des = 'P' AND Pay_init.Pay_Delete = false
                //        AND Pay_Slip_Trn.PayTr_Delete = false AND Pay_Slip_Trn.All_Id > 0
                //        AND Pay_Components.Is_sl_applicable = true
                //        ORDER BY Pay_Slip_Trn.All_Id";
                //sql = @"SELECT PAY_slip.Pay_Id,  Pay_Slip.Pay_Basic, Pay_Slip.Pay_PP, Pay_Slip.Pay_GradePay, Pay_Slip.Pay_DA_Percent, Pay_init.DA_Id
                //        FROM Pay_Slip
                //        INNER JOIN Pay_init ON Pay_Slip.Pay_Id = Pay_init.Pay_Id
                //        WHERE Pay_Slip.Pay_Id = (SELECT Max(Pay_Slip.Pay_Id) AS MaxOfPay_Id FROM Pay_Slip INNER JOIN Pay_init ON Pay_Slip.Pay_Id = Pay_init.Pay_Id
                //                                    where Pay_Slip.Pmt = true AND Pay_Slip.Pay_Delete = false AND Pay_Slip.Mem_Id = 110010008784 AND Pay_init.pay_des = 'P')
                //        AND Pay_Slip.Pmt = true AND Pay_Slip.Pay_Delete = false AND Pay_Slip.Mem_Id = 110010008784
                //        AND Pay_init.pay_des = 'P' AND Pay_init.Pay_Delete = false";

                //var result = await (from ps in CSISContext.Pay_Slip
                //              join pi in CSISContext.Pay_Init on ps.Pay_Id equals pi.Pay_Id
                //              where ps.Pay_Id == (from ps2 in CSISContext.Pay_Slip
                //                                  join pi2 in CSISContext.Pay_Init on ps2.Pay_Id equals pi2.Pay_Id
                //                                  where ps2.Pmt == true
                //                                        && ps2.Pay_Delete == false
                //                                        && ps2.Mem_Id == 110010008784
                //                                        && pi2.Pay_Des == "P"
                //                                  select ps2.Pay_Id).Max()
                //              && ps.Pmt == true
                //              && ps.Pay_Delete == false
                //              && ps.Mem_Id == 110010008784
                //              && pi.Pay_Des == "P"
                //              && pi.Pay_Delete == false
                //              select new DtoPaySLSPayment
                //              {
                //                  Pay_Id = ps.Pay_Id,
                //                  BasicPay =  ps.Pay_Basic,
                //                  PerPay =  ps.Pay_PP,
                //                  GradePay =  ps.Pay_GradePay,
                //                  DAPercentage = ps.Pay_DA_Percent,
                //                  DAId =  pi.DA_Id,
                //                  DAAmount = ps.Pay_DA_Earned,
                //              }).FirstOrDefaultAsync();
                #endregion 

                var maxPayId = (from ps in CSISContext.Pay_Slip
                                join pi in CSISContext.Pay_Init on ps.Pay_Id equals pi.Pay_Id
                                where ps.Pmt == true
                                      && ps.Pay_Delete == false
                                      && ps.Mem_Id == empId
                                      && pi.Pay_Des == "P"
                                      && ps.BrCode == brCode 
                                      && pi.BrCode == brCode 
                                select ps.Pay_Id).Max();

                var result = await (from ps in CSISContext.Pay_Slip
                             join pi in CSISContext.Pay_Init on ps.Pay_Id equals pi.Pay_Id
                             join pst in CSISContext.Pay_Slip_Trn on new { ps.Mem_Id, ps.Pay_Id } equals new { pst.Mem_Id, pst.Pay_Id }
                             join pc in CSISContext.Pay_Components on pst.All_Id equals pc.Component_Id into pcGroup
                             from pc in pcGroup.DefaultIfEmpty()
                             where ps.Pay_Id == maxPayId
                                   && ps.Pmt == true
                                   && ps.Pay_Delete == false
                                   && ps.Mem_Id == empId
                                   && ps.BrCode == brCode 
                                   && pi.Pay_Des == "P"
                                   && pi.Pay_Delete == false
                                   && pi.BrCode == brCode
                                   && pst.PayTr_Delete == false
                                   && pst.BrCode == brCode 
                                   && pst.All_Id > 0
                                   && (pc == null || pc.Is_SL_Applicable == true)
                             orderby pst.All_Id
                             select new DtoSLSComponent
                             {
                                 Pay_Id =  ps.Pay_Id,
                                 DAId = pi.DA_Id,
                                 DAPercentage =  ps.Pay_DA_Percent,
                                 Component_Id =   pst.All_Id,
                                 Allowance_Amount =  pst.Allowance_Amt,
                                 Component_Name = pc == null ? null : pc.Component_Name,
                                 Component_Code = pc == null ? null : pc.Component_Code,
                             }).ToListAsync ();
                if (result != null && result.Any()) slsList  = result.ToList ();
            }
            catch (Exception ex)
            {
                Console.Write(ex.StackTrace);
            }
            return slsList ;
        }

        public async Task<bool> IsSLSAlreadyPaid(decimal empId, DateTime fromDate, DateTime toDate, string payDesc, string brCode)
        {
            bool result = false;
            try
            {
                var count = await  (from slip in CSISContext.Pay_Slip
                             join init in CSISContext.Pay_Init
                             on slip.Pay_Id equals init.Pay_Id
                             where slip.Pmt_Date >= fromDate.Date 
                                && slip.Pmt_Date <= toDate.Date 
                                && slip.Mem_Id == empId
                                && !slip.Pay_Delete
                                && slip.BrCode == brCode 
                                && init.Pay_Des == payDesc
                                && !init.Pay_Delete
                                && init.BrCode == brCode 
                             select slip).CountAsync();
                if(count >0 ) { result = true; } else { result = false; }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result ;
        }
        #endregion
    }
}
