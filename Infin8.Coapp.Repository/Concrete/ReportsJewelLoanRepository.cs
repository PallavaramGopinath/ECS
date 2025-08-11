using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infin8.Coapp.Repository
{
    public class ReportsJewelLoanRepository : Repository<Reports_Master>, IReportsJewelLoanRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsJewelLoanRepository(CSISContext context) : base(context)
        {
        }

        public async Task<List<rptJewelLoanVerificationList>> GetJewelLoanVerificationList_old(DateTime asOnDate, string brCode)
        {
            List<rptJewelLoanVerificationList> verificationList = new List<rptJewelLoanVerificationList>();
            try
            {
                var query = await (from loanMaster in CSISContext.Loan_Master
                                   join loanTrn in CSISContext.Loan_Trn on loanMaster.Loan_Id equals loanTrn.Loan_Id
                                   join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                                   join jlDetails in CSISContext.JL_Details on loanTrn.Loan_Id equals jlDetails.Loan_Id
                                   where loanMaster.Loan_Type == 2
                                      && loanMaster.Loan_Delete == false
                                      && loanTrn.TrnTr_Delete == false
                                      && loanTrn.Trn_Date <= asOnDate
                                      && loanMaster.BrCode == brCode && loanMaster.Voc_Status =="V"
                                      && loanTrn.BrCode == brCode && loanTrn.Voc_Status == "V"
                                      && memMaster.brcode == brCode 
                                      && jlDetails.BrCode == brCode && jlDetails.Voc_Status == "V"
                                   group new { loanMaster, memMaster, loanTrn, jlDetails } by new
                                   {
                                       loanMaster.Loan_Id,
                                       memMaster.memberno,
                                       memMaster.perno,
                                       memMaster.membername,
                                       loanMaster.Loan_No,
                                       loanMaster.San_Date,
                                       jlDetails.GrossWeight,
                                       jlDetails.Wastage,
                                       jlDetails.NetWeight,
                                       jlDetails.NetValue,
                                       jlDetails.RatePerGram
                                   } into grouped
                                   let disbAmt = grouped.Sum(x => x.loanTrn.Disb_Amt)
                                   let loanOS = grouped.Sum(x => x.loanTrn.Disb_Amt) - grouped.Sum(x => x.loanTrn.PrlColl_Amt)
                                   where loanOS > 0
                                   orderby grouped.Key.Loan_No
                                   select new rptJewelLoanVerificationList
                                   {
                                       Loan_Id = grouped.Key.Loan_Id,
                                       MemberNo = grouped.Key.memberno,
                                       PerNo = grouped.Key.perno,
                                       MemberName = grouped.Key.membername,
                                       Loan_No = grouped.Key.Loan_No,
                                       San_Date = grouped.Key.San_Date,
                                       Disb_Amt = disbAmt,
                                       Loan_OS = loanOS,
                                       GrossWeight = grouped.Key.GrossWeight,
                                       Wastage = grouped.Key.Wastage,
                                       NetWeight = grouped.Key.NetWeight,
                                       NetValue = grouped.Key.NetValue,
                                       RatePerGram = grouped.Key.RatePerGram
                                   }).ToListAsync();

                // Execute the query
                //var results = query.ToList();
                if (query != null) verificationList = query;
            }
            catch (Exception)
            {

                throw;
            }
            return verificationList;
        }

        public async Task<List<rptJewelLoanVerificationList>> GetJewelLoanVerificationList(DateTime asOnDate, string brCode)
        {
            List<rptJewelLoanVerificationList> verificationList = new List<rptJewelLoanVerificationList>();
            try
            {
                // Equivalent of the LoanTrnAgg CTE
                var loanTrnAgg = CSISContext.Loan_Trn
                    .Where(trn => trn.TrnTr_Delete == false && trn.Trn_Date <= asOnDate)
                    .GroupBy(trn => trn.Loan_Id)
                    .Select(g => new
                    {
                        LoanId = g.Key,
                        TotalDisb_Amt = g.Sum(t => t.Disb_Amt),
                        TotalPrlColl_Amt = g.Sum(t => t.PrlColl_Amt)
                    });

                // Equivalent of the JloAgg CTE
                var jloAgg = CSISContext.JL_Ornments
                    .GroupBy(orn => orn.Loan_Id)
                    .Select(g => new
                    {
                        LoanId = g.Key,
                        Total_JLO_Nos = g.Sum(o => o.JLO_Nos)
                    });

                // Final query joining all data sources
                var finalQuery = from lm in CSISContext.Loan_Master
                                 join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                 join lt in loanTrnAgg on lm.Loan_Id equals lt.LoanId
                                 join ja in jloAgg on lm.Loan_Id equals ja.LoanId
                                 join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                                 where lm.Loan_Type == 2 && lm.Loan_Delete == false
                                 let loanOs = lt.TotalDisb_Amt - lt.TotalPrlColl_Amt
                                 where loanOs > 0
                                 orderby lm.Loan_No
                                 select new rptJewelLoanVerificationList
                                 {
                                     Loan_Id =  lm.Loan_Id,
                                     MemberNo =  mm.memberno,
                                     PerNo =  mm.perno,
                                     MemberName =  mm.membername,
                                     Loan_No =  lm.Loan_No,
                                     San_Date = lm.San_Date,
                                     Disb_Amt = lt.TotalDisb_Amt,
                                     Loan_OS = loanOs,
                                     GrossWeight = jd.GrossWeight,
                                     Wastage = jd.Wastage,
                                     NetWeight = jd.NetWeight,
                                     NetValue = jd.NetValue,
                                     RatePerGram = jd.RatePerGram,
                                     JLO_Name = string.Join(", ", CSISContext.JL_Ornments
                                                                     .Where(orn => orn.Loan_Id == lm.Loan_Id)
                                                                     .Select(orn => orn.JLO_Name + "-" + orn.JLO_Nos)),
                                     JLO_Nos = ja.Total_JLO_Nos
                                 };

                // To see the results, you would execute the query
                var results = await  finalQuery.ToListAsync();

                // Execute the query
                //var results = query.ToList();
                if (results != null && results.Any()) verificationList = results.ToList() ;
            }
            catch (Exception)
            {
                verificationList = new();
            }
            return verificationList;
        }
        public async Task<List<rptJewelLoanStockRegisterList>> GetJewelLoanStockRegisteList(DateTime fromDate, DateTime toDate, int loanType,string brCode)
        {
            double loanBal = 0;
            int totalNoOfBags = 0;
            double totalGrossWt = 0;
            double totalNetValue = 0;
            double loanOS = 0;
            double ratePerGram = 0;
            List<rptJewelLoanStockRegisterList> stockList = new List<rptJewelLoanStockRegisterList>();

            try
            {
                /// get ob
                List<rptJewelLoanStockRegisterList>? obList = new List<rptJewelLoanStockRegisterList>();
                #region query
                var intermediateResult = await (from loanTrn in CSISContext.Loan_Trn
                                                join jlDetails in CSISContext.JL_Details on loanTrn.Loan_Id equals jlDetails.Loan_Id
                                                join loanMaster in CSISContext.Loan_Master on jlDetails.Loan_Id equals loanMaster.Loan_Id
                                                where loanMaster.Loan_Type == loanType
                                                    && loanMaster.Loan_Delete == false
                                                    && loanTrn.Trn_Date < fromDate
                                                    && jlDetails.JL_Delete == false
                                                    && loanTrn.TrnTr_Delete == false
                                                    && loanTrn.BrCode == brCode 
                                                    && jlDetails.BrCode == brCode 
                                                    && loanMaster.BrCode == brCode 
                                                group new { loanTrn, jlDetails, loanMaster } by new
                                                {
                                                    loanMaster.Loan_Id,
                                                    loanMaster.Loan_No,
                                                    jlDetails.GrossWeight,
                                                    jlDetails.NetValue,
                                                    jlDetails.MarketRatePerGram
                                                } into grouped
                                                select new
                                                {
                                                    grouped.Key.Loan_Id,
                                                    grouped.Key.Loan_No,
                                                    Disb_Amt = grouped.Sum(x => x.loanTrn.Disb_Amt),
                                                    PrlColl_Amt = grouped.Sum(x => x.loanTrn.PrlColl_Amt),
                                                    grouped.Key.GrossWeight,
                                                    grouped.Key.MarketRatePerGram
                                                }).ToListAsync();

                var obListData = intermediateResult
                    .Select(x => new rptJewelLoanStockRegisterList
                    {
                        Loan_Id = x.Loan_Id,
                        Loan_No = x.Loan_No,
                        Disb_Amt = x.Disb_Amt,
                        GrossWeight = x.GrossWeight,
                        NetValue = x.GrossWeight * x.MarketRatePerGram,
                        PrlColl_Amt = x.PrlColl_Amt,
                        Loan_OS = x.Disb_Amt - x.PrlColl_Amt
                    })
                    .Where(x => x.Loan_OS > 0)
                    .ToList();
                if(obListData !=null && obListData.Any())
                {
                    obList = obListData.ToList();
                }
                // Finding the max sanction date that's earlier than the from date
                var maxSanDate = CSISContext.Loan_Master
                    .Where(lm => lm.San_Date < fromDate.Date)
                    .Max(lm => lm.San_Date);
                // Getting the market rate per gram for the loans with that sanction date
                ratePerGram = CSISContext.JL_Details
                    .Join(CSISContext.Loan_Master,
                        jl => jl.Loan_Id,
                        lm => lm.Loan_Id,
                        (jl, lm) => new { JLDetail = jl, LoanMaster = lm })
                    .Where(x => x.LoanMaster.San_Date == maxSanDate)
                    .Select(x => (double)x.JLDetail.MarketRatePerGram)
                    .FirstOrDefault();

                if (obList != null)
                {
                    totalNoOfBags = obList.Count();
                    totalGrossWt = obList.Sum(x => x.GrossWeight);
                    loanOS = obList.Sum(x => x.Loan_OS);
                    totalNetValue = totalGrossWt * ratePerGram;
                }

                #endregion
                
                rptJewelLoanStockRegisterList obStock = new rptJewelLoanStockRegisterList
                {
                    Loan_Id = 0,
                    Loan_No = "B/F",
                    PmtLoan_No = "",
                    Trn_Date = fromDate.Date,
                    Disb_Amt = 0,
                    Trn_SlNo = 0,
                    Rpt_GrossWeight = 0,
                    Rpt_Wastage = 0,
                    Rpt_NetWeight = 0,
                    Rpt_NetValue = 0,
                    RptLoan_No = "",
                    PrlColl_Amt = 0,
                    Pmt_GrossWeight = 0,
                    Pmt_NetWeight = 0,
                    Pmt_NetValue = 0,
                    BalNoOfBags = totalNoOfBags,
                    TotalGrossWeight = totalGrossWt,
                    TotalNetValue = totalNetValue,
                    Loan_OS = loanOS
                };
                stockList.Add(obStock);
                // LINQ query for stockCurrentList
                var stockCurrentList = await (from loanTrn in CSISContext.Loan_Trn
                                              join jlDetails in CSISContext.JL_Details on loanTrn.Loan_Id equals jlDetails.Loan_Id
                                              join loanMaster in CSISContext.Loan_Master on jlDetails.Loan_Id equals loanMaster.Loan_Id
                                              where loanMaster.Loan_Delete == false
                                                 && loanMaster.Loan_Type == loanType
                                                 && loanTrn.Trn_Date >= fromDate && loanTrn.Trn_Date <= toDate
                                                 && (loanTrn.Disb_Amt > 0 || loanTrn.PrlColl_Amt > 0)
                                                 && jlDetails.JL_Delete == false
                                                 && loanTrn.TrnTr_Delete == false
                                                 && loanTrn.BrCode == brCode && loanTrn.Voc_Status == "V"
                                                 && jlDetails.BrCode == brCode && jlDetails.Voc_Status == "V"
                                                 && loanMaster.BrCode == brCode && loanMaster.Voc_Status == "V"
                                              orderby loanTrn.Trn_Date, loanMaster.Loan_No, loanTrn.Trn_SlNo
                                              select new rptJewelLoanStockRegisterList
                                              {
                                                  Loan_Id = loanMaster.Loan_Id,
                                                  Loan_No = loanMaster.Loan_No,
                                                  Disb_Amt = (double)loanTrn.Disb_Amt,
                                                  GrossWeight = (double)jlDetails.GrossWeight,
                                                  RatePerGram = (double)jlDetails.RatePerGram,
                                                  NetValue = (double)jlDetails.NetValue,
                                                  PrlColl_Amt = (double)loanTrn.PrlColl_Amt,
                                                  Trn_Date = loanTrn.Trn_Date,
                                                  Trn_SlNo = loanTrn.Trn_SlNo,
                                                  NoOfBags = 1
                                              }).ToListAsync();
                foreach (var stock in stockCurrentList)
                {
                    // Find the maximum sanction date that's on or before the transaction date
                    maxSanDate = CSISContext.Loan_Master
                        .Where(lm => lm.San_Date <= stock.Trn_Date.Date)
                        .Max(lm => lm.San_Date);

                    // Get the market rate per gram for loans with that sanction date
                    ratePerGram = 0;
                    ratePerGram = CSISContext.JL_Details
                        .Join(CSISContext.Loan_Master,
                            jl => jl.Loan_Id,
                            lm => lm.Loan_Id,
                            (jl, lm) => new { JLDetail = jl, LoanMaster = lm })
                        .Where(x => x.LoanMaster.San_Date == maxSanDate)
                        .Select(x => (double)x.JLDetail.MarketRatePerGram)
                        .FirstOrDefault();
                    rptJewelLoanStockRegisterList stockCurrent = new rptJewelLoanStockRegisterList();

                    if (stock.Disb_Amt > 0)
                    {
                        stock.PmtLoan_No = stock.Loan_No;
                        totalNoOfBags += 1;
                        totalGrossWt += Math.Round(stock.GrossWeight, 3);
                        totalGrossWt = Math.Round(totalGrossWt, 3);
                        totalNetValue = Math.Round(totalGrossWt * ratePerGram, 2);
                        totalNetValue = Math.Round(totalNetValue, 2);
                        loanOS += stock.Disb_Amt;


                        stock.Rpt_GrossWeight = stock.GrossWeight;
                        stock.Rpt_NetValue = Math.Round(stock.GrossWeight * stock.RatePerGram, 2);
                        stock.PmtLoan_No = stock.Loan_No;
                        stock.TotalGrossWeight = totalGrossWt;
                        stock.TotalNetValue += totalNetValue;
                        stock.BalNoOfBags = totalNoOfBags;
                        stock.Loan_OS = loanOS;
                    }
                    if (stock.PrlColl_Amt > 0)
                    {
                        loanOS -= stock.PrlColl_Amt;
                        stock.Loan_OS = loanOS;
                        stock.RptLoan_No = stock.Loan_No;

                        // Calculate loan balance for specific loan and transaction number
                        loanBal = CSISContext.Loan_Trn
                            .Where(lt => lt.Loan_Id == stock.Loan_Id &&
                                        lt.Trn_SlNo <= stock.Trn_SlNo &&
                                        lt.TrnTr_Delete == false)
                            .GroupBy(lt => 1) // Group by constant to get aggregations
                            .Select(g => (double)(g.Sum(lt => lt.Disb_Amt) - g.Sum(lt => lt.PrlColl_Amt)))
                            .FirstOrDefault();
                        if (loanBal <= 0)
                        {
                            totalNoOfBags -= 1;
                            totalGrossWt -= Math.Round(stock.GrossWeight, 3);
                            totalGrossWt = Math.Round(totalGrossWt, 3);
                            totalNetValue = Math.Round(totalGrossWt * ratePerGram, 2);
                            totalNetValue = Math.Round(totalNetValue, 2);
                            loanOS -= stock.Disb_Amt;
                            stock.Pmt_GrossWeight = stock.GrossWeight;
                            stock.Pmt_NetValue = Math.Round(stock.GrossWeight * stock.RatePerGram, 2);
                        }
                        stock.TotalGrossWeight = totalGrossWt;
                        stock.TotalNetValue += totalNetValue;
                        stock.BalNoOfBags = totalNoOfBags;
                        stock.Loan_OS = loanOS;
                    }
                }
                stockList.AddRange(stockCurrentList);
            }
            catch (Exception ex)
            {
                Console.Write($"Error in fetching stock register " + ex.Message);
            }
            return stockList;
        }

        public async Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingList(DateTime asOnDate, string brCode)
        {
            DateTime fromDate;
            DateTime toDate;
            double intCalc = 0;
            double piCalc = 0;
            List<rptJewelLoanOutstandingList> jlBalanceList = new List<rptJewelLoanOutstandingList>();
            try
            {
                // Assuming you have a rptJewelLoanOutstandingList class defined
                #region discarded linq
                //jlBalanceList = await (from loanMaster in CSISContext.Loan_Master
                //                       join loanTrn in CSISContext.Loan_Trn on loanMaster.Loan_Id equals loanTrn.Loan_Id
                //                       join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                //                       join jlDetails in CSISContext.JL_Details on loanMaster.Loan_Id equals jlDetails.Loan_Id
                //                       where loanMaster.Loan_Type == 2
                //                          && loanMaster.Loan_Delete == false
                //                          && loanTrn.TrnTr_Delete == false
                //                          && loanTrn.Trn_Date <= asOnDate.Date
                //                          && loanMaster.BrCode == brCode 
                //                          && loanTrn.BrCode == brCode 
                //                          && memMaster.brcode == brCode 
                //                          && jlDetails.BrCode == brCode 
                //                       group new { loanMaster, memMaster, loanTrn, jlDetails } by new
                //                       {
                //                           loanMaster.Loan_Id,
                //                           memMaster.memberno,
                //                           memMaster.perno,
                //                           memMaster.membername,
                //                           loanMaster.Loan_No,
                //                           loanMaster.San_Date,
                //                           loanMaster.Roi,
                //                           loanMaster.Pi,
                //                           jlDetails.JL_DueDate,
                //                           jlDetails.GrossWeight,
                //                           jlDetails.NetWeight
                //                       } into grouped
                //                       let disbAmt = grouped.Sum(x => x.loanTrn.Disb_Amt)
                //                       let prlCollAmt = grouped.Sum(x => x.loanTrn.PrlColl_Amt)
                //                       let loanOS = disbAmt - prlCollAmt
                //                       let intBal = grouped.Sum(x => x.loanTrn.IntCalc_Amt) - grouped.Sum(x => x.loanTrn.IntColl_Amt)
                //                       let intCalcDate = grouped.Max(x => x.loanTrn.IntCalc_Date)
                //                       let piBal = grouped.Sum(x => x.loanTrn.PICalc_Amt) - grouped.Sum(x => x.loanTrn.PIColl_Amt)
                //                       let piCalcDate = grouped.Max(x => x.loanTrn.PICalc_Date)
                //                       where loanOS > 0
                //                       orderby grouped.Key.Loan_No
                //                       select new rptJewelLoanOutstandingList
                //                       {
                //                           Loan_Id = grouped.Key.Loan_Id,
                //                           MemberNo = grouped.Key.memberno,
                //                           PerNo = grouped.Key.perno,
                //                           MemberName = grouped.Key.membername,
                //                           Loan_No = grouped.Key.Loan_No,
                //                           San_Date = grouped.Key.San_Date,
                //                           Roi = grouped.Key.Roi,
                //                           Pi = grouped.Key.Pi,
                //                           JL_DueDate = grouped.Key.JL_DueDate,
                //                           GrossWeight = grouped.Key.GrossWeight,
                //                           NetWeight = grouped.Key.NetWeight,
                //                           Disb_Amt = disbAmt,
                //                           Loan_OS = loanOS,
                //                           Int_Bal = intBal,
                //                           IntCalc_Date = intCalcDate,
                //                           PI_Bal = piBal,
                //                           PICalc_Date = piCalcDate
                //                       }).ToListAsync();
                #endregion

                // Step 1: Execute the group and sum operations on the database server
                var intermediateResult = await (from loanMaster in CSISContext.Loan_Master
                                                join loanTrn in CSISContext.Loan_Trn on loanMaster.Loan_Id equals loanTrn.Loan_Id
                                                join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                                                join jlDetails in CSISContext.JL_Details on loanMaster.Loan_Id equals jlDetails.Loan_Id
                                                where loanMaster.Loan_Type == 2
                                                    && loanMaster.Loan_Delete == false
                                                    && loanTrn.TrnTr_Delete == false
                                                    && loanTrn.Trn_Date <= asOnDate.Date
                                                    && loanMaster.BrCode == brCode
                                                    && loanTrn.BrCode == brCode
                                                    && memMaster.brcode == brCode
                                                    && jlDetails.BrCode == brCode
                                                group new { loanMaster, memMaster, loanTrn, jlDetails } by new
                                                {
                                                    loanMaster.Loan_Id,
                                                    memMaster.memberno,
                                                    memMaster.perno,
                                                    memMaster.membername,
                                                    loanMaster.Loan_No,
                                                    loanMaster.San_Date,
                                                    loanMaster.Roi,
                                                    loanMaster.Pi,
                                                    jlDetails.JL_DueDate,
                                                    jlDetails.GrossWeight,
                                                    jlDetails.NetWeight
                                                } into grouped
                                                select new
                                                {
                                                    // Select all the key properties
                                                    grouped.Key.Loan_Id,
                                                    grouped.Key.memberno,
                                                    grouped.Key.perno,
                                                    grouped.Key.membername,
                                                    grouped.Key.Loan_No,
                                                    grouped.Key.San_Date,
                                                    grouped.Key.Roi,
                                                    grouped.Key.Pi,
                                                    grouped.Key.JL_DueDate,
                                                    grouped.Key.GrossWeight,
                                                    grouped.Key.NetWeight,
                                                    // Perform the aggregations that can be translated to SQL
                                                    Disb_Amt = grouped.Sum(x => x.loanTrn.Disb_Amt),
                                                    PrlColl_Amt = grouped.Sum(x => x.loanTrn.PrlColl_Amt),
                                                    IntCalc_Amt = grouped.Sum(x => x.loanTrn.IntCalc_Amt),
                                                    IntColl_Amt = grouped.Sum(x => x.loanTrn.IntColl_Amt),
                                                    PICalc_Amt = grouped.Sum(x => x.loanTrn.PICalc_Amt),
                                                    PIColl_Amt = grouped.Sum(x => x.loanTrn.PIColl_Amt),
                                                    IntCalc_Date = grouped.Max(x => x.loanTrn.IntCalc_Date),
                                                    PICalc_Date = grouped.Max(x => x.loanTrn.PICalc_Date)
                                                }).ToListAsync();

                // Step 2: Perform the final calculations, filtering, and projection in-memory
                var jlBalanceListData = intermediateResult
                    .Select(g => new
                    {
                        // Project into an anonymous type with calculated fields
                        LoanDetails = g,
                        LoanOS = g.Disb_Amt - g.PrlColl_Amt,
                        IntBal = g.IntCalc_Amt - g.IntColl_Amt,
                        PiBal = g.PICalc_Amt - g.PIColl_Amt
                    })
                    .Where(x => x.LoanOS > 0) // Now filter based on the calculated outstanding amount
                    .OrderBy(x => x.LoanDetails.Loan_No)
                    .Select(x => new rptJewelLoanOutstandingList
                    {
                        // Final projection into the desired concrete type
                        Loan_Id = x.LoanDetails.Loan_Id,
                        MemberNo = x.LoanDetails.memberno,
                        PerNo = x.LoanDetails.perno,
                        MemberName = x.LoanDetails.membername,
                        Loan_No = x.LoanDetails.Loan_No,
                        San_Date = x.LoanDetails.San_Date,
                        Roi = x.LoanDetails.Roi,
                        Pi = x.LoanDetails.Pi,
                        JL_DueDate = x.LoanDetails.JL_DueDate,
                        GrossWeight = x.LoanDetails.GrossWeight,
                        NetWeight = x.LoanDetails.NetWeight,
                        Disb_Amt = x.LoanDetails.Disb_Amt,
                        Loan_OS = x.LoanOS,
                        Int_Bal = x.IntBal,
                        IntCalc_Date = x.LoanDetails.IntCalc_Date,
                        PI_Bal = x.PiBal,
                        PICalc_Date = x.LoanDetails.PICalc_Date
                    })
                    .ToList();
                if (jlBalanceListData != null && jlBalanceListData.Any()) jlBalanceList = jlBalanceListData.ToList();

                foreach (var jl in jlBalanceList)
                {
                    fromDate = new DateTime(jl.San_Date.Year, jl.San_Date.Month, jl.San_Date.Day);
                    toDate = new DateTime(asOnDate.Year, asOnDate.Month, asOnDate.Day);
                    intCalc = 0; piCalc = 0;
                    if (jl.IntCalc_Date != null) fromDate = (DateTime)jl.IntCalc_Date;
                    if (jl.Loan_OS > 0)
                    {
                        intCalc = Utilities.Calculate_Interest(jl.Loan_OS, jl.Roi, Utilities.GetNoOfDays(toDate.Date, fromDate.Date));
                    }
                    if (Utilities.GetNoOfDays(toDate, jl.JL_DueDate) > 0)
                    {
                        if (jl.PICalc_Date != null)
                            fromDate = (DateTime)jl.PICalc_Date;
                        else
                            fromDate = jl.JL_DueDate;
                        piCalc = Utilities.Calculate_Interest(jl.Loan_OS, jl.Pi, Utilities.GetNoOfDays(toDate, fromDate));
                    }
                    jl.Int_Bal += intCalc;
                    jl.PI_Bal += piCalc;
                    if (intCalc > 0) jl.IntCalc_Date = toDate.Date;
                    else
                        jl.IntCalc_Date = null;
                    if (piCalc > 0) jl.PICalc_Date = toDate.Date;
                    else
                        jl.PICalc_Date = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetching jl outstanding report" + ex.Message);
            }
            return jlBalanceList;
        }

        public async Task<List<rptJewelLoanOverdueList>> GetJewelLoanOverdueList(DateTime asOnDate, string brCode)
        {
            List<rptJewelLoanOverdueList> odList = new List<rptJewelLoanOverdueList>();
            try
            {
                odList = await (from loanMaster in CSISContext.Loan_Master
                                join loanTrn in CSISContext.Loan_Trn on loanMaster.Loan_Id equals loanTrn.Loan_Id
                                join jlDetails in CSISContext.JL_Details on loanMaster.Loan_Id equals jlDetails.Loan_Id
                                join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                                where loanTrn.Trn_Date <= asOnDate.Date
                                      && loanMaster.Loan_Delete == false
                                      && loanTrn.TrnTr_Delete == false
                                      && jlDetails.JL_Delete == false
                                      && loanMaster.BrCode == brCode 
                                      && loanTrn.BrCode == brCode 
                                      && jlDetails.BrCode == brCode 
                                group new { loanMaster, loanTrn, jlDetails, memMaster } by new
                                {
                                    loanMaster.Loan_Id,
                                    loanMaster.Loan_No,
                                    loanMaster.San_Amt,
                                    loanMaster.San_Date,
                                    loanMaster.Roi,
                                    loanMaster.Pi,
                                    memMaster.memberno,
                                    memMaster.perno,
                                    memMaster.membername,
                                    jlDetails.JL_DueDate
                                } into g
                                where g.Key.JL_DueDate < asOnDate.Date
                                      && g.Key.San_Amt - g.Sum(x => x.loanTrn.PrlColl_Amt) > 0
                                select new rptJewelLoanOverdueList
                                {
                                    Loan_Id = g.Key.Loan_Id,
                                    Loan_No = g.Key.Loan_No,
                                    MemberNo = g.Key.memberno,
                                    PerNo = g.Key.perno,
                                    MemberName = g.Key.membername,
                                    San_Amt = g.Key.San_Amt,
                                    San_Date = g.Key.San_Date,
                                    Roi = g.Key.Roi,
                                    Pi = g.Key.Pi,
                                    JL_DueDate = g.Key.JL_DueDate,
                                    Prl_Bal = (float)(g.Key.San_Amt - g.Sum(x => x.loanTrn.PrlColl_Amt)),
                                    Int_Bal = (float)(g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt)),
                                    IntCalc_Date = g.Max(x => x.loanTrn.IntCalc_Date),
                                    PI_Bal = (float)(g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt)),
                                    PICalc_Date = g.Max(x => x.loanTrn.PICalc_Date)
                                }).ToListAsync();
                double intCalc = 0;
                double piCalc = 0;
                DateTime fromDate;
                DateTime toDate;
                int noOfMonths = 0;
                foreach (var jl in odList)
                {
                    intCalc = 0; piCalc = 0; noOfMonths = 0;
                    fromDate = new DateTime(jl.San_Date.Year, jl.San_Date.Month, jl.San_Date.Day);
                    toDate = new DateTime(asOnDate.Year, asOnDate.Month, asOnDate.Day);
                    if (jl.IntCalc_Date != null) fromDate = (DateTime)jl.IntCalc_Date;
                    intCalc = Utilities.Calculate_Interest(jl.Prl_Bal, jl.Roi, Utilities.GetNoOfDays(asOnDate.Date, fromDate.Date));
                    if (Utilities.GetNoOfDays(toDate, jl.JL_DueDate) > 0)
                    {
                        if (jl.PICalc_Date != null)
                            fromDate = (DateTime)jl.PICalc_Date;
                        else
                            fromDate = jl.JL_DueDate;
                        piCalc = Utilities.Calculate_Interest(jl.Prl_Bal, jl.Pi, Utilities.GetNoOfDays(asOnDate.Date, fromDate));
                    }
                    jl.IntCalc_Amt = intCalc;
                    jl.PICalc_Amt = piCalc;
                    if (intCalc > 0) jl.IntCalc_DateNow = toDate.Date;
                    else
                        jl.IntCalc_DateNow = null;
                    if (piCalc > 0) jl.PICalc_DateNow = toDate.Date;
                    else
                        jl.PICalc_DateNow = null;
                    jl.Int_Bal += intCalc;
                    jl.PI_Bal += piCalc;
                    noOfMonths = Utilities.GetMonthsBetweenDates(jl.JL_DueDate, asOnDate);
                    if (noOfMonths >= 0)
                    {
                        if (noOfMonths <= 3)
                            jl.OD_3M = (int)jl.Prl_Bal;
                        if (noOfMonths > 3 && noOfMonths <= 6)
                            jl.OD_6M = (int)jl.Prl_Bal;
                        if (noOfMonths > 6 && noOfMonths <= 9)
                            jl.OD_9M = (int)jl.Prl_Bal;
                        if (noOfMonths > 9)
                            jl.OD_12MAbove = (int)jl.Prl_Bal;
                    }
                }
                odList = odList.Where(x => x.OD_3M + x.OD_6M + x.OD_9M + x.OD_12MAbove > 0).ToList();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetcing jewel loan overdue statement" + ex.Message);
            }
            return odList;
        }

        public async Task<List<rptJewelLoanOverdueList>> GetJewelLoanAboveLimitList(DateTime asOnDate, double limitAmt, string brCode)
        {
            List<rptJewelLoanOverdueList> odList = new List<rptJewelLoanOverdueList>();
            try
            {
                #region error code on decimal does not contain defintion for contrains
                /// code from claud (error shows as 'decimal' does not contain definision for 'contains'. Hence connented
                // First, get the member IDs with outstanding loans over the limit amount
                //decimal memberIdsWithOutstandingLoans = await (CSISContext.Loan_Master
                //    .Join(CSISContext.Loan_Trn,
                //        lm => lm.Loan_Id,
                //        lt => lt.Loan_Id,
                //        (lm, lt) => new { LoanMaster = lm, LoanTrn = lt })
                //    .Where(x => x.LoanMaster.Loan_Delete == false 
                //                && x.LoanTrn.TrnTr_Delete == false 
                //                && x.LoanMaster.Loan_Type == 2)
                //    .GroupBy(x => x.LoanMaster.Mem_Id)
                //    .Where(g => g.Sum(x => x.LoanTrn.Disb_Amt) - g.Sum(x => x.LoanTrn.PrlColl_Amt) > limitAmt)
                //    .Select(g => g.Key)).FirstOrDefaultAsync();

                //// Main query for overdue loans
                //odList = await  CSISContext.Loan_Master
                //    .Join(CSISContext.Loan_Trn,
                //        lm => lm.Loan_Id,
                //        lt => lt.Loan_Id,
                //        (lm, lt) => new { LoanMaster = lm, LoanTrn = lt })
                //    .Join(CSISContext.JL_Details,
                //        x => x.LoanMaster.Loan_Id,
                //        jl => jl.Loan_Id,
                //        (x, jl) => new { x.LoanMaster, x.LoanTrn, JLDetail = jl })
                //    .Join(CSISContext.mem_master,
                //        x => x.LoanMaster.Mem_Id,
                //        mm => mm.mem_id,
                //        (x, mm) => new { x.LoanMaster, x.LoanTrn, x.JLDetail, MemMaster = mm })
                //    .Where(x => x.LoanMaster.Loan_Delete == false 
                //                && x.LoanTrn.TrnTr_Delete == false 
                //                && x.JLDetail.JL_Delete == false 
                //                && x.LoanMaster.IsAccountClosed == false 
                //                && x.LoanTrn.Trn_Date <= asOnDate.Date
                //                && memberIdsWithOutstandingLoans.Contains(x.LoanMaster.Mem_Id))
                //    .GroupBy(x => new {
                //        x.LoanMaster.Loan_Id,
                //        x.LoanMaster.Loan_No,
                //        x.MemMaster.memberno,
                //        x.MemMaster.perno,
                //        x.MemMaster.membername,
                //        x.LoanMaster.San_Amt,
                //        x.LoanMaster.San_Date,
                //        x.LoanMaster.Roi,
                //        x.LoanMaster.Pi,
                //        x.JLDetail.JL_DueDate,
                //        x.LoanMaster.Loan_Delete,
                //        x.LoanTrn.TrnTr_Delete,
                //        x.JLDetail.JL_Delete
                //    })
                //    .Select(g => new rptJewelLoanOverdueList
                //    {
                //        Loan_Id = g.Key.Loan_Id,
                //        Loan_No = g.Key.Loan_No,
                //        MemberNo = g.Key.memberno,
                //        PerNo = g.Key.perno,
                //        MemberName = g.Key.membername,
                //        San_Amt = g.Key.San_Amt,
                //        San_Date = g.Key.San_Date,
                //        Roi = g.Key.Roi,
                //        Pi = g.Key.Pi,
                //        JL_DueDate = g.Key.JL_DueDate,
                //        Prl_Bal = (float)(g.Key.San_Amt - g.Sum(x => x.LoanTrn.PrlColl_Amt)),
                //        Int_Bal = (float)(g.Sum(x => x.LoanTrn.IntCalc_Amt) - g.Sum(x => x.LoanTrn.IntColl_Amt)),
                //        IntCalc_Date = g.Max(x => x.LoanTrn.IntCalc_Date),
                //        PI_Bal = (float)(g.Sum(x => x.LoanTrn.PICalc_Amt) - g.Sum(x => x.LoanTrn.PIColl_Amt)),
                //        PICalc_Date = g.Max(x => x.LoanTrn.PICalc_Date)
                //    })
                //    .OrderBy(x => x.MemberNo)
                //    .ThenBy(x => x.San_Date)
                //    .ToListAsync();
                #endregion 

                var overdueMembers = await (from lm in CSISContext.Loan_Master
                                            join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                            where lm.Loan_Delete == false && lt.TrnTr_Delete == false && lm.Loan_Type == 2
                                            && lm.BrCode == brCode && lm.Voc_Status =="V"
                                            && lt.BrCode == brCode && lt.Voc_Status == "V"
                                            group lt by lm.Mem_Id into g
                                            where g.Sum(x => x.Disb_Amt) - g.Sum(x => x.PrlColl_Amt) > limitAmt
                                            select g.Key).ToListAsync();

                var odListData = await (from lm in CSISContext.Loan_Master
                                join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                                join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                where lm.Loan_Delete == false && lt.TrnTr_Delete == false && jd.JL_Delete == false
                                && lm.IsAccountClosed == false && lt.Trn_Date <= asOnDate.Date && overdueMembers.Contains(lm.Mem_Id)
                                && lm.BrCode == brCode && lm.Voc_Status == "V"
                                && lt.BrCode == brCode && lt.Voc_Status =="V"
                                && jd.BrCode == brCode && jd.Voc_Status == "V"
                                && mm.brcode == brCode 
                                group new { lm, lt, jd, mm } by new
                                {
                                    lm.Loan_Id,
                                    lm.Loan_No,
                                    mm.memberno,
                                    mm.perno,
                                    mm.membername,
                                    lm.San_Amt,
                                    lm.San_Date,
                                    lm.Roi,
                                    lm.Pi,
                                    jd.JL_DueDate
                                } into g
                                orderby g.Key.memberno, g.Key.San_Date
                                select new rptJewelLoanOverdueList
                                {
                                    Loan_Id = g.Key.Loan_Id,
                                    Loan_No = g.Key.Loan_No,
                                    MemberNo = g.Key.memberno,
                                    PerNo = g.Key.perno,
                                    MemberName = g.Key.membername,
                                    San_Amt = g.Key.San_Amt,
                                    San_Date = g.Key.San_Date,
                                    Roi = g.Key.Roi,
                                    Pi = g.Key.Pi,
                                    JL_DueDate = g.Key.JL_DueDate,
                                    Prl_Bal = (float)(g.Key.San_Amt - g.Sum(x => x.lt.PrlColl_Amt)),
                                    Int_Bal = (float)(g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt)),
                                    IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                    PI_Bal = (float)(g.Sum(x => x.lt.PICalc_Amt) - g.Sum(x => x.lt.PIColl_Amt)),
                                    PICalc_Date = g.Max(x => x.lt.PICalc_Date)
                                }).ToListAsync();
                if (odListData != null && odListData.Any()) odList = odListData.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetching jewel loan above the maximum limit" + ex.Message);
            }
            return odList;
        }

        ///  Issue register is for jewel loan claim, new GetJewelLoanClaim is develoved
        ///  hence it is discarded
        public async Task<List<rptJewelLoanIssueRegister>> GetJewelLoanIssueRegisterList(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptJewelLoanIssueRegister> loanList = new List<rptJewelLoanIssueRegister>();
            try
            {

                // Assuming 'context' is your DbContext instance
                loanList = await (from lm in CSISContext.Loan_Master
                                  join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                                  join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                  join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                                  where lm.San_Date >= fromDate && lm.San_Date <= toDate && lm.Loan_Delete == false
                                  && lm.BrCode == brCode
                                  && ls.BrCode == brCode 
                                  && mm.brcode == brCode 
                                  && jd.BrCode == brCode 
                                  orderby lm.Loan_No
                                  select new rptJewelLoanIssueRegister
                                  {
                                      Scheme_Id = lm.Scheme_Id,
                                      Scheme_Name = ls.Scheme_Name,
                                      Loan_Id = lm.Loan_Id,
                                      Loan_No = lm.Loan_No,
                                      MemberNo = mm.memberno,
                                      MemberName = mm.membername,
                                      San_Date = lm.San_Date,
                                      San_Amt = lm.San_Amt,
                                      GrossWeight = jd.GrossWeight,
                                      NetWeight = jd.NetWeight,
                                      NetValue = jd.NetValue
                                  }).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return loanList;
        }
        public async Task<List<rptJewelLoanClaim>> GetJewelLoanClaim(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptJewelLoanClaim> claimList = new();
            try
            {
                // Step 1: Fetch the raw, un-grouped data from the database.
                // This query joins all necessary tables and returns a flat list of loans with each ornament on a separate row.
                var flatData = await(
                    from mas in CSISContext.Loan_Master
                    join mem in CSISContext.mem_master on mas.Mem_Id equals mem.mem_id
                    join jld in CSISContext.JL_Details on mas.Loan_Id equals jld.Loan_Id
                    join ls in CSISContext.Loan_Schemes on mas.Scheme_Id equals ls.Scheme_Id
                    join orn in CSISContext.JL_Ornments on mas.Loan_Id equals orn.Loan_Id
                    where (mas.San_Date >= fromDate && mas.San_Date <= toDate)
                          && mas.Loan_Delete == false
                          && orn.JLO_Delete == false
                    select new
                    {
                        // Select all the fields needed for the final report
                        mas.Scheme_Id,
                        ls.Scheme_Name,
                        mas.Loan_Id,
                        mas.Loan_No,
                        mem.memberno,
                        mem.membername,
                        mas.San_Date,
                        mas.San_Amt,
                        jld.GrossWeight,
                        jld.NetWeight,
                        jld.NetValue,
                        // Also select the individual ornament details for later processing
                        OrnamentDetail = orn.JLO_Name + "-" + orn.JLO_Nos
                    }
                ).ToListAsync();

                // Step 2: Perform grouping and string aggregation in-memory (client-side).
                var loanList = flatData
                    .GroupBy(x => new
                    {
                        // Group by all the main loan-level fields
                        x.Scheme_Id,
                        x.Scheme_Name,
                        x.Loan_Id,
                        x.Loan_No,
                        x.memberno,
                        x.membername,
                        x.San_Date,
                        x.San_Amt,
                        x.GrossWeight,
                        x.NetWeight,
                        x.NetValue
                    })
                    .Select(g => new rptJewelLoanClaim
                    {
                        // Map the key of the group to the final report object
                        Scheme_Id = g.Key.Scheme_Id,
                        Scheme_Name = g.Key.Scheme_Name,
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        MemberNo = g.Key.memberno,
                        MemberName = g.Key.membername,
                        San_Date = g.Key.San_Date,
                        San_Amt = g.Key.San_Amt,
                        GrossWeight = g.Key.GrossWeight,
                        NetWeight = g.Key.NetWeight,
                        NetValue = g.Key.NetValue,
                        // Use string.Join to create the concatenated list of ornaments.
                        // This is the C# equivalent of the SQL STUFF FOR XML PATH trick.
                        ItemDetails = string.Join(", ", g.Select(orn => orn.OrnamentDetail))
                    })
                    .OrderBy(x => x.Loan_No)
                    .ToList();
                if (loanList != null && loanList.Any()) claimList = loanList.ToList();

            }
            catch (Exception ex)
            {
                Console.Write($"Erro in fetching jewel loan claim data " + ex.Message);
            }
            return claimList ;
        }

        public async Task<List<rptJewelLoanRedemptionList>> GetJewelLoanRedemptionList(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptJewelLoanRedemptionList> loanList = new List<rptJewelLoanRedemptionList>();
            try
            {
                var loanTransactionsInDateRange = await (CSISContext.Loan_Trn
                    .Where(lt => lt.Trn_Date >= fromDate && lt.Trn_Date <= toDate &&
                                 (lt.PIColl_Amt > 0 || lt.IntColl_Amt > 0 || lt.PrlColl_Amt > 0) &&
                                 lt.TrnTr_Delete == false && lt.BrCode == brCode )
                    .Select(lt => new
                    {
                        lt.Loan_Id,
                        lt.Trn_Date,
                        lt.PIColl_Amt,
                        lt.IntColl_Amt,
                        lt.PrlColl_Amt,
                        PIBal = 0.0,
                        IntBal = 0.0,
                        PrlOS = 0.0
                    })).ToListAsync();

                var loanBalances = await (CSISContext.Loan_Trn
                    .Where(lt => lt.TrnTr_Delete == false && lt.BrCode == brCode )
                    .GroupBy(lt => lt.Loan_Id)
                    .Where(g => g.Any(lt => lt.Trn_Date >= fromDate && lt.Trn_Date <= toDate &&
                                             (lt.PIColl_Amt > 0 || lt.IntColl_Amt > 0 || lt.PrlColl_Amt > 0))) 
                    .Select(g => new
                    {
                        Loan_Id = g.Key,
                        Trn_Date = g.Max(lt => lt.Trn_Date),
                        PIColl_Amt = 0.0,
                        IntColl_Amt = 0.0,
                        PrlColl_Amt = 0.0,
                        PIBal = g.Sum(lt => lt.PICalc_Amt - lt.PIColl_Amt),
                        IntBal = g.Sum(lt => lt.IntCalc_Amt - lt.IntColl_Amt),
                        PrlOS = CSISContext.Loan_Master.Where(lm => lm.Loan_Id == g.Key).Select(lm => lm.San_Amt).FirstOrDefault() - g.Sum(lt => lt.PrlColl_Amt)
                    })).ToListAsync();

                var db1 = loanTransactionsInDateRange.Union(loanBalances);

                var loanList1 = db1
                    .Join(CSISContext.Loan_Master, d => d.Loan_Id, lm => lm.Loan_Id, (d, lm) => new { d, lm })
                    .Join(CSISContext.mem_master, combined => combined.lm.Mem_Id, mm => mm.mem_id, (combined, mm) => new { combined.d, combined.lm, mm })
                    .Where(joined => joined.lm.Loan_Type == 2 )
                    .GroupBy(joined => new
                    {
                        joined.d.Loan_Id,
                        joined.d.Trn_Date,
                        joined.lm.Loan_No,
                        joined.mm.memberno,
                        joined.mm.membername,
                        joined.lm.San_Amt,
                        joined.lm.San_Date
                    })
                    .OrderBy(g => g.Key.Trn_Date)
                    .ThenBy(g => g.Key.Loan_No)
                    .Select(g => new rptJewelLoanRedemptionList
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        MemberNo = g.Key.memberno,
                        MemberName = g.Key.membername,
                        Trn_Date = g.Key.Trn_Date,
                        San_Date = g.Key.San_Date,
                        San_Amt = g.Key.San_Amt,
                        PIColl_Amt = g.Sum(x => x.d.PIColl_Amt),
                        IntColl_Amt = g.Sum(x => x.d.IntColl_Amt),
                        PrlColl_Amt = g.Sum(x => x.d.PrlColl_Amt),
                        PIBal = g.Sum(x => x.d.PIBal),
                        IntBal = g.Sum(x => x.d.IntBal),
                        PrlOS = g.Sum(x => x.d.PrlOS)
                    }).ToList();
                if (loanList1 != null) loanList = loanList1;
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error in fetching jewel loan redemption " + ex.Message);
            }
            return loanList;
        }

        public async Task<List<rptJewelLoanRedemption>> GetJewelLoanRedemption(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptJewelLoanRedemption> loanList = new();
            try
            {
                // --- Part 1 of UNION: Regular Collections ---
                var collections =
                    from trn in CSISContext.Loan_Trn
                    join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                    where trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate
                          && trn.TrnTr_Delete == false
                          && master.Loan_Type == 2
                          && (trn.PIColl_Amt > 0 || trn.IntColl_Amt > 0 || trn.PrlColl_Amt > 0)
                    group new { trn, master } by new
                    {
                        trn.Loan_Id,
                        master.Loan_No,
                        master.Mem_Id,
                        master.San_Date,
                        master.San_Amt,
                        master.Scheme_Id
                    } into g
                    select new rptJewelLoanRedemption 
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Mem_Id = g.Key.Mem_Id,
                        Disb_Date = g.Key.San_Date,
                        Disb_Amount = g.Key.San_Amt,
                        Scheme_Id = g.Key.Scheme_Id,
                        Trn_Date = g.Max(x => x.trn.Trn_Date),
                        PIColl_Amt = g.Sum(x => x.trn.PIColl_Amt),
                        IntColl_Amt = g.Sum(x => x.trn.IntColl_Amt),
                        PrlColl_Amt = g.Sum(x => x.trn.PrlColl_Amt),
                        FullyCollectedNo = 0,
                        FullyCollected = 0,
                        PartiallyCollectedNo = 0,
                        PartiallyCollected = 0
                    };

                // --- Subquery for Fully Cleared Loans ---
                var fullyClearedLoanIds =
                    from trn in CSISContext.Loan_Trn
                    join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                    where master.Loan_Type == 2 && trn.TrnTr_Delete == false
                    group new { trn, master } by new { trn.Loan_Id, master.San_Amt } into g
                    where g.Key.San_Amt - g.Sum(x => x.trn.PrlColl_Amt) == 0
                          && g.Sum(x => x.trn.PrlColl_Amt) > 0
                          && g.Max(x => x.trn.Trn_Date) >= fromDate && g.Max(x => x.trn.Trn_Date) <= toDate
                    select g.Key.Loan_Id;

                // --- Part 2 of UNION: Fully Cleared Loan Collections ---
                var fullyClearedCollections =
                    from trn in CSISContext.Loan_Trn
                    join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                    where fullyClearedLoanIds.Contains(trn.Loan_Id)
                          && trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate
                    group new { trn, master } by new
                    {
                        trn.Loan_Id,
                        master.Loan_No,
                        master.Mem_Id,
                        master.San_Date,
                        master.San_Amt,
                        master.Scheme_Id
                    } into g
                    where g.Sum(x => x.trn.PrlColl_Amt) > 0
                    select new rptJewelLoanRedemption 
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Mem_Id = g.Key.Mem_Id,
                        Disb_Date = g.Key.San_Date,
                        Disb_Amount = g.Key.San_Amt,
                        Scheme_Id = g.Key.Scheme_Id,
                        Trn_Date = g.Max(x => x.trn.Trn_Date),
                        PIColl_Amt = 0,
                        IntColl_Amt = 0,
                        PrlColl_Amt = 0,
                        FullyCollectedNo = 1,
                        FullyCollected = g.Sum(x => x.trn.PrlColl_Amt),
                        PartiallyCollectedNo = 0,
                        PartiallyCollected = 0
                    };

                // --- Subquery for Partially Cleared Loans ---
                var partiallyClearedLoanIds =
                    from trn in CSISContext.Loan_Trn
                    join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                    where master.Loan_Type == 2 && trn.TrnTr_Delete == false
                    group new { trn, master } by new { trn.Loan_Id, master.San_Amt } into g
                    where g.Key.San_Amt > g.Sum(x => x.trn.PrlColl_Amt)
                          && g.Sum(x => x.trn.PrlColl_Amt) > 0
                          && g.Max(x => x.trn.Trn_Date) >= fromDate && g.Max(x => x.trn.Trn_Date) <= toDate
                    select g.Key.Loan_Id;

                // --- Part 3 of UNION: Partially Cleared Loan Collections ---
                var partiallyClearedCollections =
                    from trn in CSISContext.Loan_Trn
                    join master in CSISContext.Loan_Master on trn.Loan_Id equals master.Loan_Id
                    where partiallyClearedLoanIds.Contains(trn.Loan_Id)
                          && trn.Trn_Date >= fromDate && trn.Trn_Date <= toDate
                    group new { trn, master } by new
                    {
                        trn.Loan_Id,
                        master.Loan_No,
                        master.Mem_Id,
                        master.San_Date,
                        master.San_Amt,
                        master.Scheme_Id
                    } into g
                    where g.Sum(x => x.trn.PrlColl_Amt) > 0
                    select new rptJewelLoanRedemption 
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Mem_Id = g.Key.Mem_Id,
                        Disb_Date = g.Key.San_Date,
                        Disb_Amount = g.Key.San_Amt,
                        Scheme_Id = g.Key.Scheme_Id,
                        Trn_Date = g.Max(x => x.trn.Trn_Date),
                        PIColl_Amt = 0,
                        IntColl_Amt = 0,
                        PrlColl_Amt = 0,
                        FullyCollectedNo = 0,
                        FullyCollected = 0,
                        PartiallyCollectedNo = 1,
                        PartiallyCollected = g.Sum(x => x.trn.PrlColl_Amt)
                    };

                // --- Combine all parts (the CTE) and execute final query ---
                // NOTE: This will likely fail. See alternative below.
                var db1 = collections.Concat(fullyClearedCollections).Concat(partiallyClearedCollections);

                var finalResult = await (
                    from d in db1
                    join mm in CSISContext.mem_master on d.Mem_Id equals mm.mem_id
                    join ls in CSISContext.Loan_Schemes on d.Scheme_Id equals ls.Scheme_Id
                    group new { d, mm, ls } by new
                    {
                        d.Loan_Id,
                        d.Loan_No,
                        d.Mem_Id,
                        mm.memberno,
                        mm.membername,
                        d.Disb_Date,
                        d.Disb_Amount,
                        d.Scheme_Id,
                        ls.Scheme_Name
                    } into g
                    orderby g.Key.Scheme_Name, g.Max(x => x.d.Trn_Date), g.Key.Loan_No
                    select new rptJewelLoanRedemption  // Replace with your result class
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Mem_Id = g.Key.Mem_Id,
                        MemberNo = g.Key.memberno,
                        MemberName = g.Key.membername,
                        Disb_Date = g.Key.Disb_Date, 
                        Disb_Amount = g.Key.Disb_Amount, 
                        Scheme_Id = g.Key.Scheme_Id, 
                        Scheme_Name = g.Key.Scheme_Name,
                        Trn_Date = g.Max(x => x.d.Trn_Date),
                        PrlColl_Amt = g.Sum(x => x.d.PrlColl_Amt),
                        IntColl_Amt = g.Sum(x => x.d.IntColl_Amt),
                        PIColl_Amt = g.Sum(x => x.d.PIColl_Amt),
                        FullyCollectedNo = g.Sum(x => x.d.FullyCollectedNo),
                        FullyCollected = g.Sum(x => x.d.FullyCollected),
                        PartiallyCollectedNo = g.Sum(x => x.d.PartiallyCollectedNo),
                        PartiallyCollected = g.Sum(x => x.d.PartiallyCollected)
                    }).ToListAsync();
                if (finalResult != null) loanList = finalResult.ToList();
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error in fetching jewel loan redemption " + ex.Message);
            }
            return loanList;
        }
        public async Task<List<rptJewelLoanOutstandingList>> GetJewelLoanOutstandingForMember(decimal memId, DateTime asOnDate)
        {
            DateTime fromDate;
            DateTime toDate;
            double intCalc = 0;
            double piCalc = 0;
            List<rptJewelLoanOutstandingList> loanList = new List<rptJewelLoanOutstandingList>();
            try
            {
                var osList = await (from lm in CSISContext.Loan_Master
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                    join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                                    where lm.Loan_Type == 2 && lm.Loan_Delete == false && lt.TrnTr_Delete == false 
                                    && lm.Mem_Id == memId && lt.Trn_Date <= asOnDate.Date
                                    group new { lm, lt, mm, jd } by new
                                    {
                                        lm.Loan_Id,
                                        mm.memberno,
                                        mm.perno,
                                        mm.membername,
                                        lm.Loan_No,
                                        lm.San_Date,
                                        lm.Roi,
                                        lm.Pi,
                                        jd.JL_DueDate,
                                        jd.GrossWeight,
                                        jd.NetWeight
                                    } into g
                                    let Disb_Amt = g.Sum(x => x.lt.Disb_Amt)
                                    let PrlColl_Amt = g.Sum(x => x.lt.PrlColl_Amt)
                                    where (Disb_Amt - PrlColl_Amt) > 0
                                    orderby g.Key.Loan_No
                                    select new rptJewelLoanOutstandingList
                                    {
                                        Loan_Id = g.Key.Loan_Id,
                                        Loan_No = g.Key.Loan_No,
                                        MemberNo = g.Key.memberno,
                                        PerNo = g.Key.perno,
                                        MemberName = g.Key.membername,
                                        San_Date = g.Key.San_Date,
                                        Disb_Amt = Disb_Amt,
                                        Loan_OS = Disb_Amt - PrlColl_Amt,
                                        Int_Bal = g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt),
                                        IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                        PI_Bal = g.Sum(x => x.lt.PICalc_Amt) - g.Sum(x => x.lt.PIColl_Amt),
                                        PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                                        Roi = g.Key.Roi,
                                        Pi = g.Key.Pi,
                                        JL_DueDate = g.Key.JL_DueDate,
                                        GrossWeight = g.Key.GrossWeight,
                                        NetWeight = g.Key.NetWeight
                                    }).ToListAsync();
                if (osList != null) loanList = osList.ToList();
                foreach (var jl in loanList)
                {
                    fromDate = new DateTime(jl.San_Date.Year, jl.San_Date.Month, jl.San_Date.Day);
                    toDate = new DateTime(asOnDate.Year, asOnDate.Month, asOnDate.Day);
                    intCalc = 0; piCalc = 0;
                    if (jl.IntCalc_Date != null) fromDate = (DateTime)jl.IntCalc_Date;
                    if (jl.Loan_OS > 0)
                    {
                        intCalc = Utilities.Calculate_Interest(jl.Loan_OS, jl.Roi, Utilities.GetNoOfDays(toDate.Date, fromDate.Date));
                    }
                    if (Utilities.GetNoOfDays(toDate, jl.JL_DueDate) > 0)
                    {
                        if (jl.PICalc_Date != null)
                            fromDate = (DateTime)jl.PICalc_Date;
                        else
                            fromDate = jl.JL_DueDate;
                        piCalc = Utilities.Calculate_Interest(jl.Loan_OS, jl.Pi, Utilities.GetNoOfDays(toDate, fromDate));
                    }
                    jl.Int_Bal += intCalc;
                    jl.PI_Bal += piCalc;
                    if (intCalc > 0) jl.IntCalc_Date = toDate.Date;
                    else
                        jl.IntCalc_Date = null;
                    if (piCalc > 0) jl.PICalc_Date = toDate.Date;
                    else
                        jl.PICalc_Date = null;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return loanList;
        }

        public async Task<List<rptJewelLoanOutstandingSchemeWiseList>> GetJewelLoanOutstandingSchemeWiseList(DateTime asOnDate, string brCode)
        {
            DateTime fromDate;
            DateTime toDate;
            double intCalc = 0;
            double piCalc = 0;
            List<rptJewelLoanOutstandingSchemeWiseList> jlBalanceList = new List<rptJewelLoanOutstandingSchemeWiseList>();
            try
            {
                var osList = await (from lm in CSISContext.Loan_Master
                                    join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                                    join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                                    where lm.Loan_Type == 2 && lm.Loan_Delete == false && lt.TrnTr_Delete == false 
                                    && lt.Trn_Date <= asOnDate.Date
                                    && lm.BrCode == brCode && lm.Voc_Status == "V"
                                    && ls.BrCode == brCode 
                                    && lt.BrCode == brCode && lt.Voc_Status == "V"
                                    && mm.brcode == brCode 
                                    && jd.BrCode == brCode && jd.Voc_Status == "V"
                                    group new { lm, ls, lt, mm, jd } by new
                                    {
                                        lm.Scheme_Id,
                                        ls.Scheme_Name,
                                        lm.Loan_Id,
                                        mm.memberno,
                                        mm.perno,
                                        mm.membername,
                                        lm.Loan_No,
                                        lm.San_Date,
                                        lm.Roi,
                                        lm.Pi,
                                        jd.JL_DueDate,
                                        jd.GrossWeight,
                                        jd.NetWeight
                                    } into g
                                    let Disb_Amt = g.Sum(x => x.lt.Disb_Amt)
                                    let PrlColl_Amt = g.Sum(x => x.lt.PrlColl_Amt)
                                    where (Disb_Amt - PrlColl_Amt) > 0
                                    orderby g.Key.Scheme_Id, g.Key.Loan_No
                                    select new rptJewelLoanOutstandingSchemeWiseList
                                    {
                                        Scheme_Id = g.Key.Scheme_Id,
                                        Scheme_Name = g.Key.Scheme_Name,
                                        Loan_Id = g.Key.Loan_Id,
                                        Loan_No = g.Key.Loan_No,
                                        MemberNo = g.Key.memberno,
                                        PerNo = g.Key.perno,
                                        MemberName = g.Key.membername,
                                        San_Date = g.Key.San_Date,
                                        Disb_Amt = Disb_Amt,
                                        Loan_OS = Disb_Amt - PrlColl_Amt,
                                        Int_Bal = g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt),
                                        IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                        PI_Bal = g.Sum(x => x.lt.PICalc_Amt) - g.Sum(x => x.lt.PIColl_Amt),
                                        PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                                        Roi = g.Key.Roi,
                                        Pi = g.Key.Pi,
                                        JL_DueDate = g.Key.JL_DueDate,
                                        GrossWeight = g.Key.GrossWeight,
                                        NetWeight = g.Key.NetWeight
                                    }).ToListAsync();
                if (osList != null) jlBalanceList = osList;
                foreach (var jl in jlBalanceList)
                {
                    fromDate = new DateTime(jl.San_Date.Year, jl.San_Date.Month, jl.San_Date.Day);
                    toDate = new DateTime(asOnDate.Year, asOnDate.Month, asOnDate.Day);
                    intCalc = 0; piCalc = 0;
                    if (jl.IntCalc_Date != null) fromDate = (DateTime)jl.IntCalc_Date;
                    if (jl.Loan_OS > 0)
                    {
                        intCalc = Utilities.Calculate_Interest(jl.Loan_OS, jl.Roi, Utilities.GetNoOfDays(toDate.Date, fromDate.Date));
                    }
                    if (Utilities.GetNoOfDays(toDate, jl.JL_DueDate) > 0)
                    {
                        if (jl.PICalc_Date != null)
                            fromDate = (DateTime)jl.PICalc_Date;
                        else
                            fromDate = jl.JL_DueDate;
                        piCalc = Utilities.Calculate_Interest(jl.Loan_OS, jl.Pi, Utilities.GetNoOfDays(toDate, fromDate));
                    }
                    jl.Int_Bal += intCalc;
                    jl.PI_Bal += piCalc;
                    if (intCalc > 0) jl.IntCalc_Date = toDate.Date;
                    else
                        jl.IntCalc_Date = null;
                    if (piCalc > 0) jl.PICalc_Date = toDate.Date;
                    else
                        jl.PICalc_Date = null;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return jlBalanceList;
        }

        public async Task<List<rptJewelLoanLedgerToMember>> GetJewelLoanLedgerToMembers(List<decimal> loanIdList, DateTime fromDate, DateTime toDate)
        {
            decimal loanId = 0;
            double prlOS = 0, intBal = 0, piBal = 0;
            List<rptJewelLoanLedgerToMember> loanList = new List<rptJewelLoanLedgerToMember>();
            try
            {
                string loanIdListStrng = string.Join(",", loanIdList);
                #region First part of the UNION
                var db1_part1 = await (from lm in CSISContext.Loan_Master
                                join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                where loanIdList.Contains(lm.Loan_Id) &&
                                      lm.Loan_Type == 2 
                                      && lt.Trn_Date < fromDate 
                                      && lm.Loan_Delete == false 
                                      && lt.TrnTr_Delete == false
                                group new { lm, lt } by new { lm.Mem_Id, lm.Loan_Id, lm.Loan_No, lm.San_Date, lm.San_Amt } into g
                                where (g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt)) > 0
                                select new
                                {
                                    g.Key.Mem_Id,
                                    g.Key.Loan_Id,
                                    g.Key.Loan_No,
                                    g.Key.San_Date,
                                    g.Key.San_Amt,
                                    Trn_Date = fromDate,
                                    Disb_Amt = 0.0,
                                    PICalc_Amt = 0.0,
                                    IntCalc_Amt = 0.0,
                                    PIColl_Amt = 0.0,
                                    IntColl_Amt = 0.0,
                                    PrlColl_Amt = 0.0,
                                    Prl_OS = g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt)
                                }).ToListAsync();
                #endregion

                #region  Second part of the UNION
                var db1_part2 = await (from lm in CSISContext.Loan_Master
                                join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                where loanIdList.Contains(lm.Loan_Id) 
                                      && lm.Loan_Type == 2 
                                      && lt.Trn_Date >= fromDate && lt.Trn_Date <= toDate 
                                      && lm.Loan_Delete == false 
                                      && lt.TrnTr_Delete == false
                                select new
                                {
                                    lm.Mem_Id,
                                    lm.Loan_Id,
                                    lm.Loan_No,
                                    lm.San_Date,
                                    lm.San_Amt,
                                    lt.Trn_Date,
                                    lt.Disb_Amt,
                                    lt.PICalc_Amt,
                                    lt.IntCalc_Amt,
                                    lt.PIColl_Amt,
                                    lt.IntColl_Amt,
                                    lt.PrlColl_Amt,
                                    Prl_OS = 0.0
                                }).ToListAsync();
                #endregion

                #region  Combine the two parts of the UNION
                var db1 = db1_part1.Union(db1_part2);

                // Final SELECT and JOIN with Mem_Master
                var loanListTmp =  (from d in db1
                               join mm in CSISContext.mem_master on d.Mem_Id equals mm.mem_id
                               orderby d.Loan_No, d.Trn_Date
                               select new rptJewelLoanLedgerToMember
                               {
                                   Mem_Id = d.Mem_Id,
                                   MemberNo = mm.memberno,
                                   PerNo = mm.perno, // Assuming PerNo exists in Mem_Master
                                   MemberName = mm.membername,
                                   Loan_Id = d.Loan_Id,
                                   Loan_No = d.Loan_No,
                                   San_Date = d.San_Date,
                                   San_Amt = d.San_Amt,
                                   Scheme_Name = CSISContext.Loan_Schemes
                                                     .Where(ls => ls.Scheme_Id == CSISContext.Loan_Master
                                                                                   .Where(lm => lm.Loan_Id == d.Loan_Id)
                                                                                   .Select(lm => lm.Scheme_Id)
                                                                                   .FirstOrDefault())
                                                     .Select(ls => ls.Scheme_Name)
                                                     .FirstOrDefault(),
                                   Trn_Date = d.Trn_Date,
                                   Disb_Amt = d.Disb_Amt,
                                   PICalc_Amt = d.PICalc_Amt,
                                   IntCalc_Amt = d.IntCalc_Amt,
                                   PIColl_Amt = d.PIColl_Amt,
                                   IntColl_Amt = d.IntColl_Amt,
                                   PrlColl_Amt = d.PrlColl_Amt,
                                   Prl_OS = d.Prl_OS,
                                   Int_Bal = 0, // You'll likely need to calculate this based on Loan_Trn
                                   PI_Bal = 0   // You'll likely need to calculate this based on Loan_Trn
                               }).ToList();
                #endregion

                #region  Calculate Int_Bal and PI_Bal(this might require another join or subquery)
                loanListTmp = loanListTmp.Select(ll => new rptJewelLoanLedgerToMember
                {
                    Mem_Id = ll.Mem_Id,
                    MemberNo = ll.MemberNo,
                    PerNo = ll.PerNo,
                    MemberName = ll.MemberName,
                    Loan_Id = ll.Loan_Id,
                    Loan_No = ll.Loan_No,
                    San_Date = ll.San_Date,
                    San_Amt = ll.San_Amt,
                    Scheme_Name = ll.Scheme_Name,
                    Trn_Date = ll.Trn_Date,
                    Disb_Amt = ll.Disb_Amt,
                    PICalc_Amt = ll.PICalc_Amt,
                    IntCalc_Amt = ll.IntCalc_Amt,
                    PIColl_Amt = ll.PIColl_Amt,
                    IntColl_Amt = ll.IntColl_Amt,
                    PrlColl_Amt = ll.PrlColl_Amt,
                    Prl_OS = ll.Prl_OS,
                    Int_Bal = CSISContext.Loan_Trn
                                  .Where(lt => lt.Loan_Id == ll.Loan_Id && lt.Trn_Date <= ll.Trn_Date)
                                  .Sum(lt => lt.IntCalc_Amt) -
                              CSISContext.Loan_Trn
                                  .Where(lt => lt.Loan_Id == ll.Loan_Id && lt.Trn_Date <= ll.Trn_Date)
                                  .Sum(lt => lt.IntColl_Amt),
                    PI_Bal = CSISContext.Loan_Trn
                                 .Where(lt => lt.Loan_Id == ll.Loan_Id && lt.Trn_Date <= ll.Trn_Date)
                                 .Sum(lt => lt.PICalc_Amt) -
                             CSISContext.Loan_Trn
                                 .Where(lt => lt.Loan_Id == ll.Loan_Id && lt.Trn_Date <= ll.Trn_Date)
                                 .Sum(lt => lt.PIColl_Amt)
                }).ToList();
                #endregion 

                if (loanListTmp != null) loanList = loanListTmp;

                loanList = loanList.OrderBy(x => x.Loan_Id).ThenBy(x => x.Trn_Date).ToList();
                foreach (var loan in loanList)
                {
                    if (loanId != loan.Loan_Id)
                    {
                        prlOS = 0; intBal = 0; piBal = 0;
                        prlOS = loan.Prl_OS;
                        loanId = loan.Loan_Id;
                    }
                    prlOS += loan.Disb_Amt - loan.PrlColl_Amt;
                    intBal += loan.IntCalc_Amt - loan.IntColl_Amt;
                    piBal += loan.PICalc_Amt - loan.PIColl_Amt;
                    loan.Prl_OS = prlOS;
                    loan.Int_Bal = intBal;
                    loan.PI_Bal = piBal;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return loanList;
        }

        public async Task<List<rptJewelLoanLedgerMain>> GetJewelLoanLedger(List<decimal> loanIdList)
        {
            List<rptJewelLoanLedgerMain> loanList = new List<rptJewelLoanLedgerMain>();
            string loanIdListStrng = string.Join(",", loanIdList);
            try
            {
                loanList = await (from lm in CSISContext.Loan_Master
                            join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                            join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                            where loanIdList.Contains(lm.Loan_Id)
                            select new rptJewelLoanLedgerMain
                            {
                                Loan_Id = lm.Loan_Id,
                                Mem_Id = lm.Mem_Id,
                                MemberNo = mm.memberno,
                                PerNo = mm.perno,
                                MemberName = mm.membername,
                                PhotoImage = mm.memberphoto,
                                Address = (mm.preadd1 != null && mm.preadd1.Length > 0 ? mm.preadd1 : "") +
                                          (mm.preadd2 != null && mm.preadd2.Length > 0 ? "," + mm.preadd2 : "") +
                                          (mm.preadd3 != null && mm.preadd3.Length > 0 ? "," + mm.preadd3 : "") +
                                          (mm.prepin != null && mm.prepin.Length > 0 ? "," + mm.prepin : ""),
                                MobileNo = mm.mobileno,
                                PANNo = mm.panno,
                                AadharNo = mm.aadharno,
                                SmartCardNo = mm.smartcardno,
                                Loan_No = lm.Loan_No,
                                San_Date = lm.San_Date,
                                San_Amt = lm.San_Amt,
                                Roi = lm.Roi,
                                Pi = lm.Pi,
                                JL_DueDate = jd.JL_DueDate,
                                MarketRatePerGram = jd.MarketRatePerGram,
                                RatePerGram = jd.RatePerGram,
                                GrossWeight = jd.GrossWeight,
                                Wastage = jd.Wastage,
                                NetWeight = jd.NetWeight,
                                NetValue = jd.NetWeight * jd.MarketRatePerGram,
                                JewelsImage = jd.JewelsImagePath
                            }).ToListAsync();

            }
            catch (Exception)
            {

                throw;
            }
            return loanList;
        }

        public async Task<List<rptJewelLoanOverdueSchemeWiseList>> GetJewelLoanOverdueSchemeWiseList(DateTime asOnDate, string brCode)
        {
            List<rptJewelLoanOverdueSchemeWiseList> odList = new List<rptJewelLoanOverdueSchemeWiseList>();
            try
            {
                #region linq 
                odList = await (from lm in CSISContext.Loan_Master
                              join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                              join jd in CSISContext.JL_Details on lm.Loan_Id equals jd.Loan_Id
                              join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                              join ls in CSISContext.Loan_Schemes on lm.Scheme_Id equals ls.Scheme_Id
                              where lt.Trn_Date <= asOnDate.Date &&
                                    lm.Loan_Delete == false &&
                                    lt.TrnTr_Delete == false &&
                                    jd.JL_Delete == false &&
                                    jd.JL_DueDate < asOnDate.Date &&
                                    jd.BrCode == brCode && jd.Voc_Status =="V" &&
                                    lt.BrCode == brCode && lt.Voc_Status == "V" &&
                                    jd.BrCode == brCode && jd.Voc_Status == "V" &&
                                    mm.brcode == brCode &&
                                    ls.BrCode == brCode 
                              group new { lm, lt, jd, mm, ls } by new
                              {
                                  lm.Scheme_Id,
                                  ls.Scheme_Name,
                                  lm.Loan_Id,
                                  lm.Loan_No,
                                  mm.memberno,
                                  mm.perno,
                                  mm.membername,
                                  lm.San_Amt,
                                  lm.San_Date,
                                  lm.Roi,
                                  lm.Pi,
                                  jd.JL_DueDate
                              } into g
                              let PrlColl_Amt = g.Sum(x => x.lt.PrlColl_Amt)
                              where g.Key.San_Amt - PrlColl_Amt > 0
                              select new rptJewelLoanOverdueSchemeWiseList
                              {
                                  Scheme_Id = g.Key.Scheme_Id,
                                  Scheme_Name = g.Key.Scheme_Name,
                                  Loan_Id = g.Key.Loan_Id,
                                  Loan_No = g.Key.Loan_No,
                                  MemberNo = g.Key.memberno,
                                  PerNo = g.Key.perno,
                                  MemberName = g.Key.membername,
                                  San_Amt = g.Key.San_Amt,
                                  San_Date = g.Key.San_Date,
                                  Roi = g.Key.Roi,
                                  Pi = g.Key.Pi,
                                  JL_DueDate = g.Key.JL_DueDate,
                                  Prl_Bal = (double)(g.Key.San_Amt - PrlColl_Amt),
                                  Int_Bal = (double)(g.Sum(x => x.lt.IntCalc_Amt) - g.Sum(x => x.lt.IntColl_Amt)),
                                  IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                  PI_Bal = (double)(g.Sum(x => x.lt.PICalc_Amt) - g.Sum(x => x.lt.PIColl_Amt)),
                                  PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                                  IntCalc_Amt = g.Sum(x => x.lt.IntCalc_Amt), // These were not in the original SQL, but are in the class
                                  PICalc_Amt = g.Sum(x => x.lt.PICalc_Amt),   // Adding them as they are properties in the class
                                                                              // OD_3M, OD_6M, OD_9M, OD_12MAbove would typically be calculated based on JL_DueDate and asOnDate
                                                                              // This would likely involve additional logic outside the direct translation of this SQL query.
                                  OD_3M = 0, // Placeholder
                                  OD_6M = 0, // Placeholder
                                  OD_9M = 0, // Placeholder
                                  OD_12MAbove = 0 // Placeholder
                              }).ToListAsync();
                #endregion
                // You would typically calculate OD_3M, OD_6M, OD_9M, OD_12MAbove here based on the JL_DueDate and asOnDate
                foreach (var item in odList)
                {
                    var overdueMonths = (asOnDate.Date - item.JL_DueDate.Date).Days / (365.25 / 12); // Approximate months
                    if (overdueMonths > 12)
                        item.OD_12MAbove = 1;
                    else if (overdueMonths > 9)
                        item.OD_9M = 1;
                    else if (overdueMonths > 6)
                        item.OD_6M = 1;
                    else if (overdueMonths > 3)
                        item.OD_3M = 1;
                }

                #region calculate int and pi calculations
                double intCalc = 0;
                double piCalc = 0;
                DateTime fromDate;
                DateTime toDate;
                int noOfMonths = 0;
                foreach (var jl in odList)
                {
                    intCalc = 0; piCalc = 0; noOfMonths = 0;
                    fromDate = new DateTime(jl.San_Date.Year, jl.San_Date.Month, jl.San_Date.Day);
                    toDate = new DateTime(asOnDate.Year, asOnDate.Month, asOnDate.Day);
                    if (jl.IntCalc_Date != null) fromDate = (DateTime)jl.IntCalc_Date;
                    intCalc = Utilities.Calculate_Interest(jl.Prl_Bal, jl.Roi, Utilities.GetNoOfDays(asOnDate.Date, fromDate.Date));
                    if (Utilities.GetNoOfDays(toDate, jl.JL_DueDate) > 0)
                    {
                        if (jl.PICalc_Date != null)
                            fromDate = (DateTime)jl.PICalc_Date;
                        else
                            fromDate = jl.JL_DueDate;
                        piCalc = Utilities.Calculate_Interest(jl.Prl_Bal, jl.Pi, Utilities.GetNoOfDays(asOnDate.Date, fromDate));
                    }
                    jl.IntCalc_Amt = intCalc;
                    jl.PICalc_Amt = piCalc;
                    if (intCalc > 0) jl.IntCalc_DateNow = toDate.Date;
                    else
                        jl.IntCalc_DateNow = null;
                    if (piCalc > 0) jl.PICalc_DateNow = toDate.Date;
                    else
                        jl.PICalc_DateNow = null;
                    jl.Int_Bal += intCalc;
                    jl.PI_Bal += piCalc;
                    noOfMonths = Utilities.GetMonthsBetweenDates(jl.JL_DueDate, asOnDate);
                    if (noOfMonths >= 0)
                    {
                        if (noOfMonths <= 3)
                            jl.OD_3M = (int)jl.Prl_Bal;
                        if (noOfMonths > 3 && noOfMonths <= 6)
                            jl.OD_6M = (int)jl.Prl_Bal;
                        if (noOfMonths > 6 && noOfMonths <= 9)
                            jl.OD_9M = (int)jl.Prl_Bal;
                        if (noOfMonths > 9)
                            jl.OD_12MAbove = (int)jl.Prl_Bal;
                    }
                }
                #endregion 
                odList = odList.Where(x => x.OD_3M + x.OD_6M + x.OD_9M + x.OD_12MAbove > 0).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            return odList;
        }

        public async Task<List<DropdownItem>> GetJewelLoanNosForLedger(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<DropdownItem> loanList = new();
            try
            {
                // --- First SELECT statement ---
                var query1 = await (from lm in CSISContext.Loan_Master
                             join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                             where lm.Loan_Type == 2 &&
                                   lm.Loan_Delete == false &&
                                   lt.TrnTr_Delete == false &&
                                   lm.San_Date <= fromDate
                             group new { lm, lt } by new { lm.Loan_Id, lm.Loan_No } into g
                             where g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt) > 0
                             select new DropdownItem
                             {
                                 Value = g.Key.Loan_Id.ToString(),
                                 Text = g.Key.Loan_No.ToString()
                             }).ToListAsync();

                // --- Second SELECT statement ---
                var query2 = await  (from lm in CSISContext.Loan_Master
                             join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                             where lm.Loan_Type == 2 &&
                                   lm.Loan_Delete == false &&
                                   lt.TrnTr_Delete == false &&
                                   (lm.San_Date >= fromDate && lm.San_Date <= toDate)
                             // We can select directly here since there is no HAVING clause needed for the second part in the original
                             // SQL, but for consistency with the GROUP BY, we will keep it.
                             group new { lm, lt } by new { lm.Loan_Id, lm.Loan_No } into g
                             select new DropdownItem
                             {
                                 Value = g.Key.Loan_Id.ToString(),
                                 Text = g.Key.Loan_No.ToString()
                             }).ToListAsync ();


                // --- UNION and ORDER BY ---
                var finalResult =  query1.Union(query2)
                                        .OrderBy(db1 => db1.Text)
                                        .ToList();
                if (finalResult != null && finalResult.Any()) loanList = finalResult.ToList();
            }
            catch (Exception ex)
            {
                Console.Write("Error in fetching loan nos for Jewel loan ledger " + ex.Message);
                loanList = new();
            }
            return loanList ;
        }

        public async Task<List<DropdownItem>> GetJewelLoanNosForLedger(decimal memId, DateTime fromDate, DateTime toDate, string brCode)
        {
            List<DropdownItem> loanList = new();
            try
            {
                // --- First SELECT statement ---
                var query1 = await (from lm in CSISContext.Loan_Master
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    where lm.Mem_Id == memId &&
                                          lm.Loan_Type == 2 &&
                                          lm.Loan_Delete == false &&
                                          lt.TrnTr_Delete == false &&
                                          lm.San_Date <= fromDate
                                    group new { lm, lt } by new { lm.Loan_Id, lm.Loan_No } into g
                                    where g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt) > 0
                                    select new DropdownItem
                                    {
                                        Value = g.Key.Loan_Id.ToString(),
                                        Text = g.Key.Loan_No.ToString()
                                    }).ToListAsync();

                // --- Second SELECT statement ---
                var query2 = await (from lm in CSISContext.Loan_Master
                                    join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                    where lm.Mem_Id == memId && 
                                          lm.Loan_Type == 2 &&
                                          lm.Loan_Delete == false &&
                                          lt.TrnTr_Delete == false &&
                                          (lm.San_Date >= fromDate && lm.San_Date <= toDate)
                                    // We can select directly here since there is no HAVING clause needed for the second part in the original
                                    // SQL, but for consistency with the GROUP BY, we will keep it.
                                    group new { lm, lt } by new { lm.Loan_Id, lm.Loan_No } into g
                                    select new DropdownItem
                                    {
                                        Value = g.Key.Loan_Id.ToString(),
                                        Text = g.Key.Loan_No.ToString()
                                    }).ToListAsync();


                // --- UNION and ORDER BY ---
                var finalResult = query1.Union(query2)
                                        .OrderBy(db1 => db1.Text)
                                        .ToList();
                if (finalResult != null && finalResult.Any()) loanList = finalResult.ToList();
            }
            catch (Exception ex)
            {
                Console.Write("Error in fetching loan nos for Jewel loan ledger " + ex.Message);
                loanList = new();
            }
            return loanList;
        }

        public List<rptJewelLoanLedgerOrnmentsSub> GetJewelLoanLedgerOrnmentsSub(decimal loanId)
        {
            List<rptJewelLoanLedgerOrnmentsSub> loanList = new();
            try
            {
                var result =  (from orn in CSISContext.JL_Ornments
                               where orn.Loan_Id == loanId
                                    select new rptJewelLoanLedgerOrnmentsSub
                                    {
                                        Loan_Id = orn.Loan_Id,
                                        JLO_Nos = orn.JLO_Nos,
                                        JLO_Name = orn.JLO_Name,

                                    }).ToList();
                if (result != null && result.Any()) loanList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.Write("Error in fetching Jewel loan ornments list for sub report " + ex.Message);
            }
            return loanList;
        }

        public List<rptJewelLoanLedgerTrnSub> GetJewelLoanLedgerTrnSub(decimal loanId, DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptJewelLoanLedgerTrnSub> loanList = new();
            try
            {
                // Part 1: Corresponds to the first SELECT statement in the UNION
                //var part1Query =   (from lm in CSISContext.Loan_Master
                //                 join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                //                 where lm.Loan_Id == loanId && lm.Loan_Type == 2 && lt.Trn_Date < fromDate && lm.Loan_Delete == false && lt.TrnTr_Delete == false
                //                 group new { lm, lt } by new { lm.Loan_Id, lm.San_Date, lm.San_Amt } into g
                //                 let Prl_OS = g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt)
                //                 where Prl_OS > 0 
                //                 select new rptJewelLoanLedgerTrnSub
                //                 {
                //                     Loan_Id = g.Key.Loan_Id,
                //                     San_Date = g.Key.San_Date,
                //                     San_Amt = g.Key.San_Amt,
                //                     Trn_Date = g.Max(x => x.lt.Trn_Date),
                //                     Disb_Amt = 0,
                //                     PICalc_Amt = 0,
                //                     IntCalc_Amt = 0,
                //                     PIColl_Amt = 0,
                //                     IntColl_Amt = 0,
                //                     PrlColl_Amt = 0,
                //                     Prl_OS = Prl_OS,
                //                     PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                //                     IntCalc_Date = g.Max(x => x.lt.IntCalc_Date)
                //                 }).ToList();

                var part1Query = (from lm in CSISContext.Loan_Master
                                  join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                  where lm.Loan_Id == loanId && lm.Loan_Type == 2 && lt.Trn_Date < fromDate
                                        && lm.Loan_Delete == false && lt.TrnTr_Delete == false
                                  group new { lm, lt } by new { lm.Loan_Id, lm.San_Date, lm.San_Amt } into g
                                  where g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt) > 0
                                  select new rptJewelLoanLedgerTrnSub
                                  {
                                      Loan_Id = g.Key.Loan_Id,
                                      San_Date = g.Key.San_Date,
                                      San_Amt = g.Key.San_Amt,
                                      Trn_Date = g.Max(x => x.lt.Trn_Date),
                                      Disb_Amt = 0,
                                      PICalc_Amt = 0,
                                      IntCalc_Amt = 0,
                                      PIColl_Amt = 0,
                                      IntColl_Amt = 0,
                                      PrlColl_Amt = 0,
                                      Prl_OS = g.Sum(x => x.lt.Disb_Amt) - g.Sum(x => x.lt.PrlColl_Amt),
                                      PICalc_Date = g.Max(x => x.lt.PICalc_Date),
                                      IntCalc_Date = g.Max(x => x.lt.IntCalc_Date)
                                  })
                  .ToList();

                // Part 2: Corresponds to the second SELECT statement in the UNION
                var part2Query =  (from lm in CSISContext.Loan_Master
                                 join lt in CSISContext.Loan_Trn on lm.Loan_Id equals lt.Loan_Id
                                 where lm.Loan_Id == loanId && lm.Loan_Type == 2 && lt.Trn_Date >= fromDate && lt.Trn_Date <= toDate && lm.Loan_Delete == false && lt.TrnTr_Delete == false
                                 select new rptJewelLoanLedgerTrnSub
                                 {
                                     Loan_Id = lm.Loan_Id,
                                     San_Date = lm.San_Date,
                                     San_Amt = lm.San_Amt,
                                     Trn_Date = lt.Trn_Date,
                                     Disb_Amt = lt.Disb_Amt,
                                     PICalc_Amt = lt.PICalc_Amt,
                                     IntCalc_Amt = lt.IntCalc_Amt,
                                     PIColl_Amt = lt.PIColl_Amt,
                                     IntColl_Amt = lt.IntColl_Amt,
                                     PrlColl_Amt = lt.PrlColl_Amt,
                                     Prl_OS = 0,
                                     PICalc_Date = lt.PICalc_Date,
                                     IntCalc_Date = lt.IntCalc_Date
                                 }).ToList ();

                // Combine and aggregate the results
                var combinedResults = part1Query.ToList().Union(part2Query.ToList());

                // Final aggregation and ordering to match the outer SELECT statement
                var loanListData = (from item in combinedResults
                                group item by new { item.Loan_Id, item.San_Date, item.San_Amt, item.Trn_Date, item.Prl_OS } into g
                                select new rptJewelLoanLedgerTrnSub
                                {
                                    Loan_Id = g.Key.Loan_Id,
                                    San_Date = g.Key.San_Date,
                                    San_Amt = g.Key.San_Amt,
                                    Trn_Date = g.Key.Trn_Date,
                                    Disb_Amt = g.Sum(x => x.Disb_Amt),
                                    PICalc_Amt = g.Sum(x => x.PICalc_Amt),
                                    PICalc_Date = g.Max(x => x.PICalc_Date),
                                    IntCalc_Amt = g.Sum(x => x.IntCalc_Amt),
                                    IntCalc_Date = g.Max(x => x.IntCalc_Date),
                                    PIColl_Amt = g.Sum(x => x.PIColl_Amt),
                                    IntColl_Amt = g.Sum(x => x.IntColl_Amt),
                                    PrlColl_Amt = g.Sum(x => x.PrlColl_Amt),
                                    Prl_OS = g.Key.Prl_OS
                                })
                                .OrderBy(x => x.Loan_Id)
                                .ThenBy(x => x.Trn_Date)
                                .ToList();
                if (loanListData != null && loanListData.Any()) loanList = loanListData.ToList();
            }
            catch (Exception ex)
            {
                Console.Write("Error in fetching loan balance for jewel loan ledger sub report " + ex.Message);
                loanList = new();
            }
            return loanList;
        }
    }
}
