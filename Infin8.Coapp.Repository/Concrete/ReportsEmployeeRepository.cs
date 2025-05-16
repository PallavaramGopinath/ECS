using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
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

        public async Task<List<rptEmpPayBill>> GetEmpPayBill(decimal empId, decimal payId)
        {
            List<rptEmpPayBill> payList = new List<rptEmpPayBill>();
            try
            {
                payList = await CSISContext.Database.SqlQueryRaw<rptEmpPayBill>(
                        @"SELECT
	                    Emp_Master.Mem_Id,
	                    Mem_Master.memberName,Emp_Master.Emp_Desgn,Mem_Master.DOJ,Emp_Master.Emp_Scale,
	                    Pay_init.Pay_Month, Pay_init.Pay_Year,
	                    Mem_Master.dob,Mem_Master.DOR,
                        Pay_Slip.Pay_Basic_Earned, Pay_Slip.Pay_GradePay_Earned, Pay_Slip.Pay_DA_Earned, 
	                    Pay_Slip.Pay_PF, Pay_Slip.Pay_VPF, 
	                    Pay_Slip.Pay_Tot_Allowance, 
	                    Pay_Slip.Pay_Tot_Deductions, Pay_Slip.Pay_Net,
	                    Pay_All_Ded_Master.All_Name,Pay_Slip_Trn.Allowance_Amt, 
	                    Pay_All_Ded_Master_Ded.All_Name,Pay_Slip_Trn.Deduction_Amt,
                        Fin_Ledger.Led_Name,
                        Loan_Schemes.Scheme_Name,
                        Pay_Slip_Trn.Ded_Id, Pay_Slip_Trn.Loan_id, Pay_Slip_Trn.Led_Id
                    From
                       Pay_Slip INNER JOIN Pay_init  ON Pay_Slip.Pay_Id = Pay_init.Pay_Id
                         INNER JOIN Mem_Master  ON Pay_Slip.mem_Id = Mem_Master.mem_Id
                         INNER JOIN Pay_Slip_Trn  ON Pay_Slip.Pay_Id = Pay_Slip_Trn.Pay_Id AND Pay_Slip.mem_Id = Pay_Slip_Trn.mem_id
                         LEFT OUTER JOIN Pay_All_Ded_Master  ON Pay_Slip_Trn.All_Id = Pay_All_Ded_Master.All_Id
                         LEFT OUTER JOIN Loan_Master  ON Pay_Slip_Trn.Loan_id = Loan_Master.Loan_id 
	                     INNER JOIN Emp_Master  ON Mem_Master.mem_Id = Emp_Master.Mem_Id
                         LEFT OUTER JOIN Fin_Ledger  ON Pay_Slip_Trn.Led_Id = Fin_Ledger.Led_Id
                         LEFT OUTER JOIN Pay_All_Ded_Master AS Pay_All_Ded_Master_Ded ON Pay_Slip_Trn.Ded_Id = Pay_All_Ded_Master_Ded.All_Id
                         LEFT OUTER JOIN Loan_Schemes  ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id
                    WHERE Emp_Master.Mem_Id = @empId AND Pay_Slip.Pay_Id = @payId 
                    AND Pay_Slip.Pay_Delete = FALSE AND Pay_Slip_Trn.PayTr_Delete = FALSE
                    AND Pay_Slip.voc_status = 'V' AND Pay_Slip_Trn.voc_status = 'V'
                    AND Pay_Slip.mem_Id = @empId AND Pay_Slip_Trn.mem_id = @empId
                    Order By
                        Emp_Master.Mem_Id ASC , Pay_Slip_Trn.Pay_tr_Id ASC"
                    , new NpgsqlParameter("@empId", empId)
                    , new NpgsqlParameter("@payId", payId)).ToListAsync();
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

        public async Task<(List<rptEmpPF> pfList , string rateList)> GetEmpPFLedger(decimal empId, DateTime fromDate, DateTime toDate)
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

                pfList = await  CSISContext.Database.SqlQueryRaw<rptEmpPF>(
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
            return (pfList,rateList);
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
                payList = await   CSISContext.Database.SqlQueryRaw<rptEmp12MonthsSalary>(
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
