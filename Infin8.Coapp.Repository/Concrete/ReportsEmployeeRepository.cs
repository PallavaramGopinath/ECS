using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class ReportsEmployeeRepository : Repository<Reports_Master>, IReportsEmployeeRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsEmployeeRepository(CSISContext context) : base(context)
        {
        }
        public class PayBillItem
        {
            public decimal Mem_Id { get; set; }
            public decimal Pay_Id { get; set; }
            public int Pay_Component_Type { get; set; }
            public int Display_Order { get; set; }
            public string? AllowanceName { get; set; }
            public double AllowanceAmount { get; set; }
            public string? DeductionName { get; set; }
            public double DeductionAmount { get; set; }
        }
        public class PayItem
        {
            public int Pay_Component_Type { get; set; }
            public int Display_Order { get; set; }
            public string? ItemName { get; set; }
            public double Amount { get; set; }
        }
        public async Task<List<rptEmpPayBill>> GetEmpPayBill_old(decimal empId, decimal payId)
        {
            List<rptEmpPayBill> payList = new List<rptEmpPayBill>();
            try
            {
                #region Old Code query
                //payList = await CSISContext.Database.SqlQueryRaw<rptEmpPayBill>(
                //        @"SELECT
                //     Emp_Master.Mem_Id,
                //     Mem_Master.memberName,Emp_Master.Emp_Desgn,Mem_Master.DOJ,Emp_Master.Emp_Scale,
                //     Pay_init.Pay_Month, Pay_init.Pay_Year,
                //     Mem_Master.dob,Mem_Master.DOR,
                //        Pay_Slip.Pay_Basic_Earned, Pay_Slip.Pay_GradePay_Earned, Pay_Slip.Pay_DA_Earned, 
                //     Pay_Slip.Pay_PF, Pay_Slip.Pay_VPF, 
                //     Pay_Slip.Pay_Tot_Allowance, 
                //     Pay_Slip.Pay_Tot_Deductions, Pay_Slip.Pay_Net,
                //     Pay_All_Ded_Master.All_Name,Pay_Slip_Trn.Allowance_Amt, 
                //     Pay_All_Ded_Master_Ded.All_Name,Pay_Slip_Trn.Deduction_Amt,
                //        Fin_Ledger.Led_Name,
                //        Loan_Schemes.Scheme_Name,
                //        Pay_Slip_Trn.Ded_Id, Pay_Slip_Trn.Loan_id, Pay_Slip_Trn.Led_Id
                //    From
                //       Pay_Slip INNER JOIN Pay_init  ON Pay_Slip.Pay_Id = Pay_init.Pay_Id
                //         INNER JOIN Mem_Master  ON Pay_Slip.mem_Id = Mem_Master.mem_Id
                //         INNER JOIN Pay_Slip_Trn  ON Pay_Slip.Pay_Id = Pay_Slip_Trn.Pay_Id AND Pay_Slip.mem_Id = Pay_Slip_Trn.mem_id
                //         LEFT OUTER JOIN Pay_All_Ded_Master  ON Pay_Slip_Trn.All_Id = Pay_All_Ded_Master.All_Id
                //         LEFT OUTER JOIN Loan_Master  ON Pay_Slip_Trn.Loan_id = Loan_Master.Loan_id 
                //      INNER JOIN Emp_Master  ON Mem_Master.mem_Id = Emp_Master.Mem_Id
                //         LEFT OUTER JOIN Fin_Ledger  ON Pay_Slip_Trn.Led_Id = Fin_Ledger.Led_Id
                //         LEFT OUTER JOIN Pay_All_Ded_Master AS Pay_All_Ded_Master_Ded ON Pay_Slip_Trn.Ded_Id = Pay_All_Ded_Master_Ded.All_Id
                //         LEFT OUTER JOIN Loan_Schemes  ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id
                //    WHERE Emp_Master.Mem_Id = @empId AND Pay_Slip.Pay_Id = @payId 
                //    AND Pay_Slip.Pay_Delete = FALSE AND Pay_Slip_Trn.PayTr_Delete = FALSE
                //    AND Pay_Slip.voc_status = 'V' AND Pay_Slip_Trn.voc_status = 'V'
                //    AND Pay_Slip.mem_Id = @empId AND Pay_Slip_Trn.mem_id = @empId
                //    Order By
                //        Emp_Master.Mem_Id ASC , Pay_Slip_Trn.Pay_tr_Id ASC"
                //    , new NpgsqlParameter("@empId", empId)
                //    , new NpgsqlParameter("@payId", payId)).ToListAsync();
                #endregion

                #region linq query
                var query = await
                    (from paySlip in CSISContext.Pay_Slip
                     join payInit in CSISContext.Pay_Init on paySlip.Pay_Id equals payInit.Pay_Id
                     join memMaster in CSISContext.mem_master on paySlip.Mem_Id equals memMaster.mem_id
                     join paySlipTrn in CSISContext.Pay_Slip_Trn on
                         new { paySlip.Pay_Id, paySlip.Mem_Id } equals
                         new { paySlipTrn.Pay_Id, paySlipTrn.Mem_Id }
                     join empMaster in CSISContext.Emp_Master on memMaster.mem_id equals empMaster.Mem_Id
                     join payGenInfo in CSISContext.Pay_Gen_Info on empMaster.Emp_Desgn_Id equals payGenInfo.Pay_Info_Id
                     from payComponent in CSISContext.Pay_Components
                         .Where(c => c.Component_Id == paySlipTrn.Component_Id)
                         .DefaultIfEmpty()
                     from loanMaster in CSISContext.Loan_Master
                         .Where(l => l.Loan_Id == paySlipTrn.Loan_Id)
                         .DefaultIfEmpty()
                     from finLedger in CSISContext.Fin_Ledger
                         .Where(l => l.Led_Id == paySlipTrn.Led_Id)
                         .DefaultIfEmpty()
                     from loanScheme in CSISContext.Loan_Schemes
                         .Where(s => s.Scheme_Id == (loanMaster != null ? loanMaster.Scheme_Id : (int?)null))
                         .DefaultIfEmpty()
                     where empMaster.Mem_Id == empId
                         && paySlip.Pay_Id == payId
                         && paySlip.Pay_Delete == false
                         && paySlipTrn.PayTr_Delete == false
                         && paySlip.Mem_Id == empId
                         && paySlipTrn.Mem_Id == empId
                     orderby empMaster.Mem_Id ascending, paySlipTrn.Pay_tr_Id ascending
                     select new rptEmpPayBill
                     {
                         Mem_Id = empMaster.Mem_Id,
                         MemberName = memMaster.membername,
                         Emp_Desgn = payGenInfo.Pay_Info_Name,
                         Doj = memMaster.doj,
                         Emp_Scale = empMaster.Emp_Scale,
                         Pay_Month = payInit.Pay_Month,
                         Pay_Year = payInit.Pay_Year,
                         Dob = memMaster.dob,
                         Dor = memMaster.dor,
                         Pay_Basic_Earned = paySlip.Pay_Basic_Earned,
                         Pay_GradePay_Earned = paySlip.Pay_GradePay_Earned,
                         Pay_DA_Earned = paySlip.Pay_DA_Earned,
                         Pay_PF = paySlip.Pay_PF,
                         Pay_VPF = paySlip.Pay_VPF,
                         Pay_Tot_Allowance = paySlip.Pay_Tot_Allowance,
                         Pay_Tot_Deductions = paySlip.Pay_Tot_Deductions,
                         Pay_Net = paySlip.Pay_Net,
                         All_Name = payComponent != null ? payComponent.Component_Name : null,
                         Allowance_Amt = paySlipTrn.Allowance_Amt,
                         Pay_Component_Type = paySlipTrn.Pay_Component_Type,
                         Deduction_Amt = paySlipTrn.Deduction_Amt,
                         Led_Name = finLedger != null ? finLedger.Led_Name : null,
                         Scheme_Name = loanScheme != null ? loanScheme.Scheme_Name : null,
                         Ded_Id = paySlipTrn.Ded_Id,
                         Loan_Id = paySlipTrn.Loan_Id,
                         Led_Id = paySlipTrn.Led_Id
                     }).ToListAsync();
                if (query != null && query.Any())
                {
                    payList = query.ToList();

                }
                else
                {
                    payList = new List<rptEmpPayBill>();
                }
                #endregion 


                //string rsInWords = "";
                //if (payList != null)
                //{
                //    if (payList.Count > 0)
                //    {
                //        var lastItem = payList.LastOrDefault();
                //        rsInWords = Utilities.RupeesInWords(lastItem.Pay_Net);

                //        foreach (var pay in payList)
                //        {
                //            pay.RsInWords = rsInWords;
                //        }
                //    }
                //    else
                //    {
                //        rsInWords = "Zero";
                //    }
                //}
                //else
                //{
                //    rsInWords = "Zero";
                //}
            }
            catch (Exception)
            {
                throw;
            }
            return payList;
        }
        public async Task<List<rptEmpPayBill>> GetEmpPayBill(decimal empId, decimal payId)
        {
            List<rptEmpPayBill> payList = new List<rptEmpPayBill>();
            try
            {
                var allowances = await GetAllowances(payId, empId);
                var deductions = await GetDeductions(payId, empId);

                var maxCount = Math.Max(allowances.Count, deductions.Count);
                var payBillItems = new List<PayBillItem>();

                for (int i = 0; i < maxCount; i++)
                {
                    var item = new PayBillItem
                    {
                        Mem_Id = empId,
                        Pay_Id = payId,
                        Pay_Component_Type = i < allowances.Count ? allowances[i].Pay_Component_Type : (i < deductions.Count ? deductions[i].Pay_Component_Type : 0),
                        Display_Order = i < allowances.Count ? allowances[i].Display_Order : (i < deductions.Count ? deductions[i].Display_Order : 0),  
                        AllowanceName = i < allowances.Count ? allowances[i].ItemName : "",
                        AllowanceAmount = i < allowances.Count ? allowances[i].Amount : 0,
                        DeductionName = i < deductions.Count ? deductions[i].ItemName : "",
                        DeductionAmount = i < deductions.Count ? deductions[i].Amount : 0
                    };
                    payBillItems.Add(item);
                }

                var empPayBillItems = (from item in payBillItems
                                      join emp in CSISContext.Emp_Master on item.Mem_Id equals emp.Mem_Id
                                      join mem in CSISContext.mem_master on emp.Mem_Id equals mem.mem_id
                                      join pay in CSISContext.Pay_Slip on new { item.Pay_Id, item.Mem_Id } equals new { pay.Pay_Id, pay.Mem_Id }
                                      join payInit in CSISContext.Pay_Init on item.Pay_Id equals payInit.Pay_Id
                                      join info in CSISContext.Pay_Gen_Info on emp.Emp_Desgn_Id equals info.Pay_Info_Id
                                      orderby item.Pay_Component_Type ascending, item.Display_Order ascending
                                      select new rptEmpPayBill
                                      {
                                          Mem_Id = item.Mem_Id,
                                          MemberName = mem.membername,
                                          Emp_Desgn = info.Pay_Info_Name,
                                          Doj = mem.doj,
                                          Emp_Scale = emp.Emp_Scale,
                                          Pay_Month = payInit.Pay_Month,
                                          Pay_Year = payInit.Pay_Year,
                                          Dob = mem.dob,
                                          Dor = mem.dor,
                                          Pay_Basic_Earned = pay.Pay_Basic_Earned,
                                          Pay_GradePay_Earned = pay.Pay_GradePay_Earned,
                                          Pay_DA_Earned = pay.Pay_DA_Earned,
                                          Pay_PF = pay.Pay_PF,
                                          Pay_VPF = pay.Pay_VPF,
                                          Pay_Tot_Allowance = pay.Pay_Tot_Allowance,
                                          Pay_Tot_Deductions = pay.Pay_Tot_Deductions,
                                          Pay_Net = pay.Pay_Net,
                                          Allowance_Amt = item.AllowanceAmount,
                                          All_Name = item.AllowanceName,
                                          Deduction_Amt = item.DeductionAmount,
                                          Decution_Name = item.DeductionName,
                                          Pay_Component_Type = item.Pay_Component_Type
                                      }).ToList();
                if(empPayBillItems != null && empPayBillItems.Any())
                {
                    payList =  empPayBillItems.ToList();
                }
                else
                {
                    payList = new List<rptEmpPayBill>();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return payList;
        }

        private async Task<List<PayItem>> GetAllowances(decimal payId, decimal memId)
        {
            var query = await (from trn in CSISContext.Pay_Slip_Trn
                               join comp in CSISContext.Pay_Components on trn.Component_Id equals comp.Component_Id
                               where trn.Pay_Id == payId
                                   && trn.Mem_Id == memId
                                   && trn.Pay_Component_Type == 1
                                   && trn.PayTr_Delete == false
                               orderby trn.Component_Id
                               select new PayItem
                               {
                                   Pay_Component_Type = trn.Pay_Component_Type,
                                   Display_Order = comp.Display_Order,
                                   ItemName = comp.Component_Name,
                                   Amount = trn.Allowance_Amt
                               }).ToListAsync(); ;

            return query.ToList();
        }

        private async  Task<List<PayItem>> GetDeductions(decimal payId, decimal memId)
        {
            var componentDeductions = await (from trn in CSISContext.Pay_Slip_Trn
                                      join comp in CSISContext.Pay_Components on trn.Component_Id equals comp.Component_Id
                                      where trn.Pay_Id == payId
                                          && trn.Mem_Id == memId
                                          && trn.Pay_Component_Type == 2
                                          && trn.PayTr_Delete == false
                                      select new PayItem
                                      {
                                          Pay_Component_Type = trn.Pay_Component_Type,
                                          Display_Order = comp.Display_Order,
                                          ItemName = comp.Component_Name,
                                          Amount = trn.Deduction_Amt
                                      }).ToListAsync ();
            int dispayOrder = 100;
            var loanDeductions = await (from trn in CSISContext.Pay_Slip_Trn
                                        join loan in CSISContext.Loan_Master on trn.Loan_Id equals loan.Loan_Id
                                        join scheme in CSISContext.Loan_Schemes on loan.Scheme_Id equals scheme.Scheme_Id
                                        where trn.Pay_Id == payId
                                            && trn.Mem_Id == memId
                                            && trn.Pay_Component_Type == 3
                                            && trn.PayTr_Delete == false
                                        select new PayItem
                                        {
                                            Pay_Component_Type = trn.Pay_Component_Type,
                                            Display_Order = dispayOrder,
                                            ItemName = scheme.Scheme_Name,
                                            Amount = trn.Deduction_Amt,

                                        }).ToListAsync() ;
            dispayOrder += 1;
            var ledgerDeductions = await (from trn in CSISContext.Pay_Slip_Trn
                                   join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                   where trn.Pay_Id == payId
                                       && trn.Mem_Id == memId
                                       && trn.Pay_Component_Type == 5
                                       && trn.PayTr_Delete == false
                                   select new PayItem
                                   {
                                       Pay_Component_Type = trn.Pay_Component_Type,
                                       Display_Order = dispayOrder,
                                       ItemName = ledger.Led_Name,
                                       Amount = trn.Deduction_Amt
                                   }).ToListAsync();

            return componentDeductions
                .Concat(loanDeductions)
                .Concat(ledgerDeductions)
                .OrderBy(x => x.Pay_Component_Type)
                .ThenBy (x=> x.Display_Order)
                .ToList();
        }
        public async Task<(List<rptEmpPF> pfList, string rateList)> GetEmpPFLedger(decimal empId, DateTime fromDate, DateTime toDate)
        {
            List<rptEmpPF> pfList = new List<rptEmpPF>();
            string rateList = "";
            try
            {
                if (!Update_PFBalance(empId, fromDate, toDate))
                {

                }
                List<RateOfInterestVM> roiList = new List<RateOfInterestVM>();
                roiList = GetPFRoi(fromDate, toDate);

                foreach (var roi in roiList)
                {
                    rateList += "With effect from :" + roi.Roi_Wef.ToString("dd/MM/yyyy") + " Rate of interest :" + roi.Roi.ToString() + "\n";
                }

                pfList = await CSISContext.Database.SqlQueryRaw<rptEmpPF>(
                    @"SELECT
                            Mem_Master.memberName, Mem_Master.memberNo,
                            Emp_pf.pf_date, 
	                        Emp_pf.pf_subscription + Emp_pf.vpf_contribution AS pf_subscription, 
	                        Emp_pf.pf_withdrawn + Emp_pf.vpf_withdrawn AS pf_withdrawn,
	                        Emp_pf.pf_balance + Emp_pf.vpf_balance AS pf_balance,
	                        Emp_pf.epf_Interest,
	                        Emp_pf.no_of_days, 
	                        Emp_pf.pf_Product, 
	                        Emp_pf.bpf_contribution, Emp_pf.bpf_withdrawn, Emp_pf.bpf_balance, Emp_pf.bpf_product,  
	                        Emp_pf.bpf_Interest,
                            Pay_Gen_Info.Pay_Info_Name
                        FROM
                            ((Mem_Master  INNER JOIN Emp_Master Emp_Master ON
                                Mem_Master.mem_Id = Emp_Master.Mem_Id)
                             INNER JOIN Pay_Gen_Info  ON
                                Emp_Master.Emp_Desgn_Id = Pay_Gen_Info.Pay_Info_Id)
                             INNER JOIN Emp_pf  ON
                                Emp_Master.Mem_Id = Emp_pf.Emp_Id
                        Where Emp_pf.Mem_Id = @empId AND Emp_pf.pf_Delete = FALSE
                        AND Emp_pf.voc_status = 'V'
                        And CAST(Emp_pf.pf_date as date) BETWEEN @fromDate AND @toDate 
                        ORDER BY Emp_pf.pf_date ASC, Emp_pf.status ASC,Emp_pf.slno ASC"
                , new NpgsqlParameter("@empId", empId)
                , new NpgsqlParameter("@fromDate", fromDate)
                , new NpgsqlParameter("@toDate", toDate)).ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
            return (pfList, rateList);
        }

        private bool Update_PFBalance(decimal empId, DateTime fromDate, DateTime toDate)
        {
            bool result = true;
            double PFOS = 0, VPFOS = 0, SocPFOS = 0;
            string sql = "";
            try
            {

                var pf = CSISContext.Database.SqlQueryRaw<rptEmpPFOB>(
                    @"SELECT 
                        Sum(pf_subscription) - Sum(pf_withdrawn) AS pf_balance, 
                        Sum(vpf_contribution) - Sum(vpf_withdrawn) AS vpf_balance, 
                        Sum(bpf_contribution) - Sum(bpf_withdrawn) AS bpf_balance, 
                        Sum(epf_Interest)  + Sum(vpf_Interest)  - Sum(epf_Int_withdrawn) AS epf_Int_Balance,
                        Sum(bpf_Interest)  - Sum(bpf_Int_withdrawn) AS bpf_Int_Balance
                        From Emp_pf 
                        Where pf_date :: DATE <@fromDate And pf_Delete = FALSE AND Emp_pf.voc_status = 'V'  AND Mem_Id = @empId"
                    , new NpgsqlParameter("@fromDate", fromDate.Date)
                    , new NpgsqlParameter("@empId", empId)).FirstOrDefault();
                if (pf != null)
                {
                    PFOS = pf.Pf_Balance != null ? (double)pf.Pf_Balance : 0;
                    VPFOS = pf.Vpf_Balance != null ? (double)pf.Vpf_Balance : 0;
                    SocPFOS = pf.Bpf_Balance != null ? (double)pf.Bpf_Balance : 0;
                }
                List<rptEmpPF> pfList = CSISContext.Database.SqlQueryRaw<rptEmpPF>(
                    @"Select pf_id,
                        pf_date,
                        pf_subscription,pf_withdrawn,pf_balance,no_of_days,pf_Product,
                        vpf_contribution,vpf_withdrawn,vpf_balance,vpf_product,
                        bpf_contribution,bpf_withdrawn,bpf_balance,bpf_product,
                        epf_interest,bpf_Interest,epf_Int_withdrawn,pf_Int_withdrawn,bpf_Int_withdrawn,
                        int_Calc_Upto,Status from Emp_Pf 
                        where pf_Delete = FALSE AND emp_pf.voc_status = 'V' AND mem_Id = @empId
                        AND CAST(pf_date  as date) BETWEEN @fromDate AND @toDate ORDER BY pf_Date asc,Status asc,slno asc"
                    , new NpgsqlParameter("@empId", empId)
                    , new NpgsqlParameter("@fromDate", fromDate.Date)
                    , new NpgsqlParameter("@toDate", toDate.Date)).ToList();
                if (pfList != null)
                {
                    foreach (var single in pfList)
                    {
                        PFOS += single.Pf_Subscription - single.Pf_Withdrawn;
                        VPFOS += single.Vpf_Contribution - single.Vpf_Withdrawn;
                        SocPFOS += single.Bpf_Contribution - single.Bpf_Withdrawn;
                        sql = "UPDATE Emp_Pf SET pf_balance = @PFOS, vpf_balance = @VPFOS, bpf_balance = @SocPFOS WHERE pf_Id = @pfId";
                        CSISContext.Database.ExecuteSqlRaw(sql
                            , new NpgsqlParameter("@PFOS", PFOS)
                            , new NpgsqlParameter("@VPFOS", VPFOS)
                            , new NpgsqlParameter("@SocPFOS", SocPFOS)
                            , new NpgsqlParameter("@pfId", single.Pf_Id));
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }
        private List<RateOfInterestVM> GetPFRoi(DateTime fromDate, DateTime toDate)
        {
            List<RateOfInterestVM> roiList = new List<RateOfInterestVM>();
            try
            {
                roiList = CSISContext.Database.SqlQueryRaw<RateOfInterestVM>(
                    @"With Dt1
                        AS
                        (
                        SELECT CAST(roi_wef as date) AS roi_wef,roi From Pay_PF_ROITemplate WHERE roi_wef =
                        (select max(roi_wef) from  Pay_PF_ROITemplate where CAST(roi_wef as date) < @fromDate)
                        UNION
                        SELECT CAST(roi_wef as date) AS roi_wef,roi From Pay_PF_ROITemplate WHERE roi_Delete = FALSE AND
                        CAST(roi_wef as date) BETWEEN @fromDate AND @toDate  
                        )
                        SELECT * from Dt1 ORDER BY Dt1.roi_wef"
                    , new NpgsqlParameter("@fromDate", fromDate)
                    , new NpgsqlParameter("@toDate", toDate)).ToList();
            }
            catch (Exception)
            {
            }
            return roiList;
        }

        public async Task<List<rptEmp12MonthsSalary>> GetEmployee12MonthsSalary(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptEmp12MonthsSalary> payList = new List<rptEmp12MonthsSalary>();
            try
            {
                payList = await CSISContext.Database.SqlQueryRaw<rptEmp12MonthsSalary>(
                        @"SELECT CONCAT(TO_CHAR(DATE_TRUNC('month', MAKE_DATE(Pay_init.Pay_Year, Pay_init.pay_month, 1) - INTERVAL '1 month'), 'Month'), ' ', Pay_init.Pay_Year) AS SalaryMonth,
                        Pay_init.pay_month,
                        Pay_init.Pay_Year ,Pay_init.pay_des, pay_slip.Mem_Id, Mem_Master.memberNo, Mem_Master.memberName, Emp_Master.Emp_Desgn, 
                        Pay_Basic_Earned,Pay_GradePay_Earned, Pay_PP_Earned, Pay_DA_Earned, 
                        Pay_Tot_Allowance - (Pay_Basic_Earned + Pay_GradePay_Earned + Pay_PP_Earned + Pay_DA_Earned) AS OtherAllowances,
                        Pay_Tot_Allowance,
                        Pay_PF, Pay_VPF, 
                        Pay_Tot_Deductions - ( Pay_PF + Pay_VPF) AS OtherDeductions,
                        Pay_Tot_Deductions,
                        Pay_Net,
                        Pay_SLS, Pay_ExGratia,Pay_Bonus 
                        FROM pay_slip 
                        INNER JOIN Mem_Master ON Pay_Slip.Mem_Id = Mem_Master.mem_Id 
                        INNER JOIN Emp_Master ON Pay_Slip.Mem_Id = Emp_Master.Mem_Id 
                        INNER JOIN Pay_init ON Pay_init.Pay_Id = Pay_Slip.Pay_Id 
                        WHERE pay_slip.Pay_Delete = FALSE AND Pay_init.Pay_Delete = FALSE 
                        AND Pay_slip.brcode = @brCode ANd pay_slip.voc_status = 'V'
                        AND Emp_Master.brcode = @brCode
                        AND pay_init.brcode = @brCode 
                        AND Pay_Slip.pmt_date BETWEEN @fromDate And @toDate
                        ORDER BY Pay_Slip.Mem_Id, Pay_init.Pay_Year, Pay_init.Pay_Month"
                        , new NpgsqlParameter("@fromDate", fromDate)
                        , new NpgsqlParameter("@toDate", toDate)).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return payList;
        }
    }
}
