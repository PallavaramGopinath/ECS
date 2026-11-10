using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infin8.Coapp.Repository
{
    public class RecurringDepositRepository : Repository<TermDeposit_Trn>, IRecurringDepositRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public RecurringDepositRepository(DbContext context) : base(context)
        {
        }
        //public Task<List<Mem_Demand>> GetRecurringDepositDemand(DateTime dueDate, string brCode)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task<List<Mem_Demand>> CalculateRecurringDepositDemand( decimal memId, DateOnly demandCalcDate, DateTime demandDate, string brCode)
        {
            int NoOfInstalmentsCompleted = 0;
            List<Mem_Demand> rdDemand = [];
            List<RDDetailsVM> rdDetails = [];
            List<decimal> TdIdList = [];

            try
            {
                var tdIdListResult = await  (from f in CSISContext.TermDeposit_Master
                             join g in CSISContext.TermDeposit_Schemes on f.TDScheme_Id equals g.TDScheme_Id
                             where f.Mem_Id == memId 
                                && f.TD_Delete == false
                                && f.AccountClosed == false
                                && f.Status == "D"
                                && g.TDSchemeType == "R"
                                && f.BrCode == brCode   
                                select f.TD_Id).ToListAsync();
                if (tdIdListResult == null && tdIdListResult!.Count == 0)
                {
                    return rdDemand;
                }

                decimal[] rdIds = [.. TdIdList];
                if (rdIds.Length == 0)
                {
                    return rdDemand;
                }
                rdDetails = await  GetRDDetails(rdIds, demandDate);
                if (rdDemand == null || rdDemand.Count == 0)
                {
                    return rdDemand!;
                }
                if (rdDetails != null)
                {
                    foreach (RDDetailsVM rd in rdDetails)
                    {
                        Mem_Demand single = new()
                        {
                            Id = 0,
                            Demand_Id = 0,
                            Calculated_Date = (DateOnly)demandCalcDate,
                            Demand_Type = "R",
                            Mem_Id = rd.Mem_Id,
                            Loan_Id = 0,
                            TD_Id = rd.TD_Id
                        };
                        if (rd.DepositAmount > 0)
                            single.RD_Instalment = Convert.ToInt32(rd.DepositAmount);
                        else
                            single.RD_Instalment  = 0;
                        if (demandDate > rd.MaturityDate)
                            NoOfInstalmentsCompleted = Utilities.GetMonthsBetween(rd.ValueDate, rd.MaturityDate);
                        else
                            NoOfInstalmentsCompleted = Utilities.GetMonthsBetween(rd.ValueDate, Utilities.GetLastDateOfMonth(demandDate));

                        if (rd.DepositAmount > 0 && rd.PendingRD > 0)
                            single.RD_NoOf_Instalments = Convert.ToInt16(rd.PendingRD / rd.DepositAmount);
                        else
                            single.RD_NoOf_Instalments = 0;
                        single.RD_Instalment  = Convert.ToInt32(rd.DepositAmount);
                        single.RD_Demand = Convert.ToInt32(rd.PendingRD);
                        single.RD_PI_Arrear  = Convert.ToInt32(rd.PenalCalculatedAmount - rd.PenalReceivedAmount);
                        single.RD_PI_Calc = Convert.ToInt32(rd.PICalc);
                        single.RD_Total  = single.RD_Demand + single.RD_PI_Arrear + single.RD_PI_Calc;
                        single.RD_PI_Application_Date = demandDate;
                        single.Total_Demand = single.RD_Total;
                        single.Usr_Id = 0;
                        single.Recovery_Id = 0;
                        single.Voc_Id = 0;
                        if (single.Total_Demand > 0)
                        {
                            rdDemand.Add(single);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
            return rdDemand;
        }

        public async Task< List<RDDetailsVM>> GetRDDetails(decimal[] RDNos, DateTime TrnDate)
        {
            List<RDDetailsVM> rdList = [];
            int NoOfInstalmentsCompleted = 0;
            double CompletedInstalmentAmt = 0;
            double PendingRDAmt = 0;
            double PICalcAmt = 0;
            DateTime? MaxPICalcDate = null;
            DateTime? PICalcDate = null;
            int NoOfMonthForPICalc = 0;
            double PIRate = 0;
            double RDAmt = 0;
            try
            {
                //rdList = dbTD.GetRDDetails(RDNos, out errorMessage);


                    // Assuming 'result' is a List<int> or array of TD_Id values
                    var query = await (from Master in CSISContext.TermDeposit_Master
                                join Trn in CSISContext.TermDeposit_Trn on Master.TD_Id equals Trn.TD_Id
                                where Trn.TD_Delete == false
                                   && Master.AccountClosed == false
                                   && RDNos.Contains(Master.TD_Id) // result is your list of IDs
                                group Trn by Master into g
                                where g.Sum(x => x.DepositPaidAmount) == 0
                                select new RDDetailsVM
                                {
                                    TD_Id = g.Key.TD_Id,
                                    TD_No = g.Key.TD_No,
                                    Mem_Id = g.Key.Mem_Id,
                                    TDScheme_Id = g.Key.TDScheme_Id,
                                    ValueDate = g.Key.ValueDate,
                                    DepositAmount = g.Key.DepositAmount,
                                    PeriodInMonths = g.Key.PeriodInMonths,
                                    RateOfInterest = g.Key.RateOfInterest,
                                    MaturityDate = g.Key.MaturityDate,
                                    IsCompoundInterest = g.Key.IsCompoundInterest,
                                    MaturityAmount = g.Key.MaturityAmount,
                                    NoOfInstalmentReceived = g.Sum(x => x.NoOfInstalments),
                                    RDAmountReceived = g.Sum(x => x.DepositReceiptAmount),
                                    LastInstalmentDate = g.Max(x => x.LastInstalmentDate),
                                    PenalCalculatedAmount = g.Sum(x => x.PenalCalculatedAmount),
                                    PenalAppliedDate = g.Max(x => x.PenalAppliedDate),
                                    PenalReceivedAmount = g.Sum(x => x.PenalReceivedAmount),
                                    LastReceiptDate = g.Max(x => x.Trn_Date)
                                }).ToListAsync();

                if (query == null && query!.Count ==0)
                {
                    return rdList;
                }
                else
                {
                    rdList = query?.ToList() ?? [];
                }
                foreach (RDDetailsVM rd in rdList)
                {
                    if (rd.PenalAppliedDate != null)
                        MaxPICalcDate = (DateTime)rd.PenalAppliedDate;
                    else
                        MaxPICalcDate = null;

                    if (rd.MaturityDate > TrnDate)
                    {
                        NoOfInstalmentsCompleted = Utilities.GetMonthsBetweenFinal(rd.ValueDate, Utilities.GetLastDateOfMonth(TrnDate)) + 1;
                    }
                    else
                        NoOfInstalmentsCompleted = rd.PeriodInMonths;
                    if (NoOfInstalmentsCompleted > rd.PeriodInMonths) { NoOfInstalmentsCompleted = rd.PeriodInMonths; }

                    
                    CompletedInstalmentAmt = NoOfInstalmentsCompleted * rd.DepositAmount;
                    if (rd.DepositAmount * rd.PeriodInMonths > rd.RDAmountReceived)
                        PendingRDAmt = rd.DepositAmount;
                    else
                        PendingRDAmt = 0;
                    RDAmt = rd.DepositAmount;
                    PICalcAmt = 0;
                    if (MaxPICalcDate != null)
                    {
                        if (TrnDate >= rd.MaturityDate)
                        {
                            NoOfMonthForPICalc = Utilities.GetMonthsBetween((DateTime)MaxPICalcDate, rd.MaturityDate);
                        }
                        else
                        {
                            NoOfMonthForPICalc = Utilities.GetMonthsBetween((DateTime)MaxPICalcDate, TrnDate);
                        }
                    }
                    else
                    {
                        if (rd.LastInstalmentDate != null)
                        {
                            if (TrnDate >= rd.MaturityDate)
                                NoOfMonthForPICalc = Utilities.GetMonthsBetweenFinal((DateTime)rd.LastInstalmentDate, TrnDate);
                            else
                                
                                NoOfMonthForPICalc = Utilities.GetMonthsBetweenFinal((DateTime)rd.ValueDate, TrnDate);
                        }
                        else
                            if (TrnDate >= rd.MaturityDate)
                                
                                NoOfMonthForPICalc = Utilities.GetMonthsBetweenFinal(rd.ValueDate, rd.MaturityDate);
                            else
                                
                                NoOfMonthForPICalc = Utilities.GetMonthsBetweenFinal(rd.ValueDate, TrnDate);
                    }
                    if (NoOfMonthForPICalc > 0)
                        PICalcDate = (DateTime)TrnDate;
                    //else

                    var maxWef = (from t in CSISContext.TermDeposit_Roi_Template
                                  where t.TDRoi_Delete == false
                                     && t.Wef <= rd.ValueDate
                                     && t.PeriodEnd >= rd.PeriodInMonths
                                     && t.PeriodBegin <= rd.PeriodInMonths
                                     && t.PeriodType == "M"
                                     && t.TDScheme_Id == rd.TDScheme_Id
                                  select t.Wef).Max();
                    // Now get the PenalRateForRD using the max wef
                    PIRate = (from t in CSISContext.TermDeposit_Roi_Template
                               where t.TDRoi_Delete == false
                                  && t.Wef <= rd.ValueDate
                                  && t.PeriodEnd >= rd.PeriodInMonths
                                  && t.PeriodBegin <= rd.PeriodInMonths
                                  && t.PeriodType == "M"
                                  && t.TDScheme_Id == rd.TDScheme_Id
                                  && t.Wef == maxWef
                               select t.PenalRateForRD).FirstOrDefault();

                    //PIRate = dbTD.GetPIFor_RD(rd.ValueDate, rd.TDScheme_Id, rd.PeriodInMonths, 0);

                    PICalcDate = null;
                    for (int i = 1; i <= (NoOfInstalmentsCompleted - 1) - rd.NoOfInstalmentReceived; i++)
                    {
                        PICalcAmt += PendingRDAmt * PIRate / 100 * 1;
                        PendingRDAmt += rd.DepositAmount;
                    }
                    if (PendingRDAmt < 0) PendingRDAmt = 0;
                    rd.PendingRD = PendingRDAmt;
                    rd.PICalc = PICalcAmt;
                    if (PICalcAmt > 0)
                        rd.PICalcDate = PICalcDate;
                    else
                        rd.PICalcDate = null;
                    rd.PendingRD = NoOfInstalmentsCompleted * rd.DepositAmount - rd.RDAmountReceived;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
            }
            return rdList;
        }
    }
}
