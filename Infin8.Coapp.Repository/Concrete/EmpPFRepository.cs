using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Drawing;
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
                CSISContext.SaveChanges();
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

        public async Task<List<DtoEmpPf>> CalculatePFInterestYearEnd(List<EmployeeMasterDto> empList, DateTime fromDate, 
            DateTime toDate,decimal usrId, decimal Yrid, string brCode)
        {
            decimal vocId = 0;
            string sql = "";
            //double intCalc = 0;
            double  PFIntCalc =0, SPFIntCalc = 0;
            double PFOS = 0, VPFOS = 0, SPFOS = 0, PFProduct = 0, SPFProduct = 0, PFInt = 0, SPFInt = 0,
                    PFIntTot = 0, SPFIntTot = 0;
            int NoDays = 0;
            decimal PFId = 0;
            DateTime TmpFromDate = DateTime.Now;
            List<DtoEmpPf> pfList = new();
            //DtoEmpPf pf = new();
            List<Fin_Voucher_Trn> vocTrnList = new ();
            try
            {
                foreach (var emp in empList)
                {
                    var pfbalance = await CSISContext.Emp_Pf.Where(x => x.Mem_Id == emp.Mem_Id && x.Pf_Delete == false && x.Pf_Date < fromDate)
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
                            maxIntCalcDate = g.Max(x => x.Last_Int_ApplicationDate),
                            minPF_Date = g.Min(x => x.Pf_Date)
                        }).FirstOrDefaultAsync();
                    if (pfbalance != null)
                    {
                        PFOS = pfbalance.pf_subscription - pfbalance.pf_withdrawn;
                        VPFOS = pfbalance.vpf_contribution - pfbalance.vpf_withdrawn;
                        SPFOS = pfbalance.bpf_contribution - pfbalance.bpf_withdrawn;
                        if (pfbalance.maxIntCalcDate != null)
                            TmpFromDate = (DateTime)pfbalance.maxIntCalcDate;
                        else
                            TmpFromDate = (DateTime)pfbalance.minPF_Date!;
                    }
                    else
                    {
                        TmpFromDate = fromDate.Date;
                    }
                    var empPF = CSISContext.Emp_Pf.Where(x => x.Mem_Id == emp.Mem_Id && x.Pf_Delete == false && (x.Pf_Date >= TmpFromDate.Date && x.Pf_Date < toDate.Date)).OrderBy(x => new { x.Pf_Date, x.Status, x.SlNo }).ToList();
                    foreach (var single in empPF)
                    {
                        PFId = single.Pf_Id;
                        NoDays = Utilities.GetNoOfDays(((DateTime)single.Pf_Date!).Date, TmpFromDate.Date);
                        PFProduct = (PFOS + VPFOS) * NoDays;
                        SPFProduct = SPFOS * NoDays;
                        if (NoDays > 0)
                            PFInt = Calc_Int_On_PF(PFOS + VPFOS, TmpFromDate.Date, ((DateTime)single.Pf_Date).Date);
                        else
                            PFInt = 0;
                        if (NoDays > 0)
                            SPFInt = Calc_Int_On_PF(SPFOS, TmpFromDate.Date, ((DateTime)single.Pf_Date).Date);
                        else
                            SPFInt = 0;
                        PFIntTot += PFInt;
                        SPFIntTot += SPFInt;
                        TmpFromDate = (DateTime)single.Pf_Date;
                        PFOS += single.Pf_Subscription - single.Pf_Withdrawn;
                        VPFOS += single.Vpf_Contribution - single.Vpf_Withdrawn;
                        SPFOS += single.Bpf_Contribution - single.Bpf_Withdrawn;

                        /// construct pf object
                        DtoEmpPf  pf = new()
                        {
                            PF_Id = PFId,
                            mem_id = emp.Mem_Id,
                            no_of_days = NoDays,
                            epf_Interest = PFInt,
                            bpf_Interest = SPFInt,
                            pf_Product = PFProduct,
                            bpf_product = SPFProduct,
                            int_calc_upto = TmpFromDate.Date,
                            pf_balance = PFOS,
                            vpf_balance = VPFOS,
                            bpf_balance = SPFOS,
                        };

                        pfList.Add(pf);
                    }
                    NoDays = Utilities.GetNoOfDays(toDate.Date, TmpFromDate.Date);
                    PFProduct = (PFOS + VPFOS) * NoDays;
                    SPFProduct = SPFOS * NoDays;
                    PFInt = Calc_Int_On_PF(PFOS + VPFOS, TmpFromDate.Date, toDate.Date);

                    SPFInt = Calc_Int_On_PF(SPFOS, TmpFromDate.Date, toDate.Date);
                    DtoEmpPf  pf1 = new()
                    {
                        PF_Id = PFId,
                        mem_id = emp.Mem_Id,
                        no_of_days = NoDays,
                        epf_Interest = PFInt,
                        bpf_Interest = SPFInt,
                        pf_Product = PFProduct,
                        bpf_product = SPFProduct,
                        int_calc_upto = toDate.Date,
                        pf_balance = PFOS,
                        vpf_balance = VPFOS,
                        bpf_balance = SPFOS,
                    };

                    pfList.Add(pf1);
                    PFIntTot += PFInt;
                    SPFIntTot += SPFInt;
                    PFIntCalc = Math.Round(PFIntTot, 0);
                    SPFIntCalc = Math.Round(SPFIntTot, 0);
                }
            }
            catch (Exception)
            {
                pfList = new();
                //throw;
            }
            return pfList;
        }

        public async Task<bool> SavePFInterestYearEnd(List<DtoEmpPf> pfList)
        {
            bool result = false;
            string sql = "";

            try
            {
                foreach(var pf in pfList )
                {
                    sql = "UPDATE Emp_pf set no_of_days = @noDays, epf_Interest = @pfInt, bpf_Interest = @spfInt, pf_Product = @pfProduct, bpf_product = @spfProduct, int_calc_upto = @intCalcUpto, pf_balance = @pfOS, vpf_balance = @vpfOS, bpf_balance = @spfOS WHERE PF_Id = @pfId";
                    await CSISContext.Database.ExecuteSqlRawAsync(sql
                    , new NpgsqlParameter("@noDays", pf.no_of_days)
                    , new NpgsqlParameter("@pfInt", pf.epf_Interest)
                    , new NpgsqlParameter("@spfInt", pf.bpf_Interest)
                    , new NpgsqlParameter("@pfProduct", pf.pf_Product)
                    , new NpgsqlParameter("@spfProduct", pf.bpf_product)
                    , new NpgsqlParameter("@intCalcUpto", pf.int_calc_upto)
                    , new NpgsqlParameter("@pfOS", pf.pf_balance)
                    , new NpgsqlParameter("@vpfOS", pf.vpf_balance)
                    , new NpgsqlParameter("@spfOS", pf.bpf_balance)
                    , new NpgsqlParameter("@pfId", pf.PF_Id));
                }
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private double Calc_Int_On_PF(double PFAmt, DateTime FromDate, DateTime ToDate)
        {
            double intCalcAmt = 0;
            double roi = 0;
            DateTime tmpToDate;
            int noOfDays = 0;
            //List<RateOfInterestVM> roilist = new();
            try
            {
                //roi = GetPFROI(FromDate, context, out errorMessage);
                //if (errorMessage.Length > 0)
                //{
                //    intCalcAmt = 0;
                //    return intCalcAmt;
                //}
                //roilist = GetPFROI(FromDate, ToDate, context, out errorMessage);
                //if (errorMessage.Length > 0)
                //{
                //    intCalcAmt = 0;
                //    return intCalcAmt;
                //}

                //var roiList  = CSISContext.Pay_PF_ROITemplate.Where(x=> x.Roi_Wef <= FromDate && x.Roi_Wef >= ToDate)
                //        .Select new RateOfInterestVM
                //        ({
                //            Roi = x.roi,
                //            Pi = x.Pi,
                //            Roi_Wef = x.Roi_Wef
                //        }).ToList();

                roi =  (from r in CSISContext.Pay_PF_ROITemplate
                     where r.Roi_Delete == false
                           && r.Roi_Wef ==
                              CSISContext.Pay_PF_ROITemplate
                                  .Where(x => x.Roi_Delete == false && x.Roi_Wef <= FromDate)
                                  .Max(x => x.Roi_Wef)
                     select r.Roi)
                    .FirstOrDefault();

                var roiList = (from pfInt in CSISContext.Pay_PF_ROITemplate
                               where pfInt.Roi_Wef <= FromDate && pfInt.Roi_Wef >= ToDate 
                               && pfInt.Roi_Delete == false
                               select new RateOfInterestVM
                               {
                                   Roi = pfInt.Roi,
                                   Roi_Wef = FromDate,
                               }).ToList();

                foreach (var single in roiList)
                {
                    roi = single.Roi;
                    tmpToDate = single.Roi_Wef;
                    noOfDays = (int)(tmpToDate - FromDate).TotalDays;
                    if (noOfDays <= 0) noOfDays = 0;
                    intCalcAmt += Math.Round((PFAmt * (roi / 100)) / 365 * noOfDays, 2);
                    //intCalcAmt += Utilities.Calculate_Interest_RoundOffTwoDigits(PFAmt, roi, (int)(tmpToDate - FromDate).TotalDays);
                    roi = single.Roi;
                    FromDate = single.Roi_Wef;
                }
                noOfDays = (int)(ToDate - FromDate).TotalDays;
                if (noOfDays <= 0) noOfDays = 0;
                intCalcAmt += Math.Round((PFAmt * (roi / 100)) / 365 * noOfDays, 2);
                //intCalcAmt += Utilities.Calculate_Interest_RoundOffTwoDigits(PFAmt, roi, (int)(ToDate - FromDate).TotalDays);
            }
            catch (Exception ex)
            {
                intCalcAmt = 0;
                //errorMessage = ex.Message;
            }
            return intCalcAmt;
        }

        
    }
}
