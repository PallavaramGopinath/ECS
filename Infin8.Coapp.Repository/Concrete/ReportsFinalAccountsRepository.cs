using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.VisualBasic;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;

namespace Infin8.Coapp.Repository
{
    public class ReportsFinalAccountsRepository : Repository<Reports_Master>, IReportsFinalAccountsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsFinalAccountsRepository(CSISContext context) : base(context)
        {
        }

        // Define this class outside your method to ensure type compatibility between the queries
        public class LoanQueryResult
        {
            public decimal Loan_Id { get; set; }
            public string? Loan_No { get; set; }
            public decimal Mem_Id { get; set; }
            public double Roi { get; set; }
            public DateTime San_Date { get; set; }
            public string? MemberNo { get; set; }
            public string? PerNo { get; set; }
            public string? MemberName { get; set; }
            public string? Token_PersonNo { get; set; }
            public int Loan_Type { get; set; }
            public int Scheme_Id { get; set; }
            public string? Scheme_Name { get; set; }
            public double Prl_OS { get; set; }
            public double Prl_OB { get; set; }
            public double Int_OB { get; set; }
            public double PI_OB { get; set; }
            public double IOD_OB { get; set; }
            public double Prl_Sched { get; set; }
            public double Disb_Amt { get; set; }
            public double PrlPayment { get; set; }
            public double PrlColl_Amt { get; set; }
            public double IntCalc_Amt { get; set; }
            public double IntColl_Amt { get; set; }
            public double PICalc_Amt { get; set; }
            public double PIColl_Amt { get; set; }
            public double IODCalc_Amt { get; set; }
            public double IODColl_Amt { get; set; }
            public double Prl_OD { get; set; }
            public double Int_Bal { get; set; }
            public double PI_Bal { get; set; }
            public double IOD_Bal { get; set; }
        }

        public class TempDb1
        {
            public decimal Led_Id { get; set; }
            public double OB { get; set; }
            public double Voc_Rpt { get; set; }
            public double Voc_Pmt { get; set; }
        }
        public async Task<List<rptMemberTrn>> GetRptMemberTrnSchedule(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            List<rptMemberTrn> memTrnList = new();
            //string sql = "";
            try
            {
                #region query
                //switch (trnType)
                //{
                //    case 1: /// member due to
                //    case 5: /// staff due to
                //        sql = @"SELECT Mem_Trn.Mem_Id, 
                //        Mem_Master.memberNo, 
                //        Mem_Master.PerNo, 
                //        Mem_Master.memberName,    
                //        Mem_Trn.Trn_Type, 
                //        Mem_Trn.Led_Id, 
                //        CAST(Sum(Mem_Trn.Pmt_Amt) - Sum(Mem_Trn.Rpt_Amt) AS float) AS Amt_OB, 
                //        CAST(0 as float)  AS Rpt_Amt, 
                //        CAST(0 as float)  AS Pmt_Amt,
                //        Fin_Ledger.Led_Name 
                //        FROM Mem_Trn INNER  JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id 
                //        INNER JOIN Fin_Ledger ON Mem_Trn.Led_Id = Fin_Ledger.Led_Id 
                //        WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date < @fromDate And Mem_Trn.Trn_Type = @trnType 
                //        GROUP BY Mem_Trn.Mem_Id, Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName, Mem_Trn.Trn_Type,Mem_Trn.Led_Id,Fin_Ledger.Led_Name
                //        HAVING Sum(Mem_Trn.Pmt_Amt) - Sum(Mem_Trn.Rpt_Amt) >0";

                //        break;
                //    case 2: /// member due by
                //    case 3: /// share capital
                //    case 6: /// staff due by
                //        sql = @"SELECT Mem_Trn.Mem_Id, 
                //        Mem_Master.memberNo, 
                //        Mem_Master.PerNo, 
                //        Mem_Master.memberName,    
                //        Mem_Trn.Trn_Type, 
                //        Mem_Trn.Led_Id, 
                //        CAST(Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) AS float) AS Amt_OB,
                //        CAST(0 as float)  AS Rpt_Amt,
                //        CAST(0 as float)  AS Pmt_Amt,
                //        Fin_Ledger.Led_Name
                //        FROM Mem_Trn INNER  JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id
                //        INNER JOIN Fin_Ledger ON Mem_Trn.Led_Id = Fin_Ledger.Led_Id
                //        WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date < @fromDate And Mem_Trn.Trn_Type = @trnType
                //        GROUP BY Mem_Trn.Mem_Id, Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName, Mem_Trn.Trn_Type,Mem_Trn.Led_Id,Fin_Ledger.Led_Name 
                //        HAVING Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) >0";
                //        break;

                //}
                //sql += @" UNION
                //        SELECT Mem_Trn.Mem_Id, 
                //        Mem_Master.memberNo, 
                //        Mem_Master.PerNo, 
                //        Mem_Master.memberName,  
                //        Mem_Trn.Trn_Type, 
                //        Mem_Trn.Led_Id, 
                //        CAST(0 as float) AS Amt_OB,
                //        Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, 
                //        Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt,
                //        Fin_Ledger.Led_Name 
                //        FROM Mem_Trn INNER  JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id 
                //        INNER JOIN Fin_Ledger ON Mem_Trn.Led_Id = Fin_Ledger.Led_Id 
                //        WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date BETWEEN @fromDate AND @toDate And Mem_Trn.Trn_Type = @trnType
                //        GROUP BY Mem_Trn.Mem_Id, Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName, Mem_Trn.Trn_Type,Mem_Trn.Led_Id,Fin_Ledger.Led_Name 
                //        HAVING  Sum(Mem_Trn.Rpt_Amt) >0 OR Sum(Mem_Trn.Pmt_Amt) >0
                //        ORDER BY Mem_Trn.Trn_Type, Mem_Trn.Mem_Id,Mem_Trn.Led_Id";
                //memTrnList = await CSISContext.Database.SqlQueryRaw<rptMemberTrn>(sql
                //    , new NpgsqlParameter("@trnType", trnType)
                //    , new NpgsqlParameter("@fromDate", fromDate.Date)
                //    , new NpgsqlParameter("@toDate", toDate.Date)).ToListAsync();
                #endregion

                #region linq old
                //// First part: Get opening balances based on transaction type
                //var openingBalancesQuery = trnType switch
                //{
                //    1 or 5 => // member due to or staff due to
                //        from memTrn in CSISContext.Mem_Trn
                //        join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                //        join finLedger in CSISContext.Fin_Ledger on memTrn.Led_Id equals finLedger.Led_Id
                //        where memTrn.MemTrn_Delete == false && memTrn.Trn_Date < fromDate && memTrn.Trn_Type == trnType &&
                //        memTrn.BrCode == brCode && memTrn.Voc_Status == "V" &&
                //        memMaster.brcode == brCode &&
                //        finLedger.BrCode == brCode 
                //        group new { memTrn, memMaster, finLedger } by new
                //        {
                //            memTrn.Mem_Id,
                //            memMaster.memberno,
                //            memMaster.perno,
                //            memMaster.membername,
                //            memTrn.Trn_Type,
                //            memTrn.Led_Id,
                //            finLedger.Led_Name
                //        } into g
                //        let difference = g.Sum(x => x.memTrn.Pmt_Amt) - g.Sum(x => x.memTrn.Rpt_Amt)
                //        where difference > 0
                //        select new rptMemberTrn
                //        {
                //            Mem_Id = g.Key.Mem_Id,
                //            MemberNo = g.Key.memberno,
                //            PerNo = g.Key.perno,
                //            MemberName = g.Key.membername,
                //            Trn_Type = g.Key.Trn_Type,
                //            Led_Id = g.Key.Led_Id,
                //            Amt_OB = (float)(g.Sum(x => x.memTrn.Pmt_Amt) - g.Sum(x => x.memTrn.Rpt_Amt)),
                //            Rpt_Amt = 0f,
                //            Pmt_Amt = 0f,
                //            Led_Name = g.Key.Led_Name
                //        },

                //    2 or 3 or 6 => // member due by, share capital, or staff due by
                //        from memTrn in CSISContext.Mem_Trn
                //        join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                //        join finLedger in CSISContext.Fin_Ledger on memTrn.Led_Id equals finLedger.Led_Id
                //        where memTrn.MemTrn_Delete == false 
                //        && memTrn.Voc_Status == "V"
                //        && memTrn.Trn_Date < fromDate && memTrn.Trn_Type == trnType &&
                //        memTrn.BrCode == brCode &&
                //        memMaster.brcode == brCode &&
                //        finLedger.BrCode == brCode
                //        group new { memTrn, memMaster, finLedger } by new
                //        {
                //            memTrn.Mem_Id,
                //            memMaster.memberno,
                //            memMaster.perno,
                //            memMaster.membername,
                //            memTrn.Trn_Type,
                //            memTrn.Led_Id,
                //            finLedger.Led_Name
                //        } into g
                //        let difference = g.Sum(x => x.memTrn.Rpt_Amt) - g.Sum(x => x.memTrn.Pmt_Amt)
                //        where difference > 0
                //        select new rptMemberTrn
                //        {
                //            Mem_Id = g.Key.Mem_Id,
                //            MemberNo = g.Key.memberno,
                //            PerNo = g.Key.perno,
                //            MemberName = g.Key.membername,
                //            Trn_Type = g.Key.Trn_Type,
                //            Led_Id = g.Key.Led_Id,
                //            Amt_OB = (float)(g.Sum(x => x.memTrn.Rpt_Amt) - g.Sum(x => x.memTrn.Pmt_Amt)),
                //            Rpt_Amt = 0f,
                //            Pmt_Amt = 0f,
                //            Led_Name = g.Key.Led_Name
                //        },

                //    _ => Enumerable.Empty<rptMemberTrn>()
                //};

                //// Second part: Get transactions between from and to dates
                //var transactionsQuery =
                //    from memTrn in CSISContext.Mem_Trn
                //    join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                //    join finLedger in CSISContext.Fin_Ledger on memTrn.Led_Id equals finLedger.Led_Id
                //    where memTrn.MemTrn_Delete == false &&
                //          memTrn.Trn_Date >= fromDate &&
                //          memTrn.Trn_Date <= toDate &&
                //          memTrn.Trn_Type == trnType && memTrn.Voc_Status == "V" &&
                //          memTrn.BrCode == brCode &&
                //          memMaster.brcode == brCode &&
                //          finLedger.BrCode == brCode
                //    group new { memTrn, memMaster, finLedger } by new
                //    {
                //        memTrn.Mem_Id,
                //        memMaster.memberno,
                //        memMaster.perno,
                //        memMaster.membername,
                //        memTrn.Trn_Type,
                //        memTrn.Led_Id,
                //        finLedger.Led_Name
                //    } into g
                //    where g.Sum(x => x.memTrn.Rpt_Amt) > 0 || g.Sum(x => x.memTrn.Pmt_Amt) > 0
                //    select new rptMemberTrn
                //    {
                //        Mem_Id = g.Key.Mem_Id,
                //        MemberNo = g.Key.memberno,
                //        PerNo = g.Key.perno,
                //        MemberName = g.Key.membername,
                //        Trn_Type = g.Key.Trn_Type,
                //        Led_Id = g.Key.Led_Id,
                //        Amt_OB = 0f,
                //        Rpt_Amt = (float)g.Sum(x => x.memTrn.Rpt_Amt),
                //        Pmt_Amt = (float)g.Sum(x => x.memTrn.Pmt_Amt),
                //        Led_Name = g.Key.Led_Name
                //    };

                //// Union the queries and order the results
                //var combinedQuery = openingBalancesQuery.Union(transactionsQuery)
                //    .OrderBy(r => r.Trn_Type)
                //    .ThenBy(r => r.Mem_Id)
                //    .ThenBy(r => r.Led_Id);

                //// Execute the query and get results
                //// Since we're working with Entity Framework Core, we need to add the right async method
                //memTrnList = await combinedQuery.AsQueryable().ToListAsync();

                //// Alternatively, if ToListAsync() is still not available, use one of these approaches:
                //// Option 1: If using Entity Framework Core, make sure to add the Microsoft.EntityFrameworkCore namespace
                //// using Microsoft.EntityFrameworkCore;
                //// memTrnList = await EntityFrameworkQueryableExtensions.ToListAsync(combinedQuery);

                //// Option 2: If ToListAsync is not available at all, use synchronous version
                //// memTrnList = combinedQuery.ToList();

                //memTrnList = (from mem in memTrnList
                //              group mem by new
                //              {
                //                  mem.Mem_Id,
                //                  mem.MemberNo,
                //                  mem.PerNo,
                //                  mem.MemberName,
                //                  mem.Trn_Type,
                //                  mem.Led_Id,
                //                  mem.Led_Name
                //              } into g
                //              select new rptMemberTrn
                //              {
                //                  Mem_Id = g.Key.Mem_Id,
                //                  MemberNo = g.Key.MemberNo,
                //                  PerNo = g.Key.PerNo,
                //                  MemberName = g.Key.MemberName,
                //                  Trn_Type = g.Key.Trn_Type,
                //                  Led_Id = g.Key.Led_Id,
                //                  Led_Name = g.Key.Led_Name,
                //                  Amt_OB = g.Sum(trn => trn.Amt_OB),
                //                  Rpt_Amt = g.Sum(trn => trn.Rpt_Amt),
                //                  Pmt_Amt = g.Sum(trn => trn.Pmt_Amt)
                //              }).ToList();
                #endregion

                #region linq new
                IQueryable<rptMemberTrn> openingBalancesQuery = null;

                if (trnType == 1 || trnType == 5) // member due to or staff due to
                {
                    openingBalancesQuery = from memTrn in CSISContext.Mem_Trn
                                           join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                                           join finLedger in CSISContext.Fin_Ledger on memTrn.Led_Id equals finLedger.Led_Id
                                           where memTrn.MemTrn_Delete == false && memTrn.Trn_Date < fromDate && memTrn.Trn_Type == trnType &&
                                           memTrn.BrCode == brCode && 
                                           memMaster.brcode == brCode &&
                                           finLedger.BrCode == brCode
                                           group new { memTrn, memMaster, finLedger } by new
                                           {
                                               memTrn.Mem_Id,
                                               memMaster.memberno,
                                               memMaster.perno,
                                               memMaster.membername,
                                               memTrn.Trn_Type,
                                               memTrn.Led_Id,
                                               finLedger.Led_Name
                                           } into g
                                           where g.Sum(x => x.memTrn.Pmt_Amt) - g.Sum(x => x.memTrn.Rpt_Amt) > 0
                                           select new rptMemberTrn
                                           {
                                               Mem_Id = g.Key.Mem_Id,
                                               MemberNo = g.Key.memberno,
                                               PerNo = g.Key.perno,
                                               MemberName = g.Key.membername,
                                               Trn_Type = g.Key.Trn_Type,
                                               Led_Id = g.Key.Led_Id,
                                               Amt_OB = (float)(g.Sum(x => x.memTrn.Pmt_Amt) - g.Sum(x => x.memTrn.Rpt_Amt)),
                                               Rpt_Amt = 0f,
                                               Pmt_Amt = 0f,
                                               Led_Name = g.Key.Led_Name
                                           };
                }
                else if (trnType == 2 || trnType == 3 || trnType == 6) // member due by, share capital, or staff due by
                {
                    openingBalancesQuery = from memTrn in CSISContext.Mem_Trn
                                           join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                                           join finLedger in CSISContext.Fin_Ledger on memTrn.Led_Id equals finLedger.Led_Id
                                           where memTrn.MemTrn_Delete == false && memTrn.Voc_Status == "V" &&
                                           memTrn.Trn_Date < fromDate && memTrn.Trn_Type == trnType &&
                                           memTrn.BrCode == brCode &&
                                           memMaster.brcode == brCode &&
                                           finLedger.BrCode == brCode
                                           group new { memTrn, memMaster, finLedger } by new
                                           {
                                               memTrn.Mem_Id,
                                               memMaster.memberno,
                                               memMaster.perno,
                                               memMaster.membername,
                                               memTrn.Trn_Type,
                                               memTrn.Led_Id,
                                               finLedger.Led_Name
                                           } into g
                                           where g.Sum(x => x.memTrn.Rpt_Amt) - g.Sum(x => x.memTrn.Pmt_Amt) > 0
                                           select new rptMemberTrn
                                           {
                                               Mem_Id = g.Key.Mem_Id,
                                               MemberNo = g.Key.memberno,
                                               PerNo = g.Key.perno,
                                               MemberName = g.Key.membername,
                                               Trn_Type = g.Key.Trn_Type,
                                               Led_Id = g.Key.Led_Id,
                                               Amt_OB = (float)(g.Sum(x => x.memTrn.Rpt_Amt) - g.Sum(x => x.memTrn.Pmt_Amt)),
                                               Rpt_Amt = 0f,
                                               Pmt_Amt = 0f,
                                               Led_Name = g.Key.Led_Name
                                           };
                }
                else
                {
                    // Create an empty EF queryable instead of Enumerable.Empty
                    openingBalancesQuery = CSISContext.Mem_Trn.Where(x => false).Select(x => new rptMemberTrn
                    {
                        Mem_Id = 0,
                        MemberNo = "",
                        PerNo = "",
                        MemberName = "",
                        Trn_Type = 0,
                        Led_Id = 0,
                        Amt_OB = 0f,
                        Rpt_Amt = 0f,
                        Pmt_Amt = 0f,
                        Led_Name = ""
                    });
                }

                // Your existing transactionsQuery...
                var transactionsQuery =
                    from memTrn in CSISContext.Mem_Trn
                    join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                    join finLedger in CSISContext.Fin_Ledger on memTrn.Led_Id equals finLedger.Led_Id
                    where memTrn.MemTrn_Delete == false &&
                          memTrn.Trn_Date >= fromDate &&
                          memTrn.Trn_Date <= toDate &&
                          memTrn.Trn_Type == trnType && 
                          memTrn.BrCode == brCode &&
                          memMaster.brcode == brCode &&
                          finLedger.BrCode == brCode
                    group new { memTrn, memMaster, finLedger } by new
                    {
                        memTrn.Mem_Id,
                        memMaster.memberno,
                        memMaster.perno,
                        memMaster.membername,
                        memTrn.Trn_Type,
                        memTrn.Led_Id,
                        finLedger.Led_Name
                    } into g
                    where g.Sum(x => x.memTrn.Rpt_Amt) > 0 || g.Sum(x => x.memTrn.Pmt_Amt) > 0
                    select new rptMemberTrn
                    {
                        Mem_Id = g.Key.Mem_Id,
                        MemberNo = g.Key.memberno,
                        PerNo = g.Key.perno,
                        MemberName = g.Key.membername,
                        Trn_Type = g.Key.Trn_Type,
                        Led_Id = g.Key.Led_Id,
                        Amt_OB = 0f,
                        Rpt_Amt = (float)g.Sum(x => x.memTrn.Rpt_Amt),
                        Pmt_Amt = (float)g.Sum(x => x.memTrn.Pmt_Amt),
                        Led_Name = g.Key.Led_Name
                    };

                // Union the queries and order the results
                var combinedQuery = openingBalancesQuery.Union(transactionsQuery)
                    .OrderBy(r => r.Trn_Type)
                    .ThenBy(r => r.Mem_Id)
                    .ThenBy(r => r.Led_Id);

                // Now this should work with async
                memTrnList = await combinedQuery.ToListAsync();
                #endregion 

                foreach (var trn in memTrnList)
                {
                    switch (trn.Trn_Type)
                    {
                        case 1: /// member due to
                        case 5: /// staff due to
                            trn.Amt_CB += trn.Amt_OB + Convert.ToDouble(trn.Pmt_Amt) - Convert.ToDouble(trn.Rpt_Amt);
                            break;
                        case 2: /// member due by
                        case 3: /// share capital
                        case 6: /// staff due by
                            trn.Amt_CB += trn.Amt_OB + Convert.ToDouble(trn.Rpt_Amt) - Convert.ToDouble(trn.Pmt_Amt);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fecthing member transaction data " + ex.Message);
                memTrnList = new();
            }
            return memTrnList;
        }

        public async Task<List<rptFALoanOutstanding>> GetLoanOutstandingForFA_HL(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            List<rptFALoanOutstanding> loanList = new List<rptFALoanOutstanding>();
            try
            {
                #region query
                //loanList = await CSISContext.Database.SqlQueryRaw<rptFALoanOutstanding>(
                //        @"With Dt1
                //        AS
                //        (
                //            SELECT Loan_Trn.Loan_id, 
                //            Loan_Master.Loan_No,
                //            Loan_Master.Mem_Id,
                //            Loan_Master.roi,
                //            Loan_Master.San_date,
                //            Mem_Master.memberNo,
                //            Mem_Master.PerNo,
                //            Mem_Master.memberName,
                //            Mem_Master.Token_PersonNo,
                //            Loan_Master.Loan_Type,
                //            Loan_Master.Scheme_Id,
                //            Loan_Schemes.Scheme_Name,
                //            Sum(Loan_Trn.Disb_Amt)  - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OS,
                //            Sum(Loan_Trn.Disb_Amt)  - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OB,
                //            Sum(Loan_Trn.IntCalc_Amt)  - Sum(Loan_Trn.IntColl_Amt) AS Int_OB,
                //            Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) AS PI_OB,
                //            ISNULL(Sum(Loan_Trn.IODCalc_Amt) - Sum(Loan_Trn.IODColl_Amt),0) AS IOD_OB,
                //            CAST(0 AS float) AS Prl_Sched,
                //            Sum(Loan_Trn.Disb_Amt) AS Disb_Amt,
                //            CAST(0 AS float) AS PrlPayment, 
                //            CAST(0 AS float) AS PrlColl_Amt,
                //            CAST(0 AS float) AS IntCalc_Amt,
                //            CAST(0 AS float) AS IntColl_Amt,
                //            CAST(0 AS float) AS PICalc_Amt,
                //            CAST(0 AS float) AS PIColl_Amt,
                //            CAST(0 AS float) AS IODCalc_Amt,
                //            CAST(0 AS float) AS IODColl_Amt,
                //            Sum(Loan_Trn.Prl_Sched) - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OD,
                //            CAST(0 AS float) AS Int_Bal,
                //            CAST(0 AS float) AS PI_Bal,
                //            CAST(0 AS float) AS IOD_Bal
                //            From Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id 
                //            INNER JOIN Mem_Master ON Loan_Master.Mem_Id = Mem_Master.mem_Id 
                //            INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id 
                //            WHERE Loan_Trn.TrnTr_Delete = 0 AND CAST(Loan_Trn.Trn_Date as date) <@fromDate
                //            GROUP BY Loan_Trn.Loan_id,Loan_Master.Loan_Type,Loan_Master.Loan_No,Loan_Master.Mem_Id,Loan_Master.roi,Loan_Master.San_date,Mem_Master.memberNo,Mem_Master.PerNo,Mem_Master.memberName,Mem_Master.Token_PersonNo,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name  
                //            HAVING Loan_Master.Loan_Type = @loanType AND (Sum(Loan_Trn.Disb_Amt) - Sum(Loan_Trn.PrlColl_Amt) >0 
                //            OR Sum(Loan_Trn.IntCalc_Amt) - Sum(Loan_Trn.IntColl_Amt) >0 
                //            OR Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) >0 
                //            OR Sum(Loan_Trn.IODCalc_Amt) - Sum(Loan_Trn.IODColl_Amt) >0)
                //        UNION
                //            SELECT Loan_Trn.Loan_id, 
                //            Loan_Master.Loan_No,
                //            Loan_Master.Mem_Id,
                //            Loan_Master.roi,
                //            Loan_Master.San_date,
                //            Mem_Master.memberNo,
                //            Mem_Master.PerNo,
                //            Mem_Master.memberName,
                //            Mem_Master.Token_PersonNo,
                //            Loan_Master.Loan_Type,
                //            Loan_Master.Scheme_Id,
                //            Loan_Schemes.Scheme_Name,
                //            Sum(Loan_Trn.Disb_Amt)  - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OS,
                //            CAST(0 as float) AS Prl_OB,
                //            CAST(0 as float) AS Int_OB,
                //            CAST(0 as float) AS PI_OB,
                //            CAST(0 as float) AS IOD_OB,
                //            Sum(Loan_Trn.Prl_Sched) AS Prl_Sched,
                //            Sum(Loan_Trn.Disb_Amt) AS Disb_Amt, 
                //            Sum(Loan_Trn.Disb_Amt) AS PrlPayment,
                //            Sum(Loan_Trn.PrlColl_Amt) AS PrlColl_Amt,
                //            Sum(Loan_Trn.IntCalc_Amt) AS IntCalc_Amt,
                //            Sum(Loan_Trn.IntColl_Amt) AS IntColl_Amt,
                //            Sum(Loan_Trn.PICalc_Amt) AS PICalc_Amt,
                //            Sum(Loan_Trn.PIColl_Amt) AS PIColl_Amt,
                //            ISNULL(Sum(Loan_Trn.IODCalc_Amt),0) AS IODCalc_Amt,
                //            ISNULL(Sum(Loan_Trn.IODColl_Amt),0) AS IODColl_Amt,
                //            Sum(Loan_Trn.Prl_Sched) - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OD,
                //            Sum(Loan_Trn.IntCalc_Amt) - Sum(Loan_Trn.IntColl_Amt) AS Int_Bal,
                //            Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) AS PI_Bal,
                //            ISNULL(Sum(Loan_Trn.IODCalc_Amt)  - Sum(Loan_Trn.IODColl_Amt),0) AS IOD_Bal
                //            FROM Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id 
                //            INNER JOIN Mem_Master ON Loan_Master.Mem_Id = Mem_Master.mem_Id 
                //            INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id 
                //            Where Loan_Trn.TrnTr_Delete = 0 AND CAST(Loan_Trn.Trn_Date as date) BETWEEN @fromDate AND @toDate AND Loan_Master.Loan_Type = @loanType 
                //            GROUP BY Loan_Trn.Loan_id,Loan_Master.Loan_Type,Loan_Master.Loan_No,Loan_Master.Mem_Id,Loan_Master.roi,Loan_Master.San_date,Mem_Master.memberNo,Mem_Master.PerNo,Mem_Master.memberName,Mem_Master.Token_PersonNo,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name
                //        )
                //        SELECT Loan_id, Loan_No,San_date,Mem_Id, roi,memberNo, PerNo, memberNo,memberName, Token_PersonNo, Scheme_Id, Scheme_Name, 
                //            Sum(Prl_OS) AS Prl_OS, Sum(Prl_OB) AS Prl_OB, Sum(Int_OB) AS Int_OB, Sum(PI_OB) AS PI_OB, Sum(IOD_OB) AS IOD_OB, Sum(Prl_Sched) AS Prl_Sched,
                //            Sum(Disb_Amt) AS Disb_Amt, Sum(PrlPayment) AS PrlPayment, Sum(PrlColl_Amt) AS PrlColl_Amt, 
                //            Sum(IntCalc_Amt) AS IntCalc_Amt, Sum(IntColl_Amt) AS IntColl_Amt , 
                //            Sum(PICalc_Amt) AS PICalc_Amt, Sum(PIColl_Amt)AS PIColl_Amt,
                //            Sum(IODCalc_Amt) AS IODCalc_Amt, Sum(IODColl_Amt)AS IODColl_Amt,
                //            Sum(Prl_OD) AS Prl_OD,
                //            Sum(Int_Bal) AS Int_Bal,
                //            Sum(PI_Bal) AS PI_Bal,
                //            Sum(IOD_Bal) AS IOD_Bal
                //                FROM Dt1
                //                GROUP BY Loan_id,Loan_Type,Loan_No,San_date,Mem_Id,roi,memberNo,PerNo,memberName,Token_PersonNo,Scheme_Id,Scheme_Name"
                //        , new NpgsqlParameter("@fromDate", fromDate.ToString("yyyy-MM-dd"))
                //        , new NpgsqlParameter("@toDate", toDate.ToString("yyyy-MM-dd"))
                //        , new NpgsqlParameter("@loanType", loanType)).ToListAsync();
                //loanList = loanList.Where(x => x.Loan_Id == 2098).ToList();
                #endregion

                #region linq commented
                // First, define the data we need for our CTE equivalent - we'll use anonymous types for this
                // Part 1 of the CTE - data before fromDate
                // var beforeFromDateQuery =
                //     from loanTrn in CSISContext.Loan_Trn
                //     join loanMaster in CSISContext.Loan_Master on loanTrn.Loan_Id equals loanMaster.Loan_Id
                //     join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                //     join loanSchemes in CSISContext.Loan_Schemes on loanMaster.Scheme_Id equals loanSchemes.Scheme_Id
                //     where loanTrn.TrnTr_Delete == false &&
                //           loanTrn.Trn_Date.Date < fromDate.Date &&
                //           loanMaster.Loan_Type == loanType
                //     group new { loanTrn, loanMaster, memMaster, loanSchemes } by new
                //     {
                //         loanTrn.Loan_Id,
                //         loanMaster.Loan_No,
                //         loanMaster.Mem_Id,
                //         loanMaster.Roi,
                //         loanMaster.San_Date,
                //         memMaster.memberno,
                //         memMaster.perno,
                //         memMaster.membername,
                //         memMaster.token_personno,
                //         loanMaster.Loan_Type,
                //         loanMaster.Scheme_Id,
                //         loanSchemes.Scheme_Name
                //     } into g
                //     let prlOS = g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt)
                //     let intOB = g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt)
                //     let piOB = g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt)
                //     let iodOB = g.Sum(x => x.loanTrn.IODCalc_Amt) - g.Sum(x => x.loanTrn.IODColl_Amt)
                //     where prlOS > 0 || intOB > 0 || piOB > 0 || iodOB > 0
                //     select new
                //     {
                //         g.Key.Loan_Id,
                //         g.Key.Loan_No,
                //         g.Key.Mem_Id,
                //         g.Key.Roi,
                //         g.Key.San_Date,
                //         g.Key.memberno,
                //         g.Key.perno,
                //         g.Key.membername,
                //         g.Key.token_personno,
                //         g.Key.Loan_Type,
                //         g.Key.Scheme_Id,
                //         g.Key.Scheme_Name,
                //         Prl_OS = prlOS,
                //         Prl_OB = prlOS,
                //         Int_OB = intOB,
                //         PI_OB = piOB,
                //         IOD_OB = iodOB,
                //         Prl_Sched = 0f,
                //         Disb_Amt = g.Sum(x => x.loanTrn.Disb_Amt),
                //         PrlPayment = 0f,
                //         PrlColl_Amt = 0f,
                //         IntCalc_Amt = 0f,
                //         IntColl_Amt = 0f,
                //         PICalc_Amt = 0f,
                //         PIColl_Amt = 0f,
                //         IODCalc_Amt = 0f,
                //         IODColl_Amt = 0f,
                //         Prl_OD = g.Sum(x => x.loanTrn.Prl_Sched) - g.Sum(x => x.loanTrn.PrlColl_Amt),
                //         Int_Bal = 0f,
                //         PI_Bal = 0f,
                //         IOD_Bal = 0f
                //     };

                // // Part 2 of the CTE - data between fromDate and toDate
                // var betweenDatesQuery =
                //     from loanTrn in CSISContext.Loan_Trn
                //     join loanMaster in CSISContext.Loan_Master on loanTrn.Loan_Id equals loanMaster.Loan_Id
                //     join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                //     join loanSchemes in CSISContext.Loan_Schemes on loanMaster.Scheme_Id equals loanSchemes.Scheme_Id
                //     where loanTrn.TrnTr_Delete == false &&
                //           loanTrn.Trn_Date.Date >= fromDate.Date &&
                //           loanTrn.Trn_Date.Date <= toDate.Date &&
                //           loanMaster.Loan_Type == loanType
                //     group new { loanTrn, loanMaster, memMaster, loanSchemes } by new
                //     {
                //         loanTrn.Loan_Id,
                //         loanMaster.Loan_No,
                //         loanMaster.Mem_Id,
                //         loanMaster.Roi,
                //         loanMaster.San_Date,
                //         memMaster.memberno,
                //         memMaster.perno,
                //         memMaster.membername,
                //         memMaster.token_personno,
                //         loanMaster.Loan_Type,
                //         loanMaster.Scheme_Id,
                //         loanSchemes.Scheme_Name
                //     } into g
                //     select new
                //     {
                //         g.Key.Loan_Id,
                //         g.Key.Loan_No,
                //         g.Key.Mem_Id,
                //         g.Key.Roi,
                //         g.Key.San_Date,
                //         g.Key.memberno,
                //         g.Key.perno,
                //         g.Key.membername,
                //         g.Key.token_personno,
                //         g.Key.Loan_Type,
                //         g.Key.Scheme_Id,
                //         g.Key.Scheme_Name,
                //         Prl_OS = g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt),
                //         Prl_OB = 0f,
                //         Int_OB = 0f,
                //         PI_OB = 0f,
                //         IOD_OB = 0f,
                //         Prl_Sched = g.Sum(x => x.loanTrn.Prl_Sched),
                //         Disb_Amt = g.Sum(x => x.loanTrn.Disb_Amt),
                //         PrlPayment = g.Sum(x => x.loanTrn.Disb_Amt),
                //         PrlColl_Amt = g.Sum(x => x.loanTrn.PrlColl_Amt),
                //         IntCalc_Amt = g.Sum(x => x.loanTrn.IntCalc_Amt),
                //         IntColl_Amt = g.Sum(x => x.loanTrn.IntColl_Amt),
                //         PICalc_Amt = g.Sum(x => x.loanTrn.PICalc_Amt),
                //         PIColl_Amt = g.Sum(x => x.loanTrn.PIColl_Amt),
                //         IODCalc_Amt = g.Sum(x => x.loanTrn.IODCalc_Amt),
                //         IODColl_Amt = g.Sum(x => x.loanTrn.IODColl_Amt),
                //         Prl_OD = g.Sum(x => x.loanTrn.Prl_Sched) - g.Sum(x => x.loanTrn.PrlColl_Amt),
                //         Int_Bal = g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt),
                //         PI_Bal = g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt),
                //         IOD_Bal = g.Sum(x => x.loanTrn.IODCalc_Amt) - g.Sum(x => x.loanTrn.IODColl_Amt)
                //     };

                // // Union the two queries to create the equivalent of the CTE

                //var combinedQuery = beforeFromDateQuery.Union(betweenDatesQuery);


                // // Finally, apply the aggregation on the combined result
                // var finalQuery =
                //     from data in combinedQuery
                //     group data by new
                //     {
                //         data.Loan_id,
                //         data.Loan_No,
                //         data.Mem_Id,
                //         data.roi,
                //         data.San_date,
                //         data.memberNo,
                //         data.PerNo,
                //         data.memberName,
                //         data.Token_PersonNo,
                //         data.Scheme_Id,
                //         data.Scheme_Name
                //     } into g
                //     select new rptFALoanOutstanding
                //     {
                //         Loan_Id = g.Key.Loan_id,
                //         Loan_No = g.Key.Loan_No,
                //         San_Date = g.Key.San_date,
                //         Mem_Id = g.Key.Mem_Id,
                //         Roi = g.Key.roi,
                //         MemberNo = g.Key.memberNo,
                //         PerNo = g.Key.PerNo,
                //         MemberName = g.Key.memberName,
                //         Token_PersonNo = g.Key.Token_PersonNo,
                //         Scheme_Id = g.Key.Scheme_Id,
                //         Scheme_Name = g.Key.Scheme_Name,
                //         Prl_OS = g.Sum(x => x.Prl_OS),
                //         Prl_OB = g.Sum(x => x.Prl_OB),
                //         Int_OB = g.Sum(x => x.Int_OB),
                //         PI_OB = g.Sum(x => x.PI_OB),
                //         IOD_OB = g.Sum(x => x.IOD_OB),
                //         Prl_Sched = g.Sum(x => x.Prl_Sched),
                //         Disb_Amt = g.Sum(x => x.Disb_Amt),
                //         PrlPayment = g.Sum(x => x.PrlPayment),
                //         PrlColl_Amt = g.Sum(x => x.PrlColl_Amt),
                //         IntCalc_Amt = g.Sum(x => x.IntCalc_Amt),
                //         IntColl_Amt = g.Sum(x => x.IntColl_Amt),
                //         PICalc_Amt = g.Sum(x => x.PICalc_Amt),
                //         PIColl_Amt = g.Sum(x => x.PIColl_Amt),
                //         IODCalc_Amt = g.Sum(x => x.IODCalc_Amt),
                //         IODColl_Amt = g.Sum(x => x.IODColl_Amt),
                //         Prl_OD = g.Sum(x => x.Prl_OD),
                //         Int_Bal = g.Sum(x => x.Int_Bal),
                //         PI_Bal = g.Sum(x => x.PI_Bal),
                //         IOD_Bal = g.Sum(x => x.IOD_Bal)
                //     };

                // // Execute the query
                // loanList = await finalQuery.ToListAsync();
                #endregion

                #region linq
                // First, define the data we need for our CTE equivalent - we'll use anonymous types for this
                // Part 1 of the CTE - data before fromDate
                var beforeFromDateQuery =
                    from loanTrn in CSISContext.Loan_Trn
                    join loanMaster in CSISContext.Loan_Master on loanTrn.Loan_Id equals loanMaster.Loan_Id
                    join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                    join loanSchemes in CSISContext.Loan_Schemes on loanMaster.Scheme_Id equals loanSchemes.Scheme_Id
                    where loanTrn.TrnTr_Delete == false &&
                          loanTrn.Trn_Date.Date < fromDate.Date &&
                          loanMaster.Loan_Type == loanType &&
                          loanTrn.BrCode == brCode && 
                          loanMaster.BrCode == brCode && 
                          memMaster.brcode == brCode  
                    group new { loanTrn, loanMaster, memMaster, loanSchemes } by new
                    {
                        loanTrn.Loan_Id,
                        loanMaster.Loan_No,
                        loanMaster.Mem_Id,
                        loanMaster.Roi,
                        loanMaster.San_Date,
                        memMaster.memberno,
                        memMaster.perno,
                        memMaster.membername,
                        memMaster.token_personno,
                        loanMaster.Loan_Type,
                        loanMaster.Scheme_Id,
                        loanSchemes.Scheme_Name
                    } into g
                    //let prlOS = g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt)
                    //let intOB = g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt)
                    //let piOB = g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt)
                    //let iodOB = g.Sum(x => x.loanTrn.IODCalc_Amt) - g.Sum(x => x.loanTrn.IODColl_Amt)
                    //where prlOS > 0 || intOB > 0 || piOB > 0 || iodOB > 0
                    where g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt) >0 ||
                            g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt) >0 ||
                            g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt) >0 ||
                            g.Sum(x => x.loanTrn.IODCalc_Amt) - g.Sum(x => x.loanTrn.IODColl_Amt) >0
                    select new LoanQueryResult // Create a common class to ensure type compatibility
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Mem_Id = g.Key.Mem_Id,
                        Roi = g.Key.Roi,
                        San_Date = g.Key.San_Date,
                        MemberNo = g.Key.memberno,
                        PerNo = g.Key.perno,
                        MemberName = g.Key.membername,
                        Token_PersonNo = g.Key.token_personno,
                        Loan_Type = g.Key.Loan_Type,
                        Scheme_Id = g.Key.Scheme_Id,
                        Scheme_Name = g.Key.Scheme_Name,
                        Prl_OS = g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt), ///prlOS,
                        Prl_OB = g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt), ///prlOS,
                        Int_OB = g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt), ///intOB,
                        PI_OB = g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt), ///piOB,
                        IOD_OB = g.Sum(x => x.loanTrn.IODCalc_Amt) - g.Sum(x => x.loanTrn.IODColl_Amt), ///iodOB,
                        Prl_Sched = 0f,
                        Disb_Amt = g.Sum(x => x.loanTrn.Disb_Amt),
                        PrlPayment = 0f,
                        PrlColl_Amt = 0f,
                        IntCalc_Amt = 0f,
                        IntColl_Amt = 0f,
                        PICalc_Amt = 0f,
                        PIColl_Amt = 0f,
                        IODCalc_Amt = 0f,
                        IODColl_Amt = 0f,
                        Prl_OD = g.Sum(x => x.loanTrn.Prl_Sched) - g.Sum(x => x.loanTrn.PrlColl_Amt),
                        Int_Bal = 0f,
                        PI_Bal = 0f,
                        IOD_Bal = 0f
                    };

                // Part 2 of the CTE - data between fromDate and toDate
                var betweenDatesQuery =
                    from loanTrn in CSISContext.Loan_Trn
                    join loanMaster in CSISContext.Loan_Master on loanTrn.Loan_Id equals loanMaster.Loan_Id
                    join memMaster in CSISContext.mem_master on loanMaster.Mem_Id equals memMaster.mem_id
                    join loanSchemes in CSISContext.Loan_Schemes on loanMaster.Scheme_Id equals loanSchemes.Scheme_Id
                    where loanTrn.TrnTr_Delete == false &&
                          loanTrn.Trn_Date.Date >= fromDate.Date &&
                          loanTrn.Trn_Date.Date <= toDate.Date &&
                          loanMaster.Loan_Type == loanType &&
                          loanTrn.BrCode == brCode && 
                          loanMaster.BrCode == brCode && 
                          memMaster.brcode == brCode 
                    group new { loanTrn, loanMaster, memMaster, loanSchemes } by new
                    {
                        loanTrn.Loan_Id,
                        loanMaster.Loan_No,
                        loanMaster.Mem_Id,
                        loanMaster.Roi,
                        loanMaster.San_Date,
                        memMaster.memberno,
                        memMaster.perno,
                        memMaster.membername,
                        memMaster.token_personno,
                        loanMaster.Loan_Type,
                        loanMaster.Scheme_Id,
                        loanSchemes.Scheme_Name
                    } into g
                    select new LoanQueryResult // Using the same class for type compatibility
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        Mem_Id = g.Key.Mem_Id,
                        Roi = g.Key.Roi,
                        San_Date = g.Key.San_Date,
                        MemberNo = g.Key.memberno,
                        PerNo = g.Key.perno,
                        MemberName = g.Key.membername,
                        Token_PersonNo = g.Key.token_personno,
                        Loan_Type = g.Key.Loan_Type,
                        Scheme_Id = g.Key.Scheme_Id,
                        Scheme_Name = g.Key.Scheme_Name,
                        Prl_OS = g.Sum(x => x.loanTrn.Disb_Amt) - g.Sum(x => x.loanTrn.PrlColl_Amt),
                        Prl_OB = 0f,
                        Int_OB = 0f,
                        PI_OB = 0f,
                        IOD_OB = 0f,
                        Prl_Sched = g.Sum(x => x.loanTrn.Prl_Sched),
                        Disb_Amt = g.Sum(x => x.loanTrn.Disb_Amt),
                        PrlPayment = g.Sum(x => x.loanTrn.Disb_Amt),
                        PrlColl_Amt = g.Sum(x => x.loanTrn.PrlColl_Amt),
                        IntCalc_Amt = g.Sum(x => x.loanTrn.IntCalc_Amt),
                        IntColl_Amt = g.Sum(x => x.loanTrn.IntColl_Amt),
                        PICalc_Amt = g.Sum(x => x.loanTrn.PICalc_Amt),
                        PIColl_Amt = g.Sum(x => x.loanTrn.PIColl_Amt),
                        IODCalc_Amt = g.Sum(x => x.loanTrn.IODCalc_Amt),
                        IODColl_Amt = g.Sum(x => x.loanTrn.IODColl_Amt),
                        Prl_OD = g.Sum(x => x.loanTrn.Prl_Sched) - g.Sum(x => x.loanTrn.PrlColl_Amt),
                        Int_Bal = g.Sum(x => x.loanTrn.IntCalc_Amt) - g.Sum(x => x.loanTrn.IntColl_Amt),
                        PI_Bal = g.Sum(x => x.loanTrn.PICalc_Amt) - g.Sum(x => x.loanTrn.PIColl_Amt),
                        IOD_Bal = g.Sum(x => x.loanTrn.IODCalc_Amt) - g.Sum(x => x.loanTrn.IODColl_Amt)
                    };

                // Union the two queries to create the equivalent of the CTE
                var combinedQuery = beforeFromDateQuery.Concat(betweenDatesQuery);

                // Finally, apply the aggregation on the combined result
                var finalQuery =
                    from data in combinedQuery
                    group data by new
                    {
                        data.Loan_Id,
                        data.Loan_No,
                        data.Mem_Id,
                        data.Roi,
                        data.San_Date,
                        data.MemberNo,
                        data.PerNo,
                        data.MemberName,
                        data.Token_PersonNo,
                        data.Scheme_Id,
                        data.Scheme_Name
                    } into g
                    select new rptFALoanOutstanding
                    {
                        Loan_Id = g.Key.Loan_Id,
                        Loan_No = g.Key.Loan_No,
                        San_Date = g.Key.San_Date,
                        Mem_Id = g.Key.Mem_Id,
                        Roi = g.Key.Roi,
                        MemberNo = g.Key.MemberNo,
                        PerNo = g.Key.PerNo,
                        MemberName = g.Key.MemberName,
                        Token_PersonNo = g.Key.Token_PersonNo,
                        Scheme_Id = g.Key.Scheme_Id,
                        Scheme_Name = g.Key.Scheme_Name,
                        Prl_OS = g.Sum(x => x.Prl_OS),
                        Prl_OB = g.Sum(x => x.Prl_OB),
                        Int_OB = g.Sum(x => x.Int_OB),
                        PI_OB = g.Sum(x => x.PI_OB),
                        IOD_OB = g.Sum(x => x.IOD_OB),
                        Prl_Sched = g.Sum(x => x.Prl_Sched),
                        Disb_Amt = g.Sum(x => x.Disb_Amt),
                        PrlPayment = g.Sum(x => x.PrlPayment),
                        PrlColl_Amt = g.Sum(x => x.PrlColl_Amt),
                        IntCalc_Amt = g.Sum(x => x.IntCalc_Amt),
                        IntColl_Amt = g.Sum(x => x.IntColl_Amt),
                        PICalc_Amt = g.Sum(x => x.PICalc_Amt),
                        PIColl_Amt = g.Sum(x => x.PIColl_Amt),
                        IODCalc_Amt = g.Sum(x => x.IODCalc_Amt),
                        IODColl_Amt = g.Sum(x => x.IODColl_Amt),
                        Prl_OD = g.Sum(x => x.Prl_OD),
                        Int_Bal = g.Sum(x => x.Int_Bal),
                        PI_Bal = g.Sum(x => x.PI_Bal),
                        IOD_Bal = g.Sum(x => x.IOD_Bal)
                    };

                // Execute the query
                loanList = await finalQuery.ToListAsync();
                DateTime maxIntCalcDate;
                double intCalc = 0;
                double piCalc = 0;
                DateTime DueDate = toDate;
                DateTime PIFromDate;
                int noOfMonths = 0;
                #endregion 
                foreach (var os in loanList )
                {
                    if (os.Prl_OD < 0) os.Prl_OD = 0;
                    var result = await (from lt in CSISContext.Loan_Trn
                                        join lm in CSISContext.Loan_Master on lt.Loan_Id equals lm.Loan_Id
                                        where lt.Loan_Id == os.Loan_Id &&
                                              lt.Trn_Date.Date <= toDate.Date &&
                                              lt.TrnTr_Delete == false
                                        group new { lt, lm } by new
                                        {
                                            lm.Roi,
                                            lm.San_Date,
                                            lm.Prl_Prd
                                        } into g
                                        select new rptLoanOutstanding
                                        {
                                            Roi = g.Key.Roi,
                                            Prl_Prd = g.Key.Prl_Prd,
                                            Disb_Date = g.Key.San_Date,
                                            IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                            PICalc_Date = g.Max(x => x.lt.PICalc_Date)
                                        }).FirstOrDefaultAsync();
                    if (result!.IntCalc_Date == null)
                        maxIntCalcDate = result.Disb_Date;
                    else
                        maxIntCalcDate = (DateTime)result.IntCalc_Date;
                    if (loanType == 2 || loanType == 5)
                    {
                        DueDate = Utilities.AddMonths(result.Disb_Date, result.Prl_Prd );
                    }
                    if (loanType == 3 || loanType == 4)
                    {
                        DueDate = await (from lt in CSISContext.Lien_Trn
                                         join td in CSISContext.TermDeposit_Master on lt.TD_Id equals td.TD_Id
                                         where lt.Loan_Id == os.Loan_Id
                                         select td.MaturityDate.Date).FirstOrDefaultAsync();
                    }
                    if (Utilities.GetNoOfDays(toDate, DueDate) > 0)
                    {
                        intCalc = Utilities.Calculate_Interest(os.Prl_OS, result.Roi, Utilities.GetNoOfDays(DueDate, maxIntCalcDate));
                        intCalc += Utilities.Calculate_Interest(os.Prl_OS, result.Roi, Utilities.GetNoOfDays(toDate.AddDays(1), DueDate));
                        os.Int_Bal = os.IntCalc_Amt - os.IntColl_Amt + intCalc;
                        if (result.PICalc_Date != null)
                            PIFromDate = (DateTime)result.PICalc_Date;
                        else
                            PIFromDate = DueDate;
                        piCalc = Utilities.Calculate_Interest(os.Prl_OS, result.Roi, Utilities.GetNoOfDays(toDate.AddDays(1), PIFromDate));
                        os.PI_Bal = os.PICalc_Amt - os.PIColl_Amt + piCalc;
                    }
                    else
                    {
                        intCalc = Utilities.Calculate_Interest(os.Prl_OS, result.Roi, Utilities.GetNoOfDays(toDate.AddDays(1), maxIntCalcDate));
                        os.AccruedInt = os.IntCalc_Amt - os.IntColl_Amt + intCalc;
                        os.Int_Bal = 0;
                    }
                    noOfMonths = Utilities.GetNoOfMonths(DueDate, toDate);
                    //noOfMonths = Utilities.GetNoOfMonths(DueDate, toDate);
                    //if (noOfMonths == 0)
                    //{
                    //    if(DueDate.Month == toDate.Month && DueDate.Year == toDate.Year )
                    //    noOfMonths = 1;
                    //}
                    if (noOfMonths <= 3)
                        os.OD3M = os.Prl_OD;
                    if (noOfMonths > 3 && noOfMonths >= 6)
                        os.OD3M_6M = os.Prl_OD;
                    if (noOfMonths > 6 && noOfMonths >= 12)
                        os.OD7M_12M = os.Prl_OD;
                    if (noOfMonths > 13 && noOfMonths <= 24)
                        os.OD25M_36M = os.Prl_OD;
                    if (noOfMonths > 24 && noOfMonths <= 36)
                        os.OD25M_36M = os.Prl_OD;
                    if (noOfMonths > 36)
                        os.OD37M_Above = os.Prl_OD;
                }
            
                //foreach (var os in loanList)
                //{
                //    //os.Prl_OS = os.Prl_OB +  os.Disb_Amt - os.PrlColl_Amt;
                //    os.PI_Bal = os.PI_OB + os.PICalc_Amt - os.PIColl_Amt;
                //    os.Int_Bal = os.Int_OB + os.IntCalc_Amt - os.IntColl_Amt;
                //    os.IOD_Bal = os.IOD_OB + os.IODCalc_Amt - os.IODColl_Amt;
                //    if (os.Prl_OD < 0) os.Prl_OD = 0;
                //    if (os.Int_Bal < 0) os.Int_Bal = 0;
                //    if (os.PI_Bal < 0) os.PI_Bal = 0;
                //    if (os.IOD_Bal < 0) os.IOD_Bal = 0;
                //    if (os.Int_OB < 0) os.Int_OB = 0;
                //    if (os.PI_OB < 0) os.PI_OB = 0;
                //    if (os.IOD_OB < 0) os.IOD_OB = 0;
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Error in fetching loan data for audit schedule " + ex.Message);
                loanList = new();
            }
            return loanList;
        }

        public async Task<List<rptFALoanOutstanding>> GetLoanOutstandingJLFDRD(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            List<rptFALoanOutstanding> loanList = new List<rptFALoanOutstanding>();
            DateTime maxIntCalcDate;
            double intCalc = 0;
            double piCalc = 0;
            DateTime DueDate = toDate;
            DateTime PIFromDate;
            int noOfMonths = 0;
            try
            {
                loanList = await GetLoanOutstandingForFA_HL(fromDate, toDate, loanType, brCode);
                foreach (var loan in loanList)
                {
                    //if(loan.Loan_id == 959 || loan.Loan_id == 971)
                    //{
                    //    errorMessage = "";
                    //}
                    rptLoanOutstanding lnDetails = new rptLoanOutstanding();
                    #region query
                    //lnDetails = await CSISContext.Database.SqlQueryRaw<rptLoanOutstanding>(
                    //    @"SELECT Loan_Master.roi, 
                    //            Loan_Master.prl_prd,
                    //            Max(Loan_Trn.Disb_Date) AS Disb_Date, 
                    //            Max(Loan_Trn.IntCalc_Date) AS IntCalc_Date,
                    //            Max(Loan_Trn.PICalc_Date) AS PICalc_Date
                    //            FROM Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id 
                    //            WHERE Loan_Trn.Loan_id = @loanId AND CAST(Loan_Trn.Trn_Date AS date) <=@toDate AND Loan_Trn.TrnTr_Delete = 0
                    //            GROUP BY Loan_Master.roi,Loan_Master.prl_prd"
                    //        , new NpgsqlParameter("@loanId", loan.Loan_Id)
                    //        , new NpgsqlParameter("@toDate", toDate)).FirstAsync();
                    #endregion

                    #region linq
                    lnDetails = await (from lt in CSISContext.Loan_Trn
                                       join lm in CSISContext.Loan_Master on lt.Loan_Id equals lm.Loan_Id
                                       where lt.Loan_Id == loan.Loan_Id
                                             && lt.Trn_Date.Date <= toDate.Date
                                             && lt.TrnTr_Delete == false
                                             && lt.BrCode == brCode && lt.Voc_Status == "V" 
                                             && lm.BrCode == brCode && lm.Voc_Status == "V"
                                       group new { lt, lm } by new { lm.Roi, lm.Prl_Prd } into g
                                       select new rptLoanOutstanding
                                       {
                                           Roi = g.Key.Roi,
                                           Prl_Prd = g.Key.Prl_Prd,
                                           Disb_Date = g.Max(x => x.lt.Disb_Date)!.Value,
                                           IntCalc_Date = g.Max(x => x.lt.IntCalc_Date),
                                           PICalc_Date = g.Max(x => x.lt.PICalc_Date)
                                       })
                   .FirstAsync();

                    #endregion


                    if (lnDetails.IntCalc_Date == null)
                        maxIntCalcDate = lnDetails.Disb_Date;
                    else
                        maxIntCalcDate = (DateTime)lnDetails.IntCalc_Date;
                    if (loanType == 2 || loanType == 5)
                    {
                        DueDate = Utilities.AddMonths(lnDetails.Disb_Date, lnDetails.Prl_Prd);
                    }
                    if (loanType == 3 || loanType == 4)
                    {
                        #region query
                        //DueDate = await CSISContext.Database.SqlQueryRaw<DateTime>(
                        //    @"SELECT CAST(TermDeposit_Master.MaturityDate as date) AS Due_Date 
                        //        FROM Lien_Tr INNER JOIN TermDeposit_Master ON Lien_Tr.TD_Id = TermDeposit_Master.TD_Id
                        //        WHERE Lien_Tr.Loan_id = @loanId"
                        //    , new NpgsqlParameter("@loanId", loan.Loan_Id)).FirstAsync();
                        #endregion 

                        DueDate = await (from lt in CSISContext.Lien_Trn
                                         join td in CSISContext.TermDeposit_Master on lt.TD_Id equals td.TD_Id
                                         where lt.Loan_Id == loan.Loan_Id
                                         && lt.BrCode == brCode && lt.Voc_Status == "V"
                                         && td.BrCode == brCode && td.Voc_Status == "V"
                                         select td.MaturityDate.Date)
                 .FirstAsync();
                    }
                    if (Utilities.GetNoOfDays(toDate, DueDate) > 0)
                    {
                        intCalc = Utilities.Calculate_Interest(loan.Prl_OS, lnDetails.Roi, Utilities.GetNoOfDays(DueDate, maxIntCalcDate));
                        intCalc += Utilities.Calculate_Interest(loan.Prl_OS, lnDetails.Roi, Utilities.GetNoOfDays(toDate.AddDays(1), DueDate));
                        loan.Int_Bal = loan.IntCalc_Amt - loan.IntColl_Amt + intCalc;
                        if (lnDetails.PICalc_Date != null)
                            PIFromDate = (DateTime)lnDetails.PICalc_Date;
                        else
                            PIFromDate = DueDate;
                        piCalc = Utilities.Calculate_Interest(loan.Prl_OS, lnDetails.Roi, Utilities.GetNoOfDays(toDate.AddDays(1), PIFromDate));
                        loan.PI_Bal = loan.PICalc_Amt - loan.PIColl_Amt + piCalc;
                    }
                    else
                    {
                        intCalc = Utilities.Calculate_Interest(loan.Prl_OS, lnDetails.Roi, Utilities.GetNoOfDays(toDate.AddDays(1), maxIntCalcDate));
                        loan.AccruedInt = loan.IntCalc_Amt - loan.IntColl_Amt + intCalc;
                        loan.Int_Bal = 0;
                    }
                    noOfMonths = Utilities.GetMonthsBetweenDates(DueDate, toDate);

                    if (noOfMonths <= 3)
                        loan.OD3M = loan.Prl_OD;
                    if (noOfMonths > 3 && noOfMonths >= 6)
                        loan.OD3M_6M = loan.Prl_OD;
                    if (noOfMonths > 6 && noOfMonths >= 12)
                        loan.OD7M_12M = loan.Prl_OD;
                    if (noOfMonths > 13 && noOfMonths <= 24)
                        loan.OD25M_36M = loan.Prl_OD;
                    if (noOfMonths > 24 && noOfMonths <= 36)
                        loan.OD25M_36M = loan.Prl_OD;
                    if (noOfMonths > 36)
                        loan.OD37M_Above = loan.Prl_OD;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }

        public async Task<List<rptTermDepositOutstanding>> GetTermDepositOutstandingFA(DateTime fromDate, DateTime toDate, string TDType, string brCode)
        {
            List<rptTermDepositOutstanding> tdOSList = new List<rptTermDepositOutstanding>();
            double intCalc = 0;
            DateTime maxIntCalcDate = DateTime.MinValue;
            try
            {
                tdOSList = await GetTermDepositOutstandingFAData(fromDate, toDate, TDType, brCode);

                foreach (var td in tdOSList)
                {
                    if (td.InterestAppliedDate != null)
                        maxIntCalcDate = (DateTime)td.InterestAppliedDate;
                    else
                        maxIntCalcDate = td.ValueDate;
                    if (TDType == "F")
                    {
                        if (Utilities.GetNoOfDays(toDate, td.MaturityDate) > 0)
                        {
                            intCalc = Utilities.Calculate_Interest(td.DepositReceiptAmount, td.RateOfInterest, Utilities.GetNoOfDays(td.MaturityDate, maxIntCalcDate));
                        }
                        else
                        {
                            intCalc = Utilities.Calculate_Interest(td.Deposit_OB + td.DepositReceiptAmount - td.DepositPaidAmount, td.RateOfInterest, Utilities.GetNoOfDays(toDate, maxIntCalcDate) + 1);
                        }
                    }
                    if (TDType == "R")
                    {
                        if (td.Deposit_OB + td.DepositReceiptAmount - td.DepositPaidAmount > 0)
                        {
                            intCalc = Utilities.GetRDMaturityAmount(td.DepositAmount, (int)((td.Deposit_OB + td.DepositReceiptAmount) / td.DepositAmount), td.RateOfInterest, td.IsCompoundInterest);
                            intCalc -= (td.Deposit_OB + td.DepositReceiptAmount - td.DepositPaidAmount);
                        }
                        else
                            intCalc = 0;
                    }
                    td.InterestCalculatedAmount += intCalc;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return tdOSList;
        }

        private async Task<List<rptTermDepositOutstanding>> GetTermDepositOutstandingFAData(DateTime fromDate, DateTime toDate, string TDType, string brCode)
        {
            List<rptTermDepositOutstanding> tdList = new List<rptTermDepositOutstanding>();
            //string sql = "";
            try
            {
                #region query
                //         sql = @"WITH Dt1
                //                     AS
                //                     (
                //                         SELECT TermDeposit_Trn.TD_Id,
                //                         Sum(TermDeposit_Trn.DepositReceiptAmount) - Sum(TermDeposit_Trn.DepositPaidAmount)  AS Deposit_OB,
                //                         Sum(InterestCalculatedAmount) - Sum(InterestPaidAmount) AS Interest_OB,
                //                         CAST(0 as float) AS DepositReceiptAmount,
                //                         CAST(0 as float) AS DepositPaidAmount,
                //                         CAST(0 as float) AS InterestCalculatedAmount,
                //                         Max(TermDeposit_Trn.InterestAppliedDate) AS InterestAppliedDate,
                //                         CAST(0 as float) AS InterestPaidAmount 
                //                         From TermDeposit_Trn 
                //	INNER JOIN TermDeposit_Master on TermDeposit_Trn.TD_Id = TermDeposit_Master.TD_Id 
                //                         Where CAST(TermDeposit_Trn.Trn_Date as date) <@fromDate And TermDeposit_Trn.TD_Delete = 0";
                //         if (TDType == "F")
                //             sql += @" AND TermDeposit_Master.TDScheme_Id <40000";
                //         else
                //             sql += @" AND TermDeposit_Master.TDScheme_Id > 40000";

                //         sql += @" GROUP BY TermDeposit_Trn.TD_Id
                //                         HAVING Sum(TermDeposit_Trn.DepositReceiptAmount) - Sum(TermDeposit_Trn.DepositPaidAmount)  >0
                //                     UNION
                //                         SELECT TermDeposit_Trn.TD_Id, 
                //                         CAST(0 as float)  AS Deposit_OB,
                //                         CAST(0 as float) AS Interest_OB,
                //                         Sum(TermDeposit_Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                //                         Sum(TermDeposit_Trn.DepositPaidAmount) AS DepositPaidAmount,
                //                         Sum(TermDeposit_Trn.InterestCalculatedAmount) AS InterestCalculatedAmount,
                //                         Max(TermDeposit_Trn.InterestAppliedDate) AS InterestAppliedDate,
                //                         CAST(0 as float) AS InterestPaidAmount 
                //                         From TermDeposit_Trn INNER JOIN TermDeposit_Master ON TermDeposit_Trn.TD_Id = TermDeposit_Master.TD_Id
                //                         Where  CAST(TermDeposit_Trn.Trn_Date as date) BETWEEN @fromDate AND @toDate And TermDeposit_Trn.TD_Delete = 0";
                //         if (TDType == "F")
                //             sql += @" AND TermDeposit_Master.TDScheme_Id <40000";
                //         else
                //             sql += @" AND TermDeposit_Master.TDScheme_Id >40000";
                //         sql += @" GROUP BY TermDeposit_Trn.TD_Id
                //UNION
                //                         SELECT TermDeposit_Trn.TD_Id, 
                //                         CAST(0 as float)  AS Deposit_OB,
                //                         CAST(0 as float) AS Interest_OB,
                //	CAST(0 as float) AS DepositReceiptAmount,
                //                         CAST(0 as float) AS DepositPaidAmount,
                //                         CAST(0 as float) AS InterestCalculatedAmount,
                //                         Max(TermDeposit_Trn.InterestAppliedDate) AS InterestAppliedDate,
                //                         Sum(TermDeposit_Trn.InterestPaidAmount) AS InterestPaidAmount 
                //                         From TermDeposit_Trn INNER JOIN TermDeposit_Master ON TermDeposit_Trn.TD_Id = TermDeposit_Master.TD_Id 
                //                         Where (TermDeposit_Trn.InterestPaidAmount >0) AND  CAST(TermDeposit_Trn.Trn_Date as date) BETWEEN @fromDate AND @toDate And TermDeposit_Trn.TD_Delete = 0";
                //         if (TDType == "F")
                //             sql += @" AND TermDeposit_Master.TDScheme_Id <40000";
                //         else
                //             sql += @" AND TermDeposit_Master.TDScheme_Id >40000";
                //         sql += @" GROUP BY TermDeposit_Trn.TD_Id 
                //                     )
                //                     SELECT Dt1.TD_Id,  
                //                     TD_No,
                //                     DepositAmount,
                //                     ValueDate,
                //                     MaturityDate,
                //                     RateOfInterest,
                //                     TermDeposit_Master.TDScheme_Id,
                //                     IsCompoundInterest,
                //                     MaturityAmount,
                //                     TDScheme_Name,
                //                     TermDeposit_Master.Mem_Id,
                //                     memberNo,
                //                     PerNo,
                //                     memberName,
                //                     Token_PersonNo,
                //                     Sum(Deposit_OB) AS Deposit_OB,
                //                     Sum(Interest_OB) AS Interest_OB,
                //                     Sum(DepositReceiptAmount) AS DepositReceiptAmount,
                //                     Sum(DepositPaidAmount) AS DepositPaidAmount,
                //                     Sum(InterestCalculatedAmount) AS InterestCalculatedAmount,
                //                     Max(InterestAppliedDate) AS InterestAppliedDate,
                //                     Sum(InterestPaidAmount) AS InterestPaidAmount
                //                     from Dt1 
                //INNER JOIN TermDeposit_Master ON Dt1.TD_Id = TermDeposit_Master.TD_Id 
                //                         INNER JOIN Mem_Master ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id 
                //                         INNER JOIN TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id 
                //                     GROUP BY Dt1.TD_Id,Td_No,DepositAmount,ValueDate,MaturityDate,RateOfInterest,TermDeposit_Master.TDScheme_Id,IsCompoundInterest,MaturityAmount,TDScheme_Name,TermDeposit_Master.Mem_Id,memberNo,PerNo,memberName,Token_PersonNo";

                //         var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositOutstanding>(sql
                //             , new NpgsqlParameter("@fromDate", fromDate.Date)
                //             , new NpgsqlParameter("@toDate", toDate.Date)).ToListAsync();
                #endregion
                var fromDateOnly = fromDate.Date;
                var toDateOnly = toDate.Date;
                #region linq
                /// 11001400
                int.TryParse(brCode +"400", out int tdSchemeId);
                var tdSchemeCondition = TDType == "F"
                    ? (Func<int, bool>)(id => id < tdSchemeId)
                    : (id => id > tdSchemeId);

                // First SELECT (Deposit_OB and Interest_OB before fromDate)
                var q1 = from trn in CSISContext.TermDeposit_Trn
                         join master in CSISContext.TermDeposit_Master on trn.TD_Id equals master.TD_Id
                         join scheme in CSISContext.TermDeposit_Schemes on master.TDScheme_Id equals scheme.TDScheme_Id
                         where trn.Trn_Date < fromDateOnly
                               && trn.TD_Delete == false
                               && trn.BrCode == brCode
                               && master.BrCode == brCode
                               && scheme.TDSchemeType == TDType ///     && tdSchemeCondition(master.TDScheme_Id)
                         group trn by trn.TD_Id into g
                         //let depositOB = g.Sum(x => x.DepositReceiptAmount) - g.Sum(x => x.DepositPaidAmount)
                         where g.Sum(x => x.DepositReceiptAmount) - g.Sum(x => x.DepositPaidAmount) >0 ///depositOB > 0
                         select new
                         {
                             TD_Id = g.Key,
                             Deposit_OB = g.Sum(x => x.DepositReceiptAmount) - g.Sum(x => x.DepositPaidAmount), ///depositOB,
                             Interest_OB = g.Sum(x => x.InterestCalculatedAmount) - g.Sum(x => x.InterestPaidAmount),
                             DepositReceiptAmount = 0.0,
                             DepositPaidAmount = 0.0,
                             InterestCalculatedAmount = 0.0,
                             InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                             InterestPaidAmount = 0.0
                         };

                // Second SELECT (Transactions between fromDate and toDate)
                var q2 = from trn in CSISContext.TermDeposit_Trn
                         join master in CSISContext.TermDeposit_Master on trn.TD_Id equals master.TD_Id
                         join scheme in CSISContext.TermDeposit_Schemes on master.TDScheme_Id equals scheme.TDScheme_Id
                         where trn.Trn_Date >= fromDateOnly
                               && trn.Trn_Date <= toDateOnly
                               && trn.TD_Delete == false
                               && trn.BrCode == brCode 
                               && master.BrCode == brCode 
                               && scheme.TDSchemeType == TDType  ////         && tdSchemeCondition(master.TDScheme_Id)
                         group trn by trn.TD_Id into g
                         select new
                         {
                             TD_Id = g.Key,
                             Deposit_OB = 0.0,
                             Interest_OB = 0.0,
                             DepositReceiptAmount = g.Sum(x => x.DepositReceiptAmount),
                             DepositPaidAmount = g.Sum(x => x.DepositPaidAmount),
                             InterestCalculatedAmount = g.Sum(x => x.InterestCalculatedAmount),
                             InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                             InterestPaidAmount = 0.0
                         };

                // Third SELECT (InterestPaidAmount only)
                var q3 = from trn in CSISContext.TermDeposit_Trn
                         join master in CSISContext.TermDeposit_Master on trn.TD_Id equals master.TD_Id
                         join scheme in CSISContext.TermDeposit_Schemes on master.TDScheme_Id equals scheme.TDScheme_Id
                         where trn.InterestPaidAmount > 0
                               && trn.Trn_Date >= fromDateOnly
                               && trn.Trn_Date <= toDate
                               && trn.TD_Delete == false
                               && trn.BrCode == brCode 
                               && master.BrCode == brCode 
                               && scheme.TDSchemeType == TDType     ////  && tdSchemeCondition(master.TDScheme_Id)
                         group trn by trn.TD_Id into g
                         select new
                         {
                             TD_Id = g.Key,
                             Deposit_OB = 0.0,
                             Interest_OB = 0.0,
                             DepositReceiptAmount = 0.0,
                             DepositPaidAmount = 0.0,
                             InterestCalculatedAmount = 0.0,
                             InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                             InterestPaidAmount = g.Sum(x => x.InterestPaidAmount)
                         };

                // Combine all results
                var dt1 = q1.Concat(q2).Concat(q3).ToList();

                // Final join and grouping
                var result = from d in dt1
                             join master in CSISContext.TermDeposit_Master on d.TD_Id equals master.TD_Id
                             join mem in CSISContext.mem_master on master.Mem_Id equals mem.mem_id
                             join scheme in CSISContext.TermDeposit_Schemes on master.TDScheme_Id equals scheme.TDScheme_Id
                             group new { d, master, mem, scheme } by new
                             {
                                 d.TD_Id,
                                 master.TD_No,
                                 master.DepositAmount,
                                 master.ValueDate,
                                 master.MaturityDate,
                                 master.RateOfInterest,
                                 master.TDScheme_Id,
                                 master.IsCompoundInterest,
                                 master.MaturityAmount,
                                 scheme.TDScheme_Name,
                                 master.Mem_Id,
                                 mem.memberno,
                                 mem.perno,
                                 mem.membername,
                                 mem.token_personno
                             } into g
                             select new rptTermDepositOutstanding
                             {
                                 TD_Id = g.Key.TD_Id,
                                 TD_No = g.Key.TD_No,
                                 DepositAmount = g.Key.DepositAmount,
                                 ValueDate = g.Key.ValueDate,
                                 MaturityDate = g.Key.MaturityDate,
                                 RateOfInterest = g.Key.RateOfInterest,
                                 TDScheme_Id = g.Key.TDScheme_Id,
                                 IsCompoundInterest = g.Key.IsCompoundInterest,
                                 MaturityAmount = g.Key.MaturityAmount,
                                 TDScheme_Name = g.Key.TDScheme_Name,
                                 Mem_Id = g.Key.Mem_Id,
                                 MemberNo = g.Key.memberno,
                                 PerNo = g.Key.perno,
                                 MemberName = g.Key.membername,
                                 Token_PersonNo = g.Key.token_personno,
                                 Deposit_OB = g.Sum(x => x.d.Deposit_OB),
                                 Interest_OB = g.Sum(x => x.d.Interest_OB),
                                 DepositReceiptAmount = g.Sum(x => x.d.DepositReceiptAmount),
                                 DepositPaidAmount = g.Sum(x => x.d.DepositPaidAmount),
                                 InterestCalculatedAmount = g.Sum(x => x.d.InterestCalculatedAmount),
                                 InterestAppliedDate = g.Max(x => x.d.InterestAppliedDate),
                                 InterestPaidAmount = g.Sum(x => x.d.InterestPaidAmount)
                             };
                var tdListTmp = await Task.Run(() => result.ToList());

                if (tdListTmp != null) tdList = tdListTmp.ToList();

                //if (result != null) tdList = await result.ToListAsync();

                #endregion 

                //if (tdListTmp != null) tdList = tdListTmp;
            }
            catch (Exception  ex)
            {
                Console.Write("Error in fetching term deposit data schedule " + ex.Message);
                tdList = new();
            }
            return tdList;
        }

        public async Task<List<rptFADividend>> GetDividendFA(DateTime fromDate, DateTime toDate, int trnType, string brCode)
        {
            List<rptFADividend> dividend = new List<rptFADividend>();
            try
            {
                #region query
                //dividendList = await CSISContext.Database.SqlQueryRaw<rptFADividend>(
                //        @"With Dt1
                //        AS
                //        (
                //            SELECT Mem_Trn.Mem_Id, 
                //            Mem_Trn.Led_Id, 
                //            Mem_Master.memberNo,
                //            Mem_Master.PerNo,
                //            Mem_Master.memberName,
                //            Mem_Master.Token_PersonNo,
                //            CAST(Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) AS float) AS Int_OB,
                //            CAST(0 as float) AS IntCalc_Amt,
                //            CAST(0 as float) AS IntPaid_Amt
                //            FROM Mem_Trn INNER JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id 
                //            WHERE Mem_Trn.MemTrn_Delete = 0 And CAST(Mem_Trn.Trn_Date as date)< @fromDate And Mem_Trn.Trn_Type = @trnType
                //            GROUP BY Mem_Trn.Mem_Id, Mem_Trn.Led_Id,Mem_Master.memberNo,Mem_Master.PerNo,Mem_Master.memberName,Mem_Master.Token_PersonNo
                //        UNION
                //            SELECT Mem_Trn.Mem_Id, 
                //            Mem_Trn.Led_Id, 
                //            Mem_Master.memberNo,
                //            Mem_Master.PerNo,
                //            Mem_Master.memberName,
                //            Mem_Master.Token_PersonNo,
                //            CAST(0 as float) AS Int_OB,
                //            CAST(Sum(Mem_Trn.IntCalc_Amt) as float) AS IntCalc_Amt, 
                //            CAST(Sum(Mem_Trn.IntPaid_Amt) as float) AS IntPaid_Amt 
                //            FROM Mem_Trn INNER JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id 
                //            WHERE Mem_Trn.MemTrn_Delete = 0 And CAST(Mem_Trn.Trn_Date as date) BETWEEN @fromDate AND @toDate  And Mem_Trn.Trn_Type = @trnType
                //            GROUP BY Mem_Trn.Mem_Id, Mem_Trn.Led_Id,Mem_Master.memberNo,Mem_Master.PerNo,Mem_Master.memberName,Mem_Master.Token_PersonNo
                //        )
                //            SELECT 
                //            Mem_Id, 
                //            Led_Id, 
                //            memberNo,
                //            PerNo,
                //            memberName,
                //            Token_PersonNo,
                //            Sum(Int_OB) AS Int_OB,
                //            Sum(IntCalc_Amt) AS IntCalc_Amt, 
                //            Sum(IntPaid_Amt) AS IntPaid_Amt 
                //            FROM Dt1
                //            GROUP BY Mem_Id, Led_Id,memberNo,PerNo,memberName,Token_PersonNo"
                //                            , new NpgsqlParameter("@fromDate", fromDate)
                //                            , new NpgsqlParameter("@toDate", toDate)
                //                            , new NpgsqlParameter("@trnType", trnType)).ToListAsync();
                #endregion

                #region linq
                // First query - before fromDate
                var beforeFromDate = from memTrn in CSISContext.Mem_Trn
                                     join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                                     where memTrn.MemTrn_Delete == false
                                           && memTrn.Trn_Date.Date < fromDate.Date
                                           && memTrn.Trn_Type == trnType
                                           && memTrn.BrCode == brCode && memTrn.Voc_Status == "V"
                                           && memMaster.brcode == brCode 
                                     group new { memTrn, memMaster } by new
                                     {
                                         memTrn.Mem_Id,
                                         memTrn.Led_Id,
                                         memMaster.memberno,
                                         memMaster.perno,
                                         memMaster.membername,
                                         memMaster.token_personno
                                     } into g
                                     select new
                                     {
                                         g.Key.Mem_Id,
                                         g.Key.Led_Id,
                                         g.Key.memberno,
                                         g.Key.perno,
                                         g.Key.membername,
                                         g.Key.token_personno,
                                         Int_OB = (float)(g.Sum(x => x.memTrn.IntCalc_Amt) - g.Sum(x => x.memTrn.IntPaid_Amt)),
                                         IntCalc_Amt = 0f,
                                         IntPaid_Amt = 0f
                                     };

                // Second query - between fromDate and toDate
                var betweenDates = from memTrn in CSISContext.Mem_Trn
                                   join memMaster in CSISContext.mem_master on memTrn.Mem_Id equals memMaster.mem_id
                                   where memTrn.MemTrn_Delete == false
                                         && memTrn.Trn_Date.Date >= fromDate.Date
                                         && memTrn.Trn_Date.Date <= toDate.Date
                                         && memTrn.Trn_Type == trnType
                                         && memTrn.BrCode == brCode && memTrn.Voc_Status == "V"
                                         && memMaster.brcode == brCode
                                   group new { memTrn, memMaster } by new
                                   {
                                       memTrn.Mem_Id,
                                       memTrn.Led_Id,
                                       memMaster.memberno,
                                       memMaster.perno,
                                       memMaster.membername,
                                       memMaster.token_personno
                                   } into g
                                   select new
                                   {
                                       g.Key.Mem_Id,
                                       g.Key.Led_Id,
                                       g.Key.memberno,
                                       g.Key.perno,
                                       g.Key.membername,
                                       g.Key.token_personno,
                                       Int_OB = 0f,
                                       IntCalc_Amt = (float)g.Sum(x => x.memTrn.IntCalc_Amt),
                                       IntPaid_Amt = (float)g.Sum(x => x.memTrn.IntPaid_Amt)
                                   };

                // Combine and aggregate
                var dividendList = await beforeFromDate
                    .Union(betweenDates)
                    .GroupBy(x => new
                    {
                        x.Mem_Id,
                        x.Led_Id,
                        x.memberno,
                        x.perno,
                        x.membername,
                        x.token_personno
                    })
                    .Select(g => new rptFADividend
                    {
                        Mem_Id = g.Key.Mem_Id,
                        //Led_Id = g.Key.Led_Id,
                        MemberNo = g.Key.memberno,
                        PerNo = g.Key.perno,
                        MemberName = g.Key.membername,
                        Token_PersonNo = g.Key.token_personno,
                        Int_OB = g.Sum(x => x.Int_OB),
                        IntCalc_Amt = g.Sum(x => x.IntCalc_Amt),
                        IntPaid_Amt = g.Sum(x => x.IntPaid_Amt)
                    })
                    .ToListAsync();
                #endregion 
                foreach (var divi in dividendList)
                {
                    divi.Int_CB = divi.Int_OB + divi.IntCalc_Amt - divi.IntPaid_Amt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return dividend;
        }

        public async Task<List<rptFALedgerTrn>> GetLedgerOutstandingFA(DateTime fromDate, DateTime toDate, decimal yrId, int fnlId, string brCode)
        {
            decimal cashLedId = 0;
            double ledgerbalance = 0, totalReceipts = 0, totalPayments = 0;
            List<rptFALedgerTrn> ledListFnl = new List<rptFALedgerTrn>();
            DateTime FinYearBegin = new DateTime(2024, 04, 01);
            try
            {
                cashLedId = CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
                #region query
                //ledList = await CSISContext.Database.SqlQueryRaw<rptFALedgerTrn>(
                //        @"With Db1
                //        AS
                //        (
                //        SELECT led_id, OB_Amt AS OB, 0 AS voc_rpt , 0 AS voc_pmt  FROM Fin_Ledger_Trn 
                //        WHERE  Yr_Id = @yrId AND  LedgerTrn_Delete = 0
                //        UNION
                //        select Fin_voucher_tr.led_id, Sum(Fin_voucher_tr.voc_pmt) - Sum(Fin_voucher_tr.voc_rpt) AS OB,0 AS voc_rpt, 0 AS voc_pmt
                //        from Fin_voucher_tr inner join Fin_Voucher on Fin_voucher_tr.voc_id =  Fin_Voucher.Voc_Id 
                //        INNER JOIN Fin_Ledger ON Fin_voucher_tr.led_id =  Fin_Ledger.Led_Id 
                //        INNER JOIN Fin_Ledger_Grp  ON Fin_Ledger.Grp_Id  = Fin_Ledger_Grp.Grp_Id
                //        where Fin_Voucher.Voc_Date >= @FinYearBegin AND  Fin_Voucher.Voc_Date <@fromDate 
                //        AND Fin_Ledger_Grp.Fnl_Id in(1,4) AND Fin_voucher_tr.led_id != @cashId
                //        AND Fin_Voucher.Voc_Delete = 0 AND Fin_voucher_tr.FinVocTr_Delete = 0
                //        GROUP BY Fin_voucher_tr.led_id
                //        UNION
                //        select Fin_voucher_tr.led_id,Sum(Fin_voucher_tr.voc_rpt) - Sum(Fin_voucher_tr.voc_pmt)  AS OB,0 AS voc_rpt, 0 AS voc_pmt
                //        from Fin_voucher_tr inner join Fin_Voucher on Fin_voucher_tr.voc_id =  Fin_Voucher.Voc_Id 
                //        INNER JOIN Fin_Ledger ON Fin_voucher_tr.led_id =  Fin_Ledger.Led_Id 
                //        INNER JOIN Fin_Ledger_Grp  ON Fin_Ledger.Grp_Id  = Fin_Ledger_Grp.Grp_Id
                //        where Fin_Voucher.Voc_Date >= @FinYearBegin AND  Fin_Voucher.Voc_Date <@fromDate 
                //        AND Fin_Ledger_Grp.Fnl_Id in(2,3) AND Fin_voucher_tr.led_id != @cashId
                //        AND Fin_Voucher.Voc_Delete = 0 AND Fin_voucher_tr.FinVocTr_Delete = 0
                //        GROUP BY Fin_voucher_tr.led_id
                //        UNION
                //        select Fin_voucher_tr.led_id,Sum(Fin_voucher_tr.voc_rpt) - Sum(Fin_voucher_tr.voc_pmt)  AS OB,0 AS voc_rpt, 0 AS voc_pmt
                //        from Fin_voucher_tr inner join Fin_Voucher on Fin_voucher_tr.voc_id =  Fin_Voucher.Voc_Id 
                //        INNER JOIN Fin_Ledger ON Fin_voucher_tr.led_id =  Fin_Ledger.Led_Id 
                //        INNER JOIN Fin_Ledger_Grp  ON Fin_Ledger.Grp_Id  = Fin_Ledger_Grp.Grp_Id
                //        where Fin_Voucher.Voc_Date >= @FinYearBegin AND  Fin_Voucher.Voc_Date <@fromDate 
                //        AND  Fin_voucher_tr.led_id = @cashId
                //        AND Fin_Voucher.Voc_Delete = 0 AND Fin_voucher_tr.FinVocTr_Delete = 0
                //        GROUP BY Fin_voucher_tr.led_id
                //        UNION
                //        select Fin_voucher_tr.led_id, 0 AS OB,Sum(Fin_voucher_tr.voc_rpt) AS rpt, Sum(Fin_voucher_tr.voc_pmt)  AS pmt
                //        from Fin_voucher_tr inner join Fin_Voucher on Fin_voucher_tr.voc_id =  Fin_Voucher.Voc_Id 
                //        INNER JOIN Fin_Ledger ON Fin_voucher_tr.led_id =  Fin_Ledger.Led_Id 
                //        INNER JOIN Fin_Ledger_Grp  ON Fin_Ledger.Grp_Id  = Fin_Ledger_Grp.Grp_Id
                //        where Fin_Voucher.Voc_Date >= @fromDate AND  Fin_Voucher.Voc_Date <= @toDate
                //        AND Fin_Voucher.Voc_Delete = 0 AND Fin_voucher_tr.FinVocTr_Delete = 0
                //        GROUP BY Fin_voucher_tr.led_id
                //        )
                //        SELECT Db1.Led_Id, Fin_Ledger.Led_SlNo, Fin_Ledger.Led_Name, Fin_Ledger_Grp.Grp_SlNo,Fin_Ledger_Grp.Grp_Name, 
                //        Fin_Ledger_Grp.Fnl_Id, Fin_Ledger_Fnl.Fnl_Name,
                //        Sum(OB) AS OB_Amt, Sum(voc_rpt) AS Tot_Rpt_Amt, Sum(voc_pmt) AS Tot_Pmt_Amt  FROM Db1
                //        INNER JOIN Fin_Ledger ON Db1.Led_Id = Fin_Ledger.led_id 
                //        INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id 
                //        INNER JOIN Fin_Ledger_Fnl ON Fin_Ledger_Grp.Fnl_Id =  Fin_Ledger_Fnl.Fnl_Id 
                //        GROUP BY Db1.Led_Id,Fin_Ledger.Led_SlNo, Fin_Ledger.Led_Name, Fin_Ledger_Grp.Grp_SlNo,Fin_Ledger_Grp.Grp_Name, 
                //        Fin_Ledger_Grp.Fnl_Id, Fin_Ledger_Fnl.Fnl_Name"
                //        , new NpgsqlParameter("@cashId", cashLedId)
                //        , new NpgsqlParameter("@FinYearBegin", yrId - 1)
                //        , new NpgsqlParameter("@fromDate", fromDate)
                //        , new NpgsqlParameter("@toDate", toDate)
                //        , new NpgsqlParameter("@yrId", yrId)).ToListAsync();
                #endregion

                #region linq first commented
                //// First, let's define the individual queries that make up the CTE

                //// Opening balance query
                //var openingBalances = CSISContext.Set<Fin_Ledger_Trn>()
                //    .Where(lt => lt.Yr_Id == yrId && lt.LedgerTrn_Delete == false && lt.BrCode == brCode )
                //    .Select(lt => new {
                //        led_id = lt.Led_Id,
                //        OB = lt.OB_Amt,
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// First union part - for ledger groups 1 and 4
                //var union1 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= FinYearBegin &&
                //                j.v.Voc_Date < fromDate &&
                //                (j.g.Fnl_Id == 1 || j.g.Fnl_Id == 4) &&
                //                j.vt.Led_Id != cashLedId &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new {
                //        led_id = g.Key,
                //        OB = g.Sum(x => x.vt.Voc_Pmt) - g.Sum(x => x.vt.Voc_Rpt),
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// Second union part - for ledger groups 2 and 3
                //var union2 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= FinYearBegin &&
                //                j.v.Voc_Date < fromDate &&
                //                (j.g.Fnl_Id == 2 || j.g.Fnl_Id == 3) &&
                //                j.vt.Led_Id != cashLedId &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new {
                //        led_id = g.Key,
                //        OB = g.Sum(x => x.vt.Voc_Rpt) - g.Sum(x => x.vt.Voc_Pmt),
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// Third union part - for cash ledger
                //var union3 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= FinYearBegin &&
                //                j.v.Voc_Date < fromDate &&
                //                j.vt.Led_Id == cashLedId &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new {
                //        led_id = g.Key,
                //        OB = g.Sum(x => x.vt.Voc_Rpt) - g.Sum(x => x.vt.Voc_Pmt),
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// Fourth union part - transactions within date range
                //var union4 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= fromDate &&
                //                j.v.Voc_Date <= toDate &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new {
                //        led_id = g.Key,
                //        OB = 0,
                //        voc_rpt = g.Sum(x => x.vt.Voc_Rpt),
                //        voc_pmt = g.Sum(x => x.vt.Voc_Pmt)
                //    });

                //// Combine all union parts to create the CTE equivalent
                //var db1 = openingBalances
                //    .Union(union1)
                //    .Union(union2)
                //    .Union(union3)
                //    .Union(union4);

                //// Now build the final query that joins with the CTE result
                //var ledList = await (
                //    from d in db1
                //    join l in CSISContext.Set<Fin_Ledger>() on d.led_id equals l.Led_Id
                //    join g in CSISContext.Set<Fin_Ledger_Grp>() on l.Grp_Id equals g.Grp_Id
                //    join f in CSISContext.Set<Fin_Ledger_Fnl>() on g.Fnl_Id equals f.Fnl_Id
                //    group new { d, l, g, f } by new
                //    {
                //        d.led_id,
                //        l.Led_SlNo,
                //        l.Led_Name,
                //        g.Grp_SlNo,
                //        g.Grp_Name,
                //        g.Fnl_Id,
                //        f.Fnl_Name
                //    } into grouped
                //    select new rptFALedgerTrn
                //    {
                //        Led_Id = grouped.Key.led_id,
                //        Led_SlNo = grouped.Key.Led_SlNo,
                //        Led_Name = grouped.Key.Led_Name,
                //        Grp_SlNo = grouped.Key.Grp_SlNo,
                //        Grp_Name = grouped.Key.Grp_Name,
                //        Fnl_Id = grouped.Key.Fnl_Id,
                //        Fnl_Name = grouped.Key.Fnl_Name,
                //        OB_Amt = grouped.Sum(x => x.d.OB),
                //        Tot_Rpt_Amt = grouped.Sum(x => x.d.voc_rpt),
                //        Tot_Pmt_Amt = grouped.Sum(x => x.d.voc_pmt)
                //    }).ToListAsync();
                #endregion

                #region linq second commented
                //// First, let's define the individual queries that make up the CTE

                //// Opening balance query
                //var openingBalances = CSISContext.Set<Fin_Ledger_Trn>()
                //    .Where(lt => lt.Yr_Id == yrId && lt.LedgerTrn_Delete == false && lt.BrCode == brCode )
                //    .Select(lt => new
                //    {
                //        led_id = lt.Led_Id,
                //        OB = lt.OB_Amt,
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// First union part - for ledger groups 1 and 4
                //var union1 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= fromDate  &&
                //                j.v.Voc_Date <= toDate  &&
                //                (j.g.Fnl_Id == 1 || j.g.Fnl_Id == 4) &&
                //                j.vt.Led_Id != cashLedId &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false &&
                //                j.v.BrCode == brCode && 
                //                j.vt.BrCode == brCode && 
                //                j.l.BrCode == brCode )
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new
                //    {
                //        Led_Id = g.Key,
                //        OB = g.Sum(x => x.vt.Voc_Pmt) - g.Sum(x => x.vt.Voc_Rpt),
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// Second union part - for ledger groups 2 and 3
                //var union2 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= fromDate  &&
                //                j.v.Voc_Date <= toDate  &&
                //                (j.g.Fnl_Id == 2 || j.g.Fnl_Id == 3) &&
                //                j.vt.Led_Id != cashLedId &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false &&
                //                j.vt.BrCode == brCode && 
                //                j.v.BrCode == brCode && 
                //                j.l.BrCode == brCode)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new
                //    {
                //        led_id = g.Key,
                //        OB = g.Sum(x => x.vt.Voc_Rpt) - g.Sum(x => x.vt.Voc_Pmt),
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// Third union part - for cash ledger
                //var union3 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= fromDate &&
                //                j.v.Voc_Date <= toDate  &&
                //                j.vt.Led_Id == cashLedId &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false &&
                //                j.vt.BrCode == brCode && 
                //                j.v.BrCode == brCode && 
                //                j.l.BrCode == brCode)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new
                //    {
                //        Led_Id = g.Key,
                //        OB = g.Sum(x => x.vt.Voc_Rpt) - g.Sum(x => x.vt.Voc_Pmt),
                //        voc_rpt = 0,
                //        voc_pmt = 0
                //    });

                //// Fourth union part - transactions within date range
                //var union4 = CSISContext.Set<Fin_Voucher_Trn>()
                //    .Join(CSISContext.Set<Fin_Voucher>(),
                //          vt => vt.Voc_Id,
                //          v => v.Voc_Id,
                //          (vt, v) => new { vt, v })
                //    .Join(CSISContext.Set<Fin_Ledger>(),
                //          j => j.vt.Led_Id,
                //          l => l.Led_Id,
                //          (j, l) => new { j.vt, j.v, l })
                //    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                //          j => j.l.Grp_Id,
                //          g => g.Grp_Id,
                //          (j, g) => new { j.vt, j.v, j.l, g })
                //    .Where(j => j.v.Voc_Date >= fromDate &&
                //                j.v.Voc_Date <= toDate &&
                //                j.v.Voc_Delete == false &&
                //                j.vt.FinVocTr_Delete == false &&
                //                j.vt.BrCode == brCode && 
                //                j.v.BrCode == brCode && 
                //                j.l.BrCode == brCode)
                //    .GroupBy(j => j.vt.Led_Id)
                //    .Select(g => new
                //    {
                //        led_id = g.Key,
                //        OB = 0,
                //        voc_rpt = g.Sum(x => x.vt.Voc_Rpt),
                //        voc_pmt = g.Sum(x => x.vt.Voc_Pmt)
                //    });

                //// Create a temporary type that matches our anonymous type structure


                //// Combine all union parts to create the CTE equivalent
                //// Use explicit typing to avoid null reference issues
                //var db1Query = openingBalances.AsEnumerable()
                //    .Select(x => new TempDb1
                //    {
                //        Led_Id = x.led_id,
                //        OB = x.OB,
                //        Voc_Rpt = x.voc_rpt,
                //        Voc_Pmt = x.voc_pmt
                //    })
                //    .Concat(union1.AsEnumerable().Select(x => new TempDb1
                //    {
                //        Led_Id = x.Led_Id,
                //        OB = x.OB,
                //        Voc_Rpt = x.voc_rpt,
                //        Voc_Pmt = x.voc_pmt
                //    }))
                //    .Concat(union2.AsEnumerable().Select(x => new TempDb1
                //    {
                //        Led_Id = x.led_id,
                //        OB = x.OB,
                //        Voc_Rpt = x.voc_rpt,
                //        Voc_Pmt = x.voc_pmt
                //    }))
                //    .Concat(union3.AsEnumerable().Select(x => new TempDb1
                //    {
                //        Led_Id = x.Led_Id,
                //        OB = x.OB,
                //        Voc_Rpt = x.voc_rpt,
                //        Voc_Pmt = x.voc_pmt
                //    }))
                //    .Concat(union4.AsEnumerable().Select(x => new TempDb1
                //    {
                //        Led_Id = x.led_id,
                //        OB = x.OB,
                //        Voc_Rpt = x.voc_rpt,
                //        Voc_Pmt = x.voc_pmt
                //    }));

                //// Create a DbSet from our combined query to use in the final query
                //var db1 = db1Query.AsQueryable();

                //// Now build the final query that joins with the CTE result
                //var ledList = await (
                //    from d in db1
                //    join l in CSISContext.Set<Fin_Ledger>() on d.Led_Id equals l.Led_Id
                //    join g in CSISContext.Set<Fin_Ledger_Grp>() on l.Grp_Id equals g.Grp_Id
                //    join f in CSISContext.Set<Fin_Ledger_Fnl>() on g.Fnl_Id equals f.Fnl_Id
                //    group new { d, l, g, f } by new
                //    {
                //        d.Led_Id,
                //        l.Led_SlNo,
                //        l.Led_Name,
                //        g.Grp_SlNo,
                //        g.Grp_Name,
                //        g.Fnl_Id,
                //        f.Fnl_Name
                //    } into grouped
                //    select new rptFALedgerTrn
                //    {
                //        Led_Id = grouped.Key.Led_Id,
                //        Led_SlNo = grouped.Key.Led_SlNo,
                //        Led_Name = grouped.Key.Led_Name,
                //        Grp_SlNo = grouped.Key.Grp_SlNo,
                //        Grp_Name = grouped.Key.Grp_Name,
                //        Fnl_Id = grouped.Key.Fnl_Id,
                //        Fnl_Name = grouped.Key.Fnl_Name,
                //        OB_Amt = grouped.Sum(x => x.d.OB),
                //        Tot_Rpt_Amt = grouped.Sum(x => x.d.Voc_Rpt),
                //        Tot_Pmt_Amt = grouped.Sum(x => x.d.Voc_Pmt)
                //    }).ToListAsync();
                #endregion

                #region linq third
                // Opening balances query
                var openingBalances = CSISContext.Set<Fin_Ledger_Trn>()
                    .Where(lt => lt.Yr_Id == yrId && lt.LedgerTrn_Delete == false && lt.BrCode == brCode)
                    .Select(lt => new
                    {
                        Led_Id = lt.Led_Id,
                        OB = lt.OB_Amt,
                        Voc_Rpt = 0.0,
                        Voc_Pmt = 0.0
                    });

                // First union part - for ledger groups 1 and 4
                var union1 = CSISContext.Set<Fin_Voucher_Trn>()
                    .Join(CSISContext.Set<Fin_Voucher>(),
                          vt => vt.Voc_Id,
                          v => v.Voc_Id,
                          (vt, v) => new { vt, v })
                    .Join(CSISContext.Set<Fin_Ledger>(),
                          j => j.vt.Led_Id,
                          l => l.Led_Id,
                          (j, l) => new { j.vt, j.v, l })
                    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                          j => j.l.Grp_Id,
                          g => g.Grp_Id,
                          (j, g) => new { j.vt, j.v, j.l, g })
                    .Where(j => j.v.Voc_Date >= fromDate &&
                                j.v.Voc_Date <= toDate &&
                                (j.g.Fnl_Id == 1 || j.g.Fnl_Id == 4) &&
                                j.vt.Led_Id != cashLedId &&
                                j.v.Voc_Delete == false &&
                                j.vt.FinVocTr_Delete == false &&
                                j.v.BrCode == brCode &&
                                j.vt.BrCode == brCode &&
                                j.l.BrCode == brCode)
                    .GroupBy(j => j.vt.Led_Id)
                    .Select(g => new
                    {
                        Led_Id = g.Key,
                        OB = g.Sum(x => x.vt.Voc_Pmt) - g.Sum(x => x.vt.Voc_Rpt),
                        Voc_Rpt = 0.0,
                        Voc_Pmt = 0.0
                    });

                // Second union part - for ledger groups 2 and 3
                var union2 = CSISContext.Set<Fin_Voucher_Trn>()
                    .Join(CSISContext.Set<Fin_Voucher>(),
                          vt => vt.Voc_Id,
                          v => v.Voc_Id,
                          (vt, v) => new { vt, v })
                    .Join(CSISContext.Set<Fin_Ledger>(),
                          j => j.vt.Led_Id,
                          l => l.Led_Id,
                          (j, l) => new { j.vt, j.v, l })
                    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                          j => j.l.Grp_Id,
                          g => g.Grp_Id,
                          (j, g) => new { j.vt, j.v, j.l, g })
                    .Where(j => j.v.Voc_Date >= fromDate &&
                                j.v.Voc_Date <= toDate &&
                                (j.g.Fnl_Id == 2 || j.g.Fnl_Id == 3) &&
                                j.vt.Led_Id != cashLedId &&
                                j.v.Voc_Delete == false &&
                                j.vt.FinVocTr_Delete == false &&
                                j.vt.BrCode == brCode &&
                                j.v.BrCode == brCode &&
                                j.l.BrCode == brCode)
                    .GroupBy(j => j.vt.Led_Id)
                    .Select(g => new
                    {
                        Led_Id = g.Key,
                        OB = g.Sum(x => x.vt.Voc_Rpt) - g.Sum(x => x.vt.Voc_Pmt),
                        Voc_Rpt = 0.0,
                        Voc_Pmt = 0.0
                    });

                // Third union part - for cash ledger
                var union3 = CSISContext.Set<Fin_Voucher_Trn>()
                    .Join(CSISContext.Set<Fin_Voucher>(),
                          vt => vt.Voc_Id,
                          v => v.Voc_Id,
                          (vt, v) => new { vt, v })
                    .Join(CSISContext.Set<Fin_Ledger>(),
                          j => j.vt.Led_Id,
                          l => l.Led_Id,
                          (j, l) => new { j.vt, j.v, l })
                    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                          j => j.l.Grp_Id,
                          g => g.Grp_Id,
                          (j, g) => new { j.vt, j.v, j.l, g })
                    .Where(j => j.v.Voc_Date >= fromDate &&
                                j.v.Voc_Date <= toDate &&
                                j.vt.Led_Id == cashLedId &&
                                j.v.Voc_Delete == false &&
                                j.vt.FinVocTr_Delete == false &&
                                j.vt.BrCode == brCode &&
                                j.v.BrCode == brCode &&
                                j.l.BrCode == brCode)
                    .GroupBy(j => j.vt.Led_Id)
                    .Select(g => new
                    {
                        Led_Id = g.Key,
                        OB = g.Sum(x => x.vt.Voc_Rpt) - g.Sum(x => x.vt.Voc_Pmt),
                        Voc_Rpt = 0.0,
                        Voc_Pmt = 0.0
                    });

                // Fourth union part - transactions within date range
                var union4 = CSISContext.Set<Fin_Voucher_Trn>()
                    .Join(CSISContext.Set<Fin_Voucher>(),
                          vt => vt.Voc_Id,
                          v => v.Voc_Id,
                          (vt, v) => new { vt, v })
                    .Join(CSISContext.Set<Fin_Ledger>(),
                          j => j.vt.Led_Id,
                          l => l.Led_Id,
                          (j, l) => new { j.vt, j.v, l })
                    .Join(CSISContext.Set<Fin_Ledger_Grp>(),
                          j => j.l.Grp_Id,
                          g => g.Grp_Id,
                          (j, g) => new { j.vt, j.v, j.l, g })
                    .Where(j => j.v.Voc_Date >= fromDate &&
                                j.v.Voc_Date <= toDate &&
                                j.v.Voc_Delete == false &&
                                j.vt.FinVocTr_Delete == false &&
                                j.vt.BrCode == brCode &&
                                j.v.BrCode == brCode &&
                                j.l.BrCode == brCode)
                    .GroupBy(j => j.vt.Led_Id)
                    .Select(g => new
                    {
                        Led_Id = g.Key,
                        OB = 0.0,
                        Voc_Rpt = g.Sum(x => x.vt.Voc_Rpt),
                        Voc_Pmt = g.Sum(x => x.vt.Voc_Pmt)
                    });

                // Combine all parts using Union (keeps everything as IQueryable)
                var combinedQuery = openingBalances
                    .Union(union1)
                    .Union(union2)
                    .Union(union3)
                    .Union(union4);

                // Now build the final query that joins with the combined result
                var ledList = await (
                    from d in combinedQuery
                    join l in CSISContext.Set<Fin_Ledger>() on d.Led_Id equals l.Led_Id
                    join g in CSISContext.Set<Fin_Ledger_Grp>() on l.Grp_Id equals g.Grp_Id
                    join f in CSISContext.Set<Fin_Ledger_Fnl>() on g.Fnl_Id equals f.Fnl_Id
                    group new { d, l, g, f } by new
                    {
                        d.Led_Id,
                        l.Led_SlNo,
                        l.Led_Name,
                        g.Grp_SlNo,
                        g.Grp_Name,
                        g.Fnl_Id,
                        f.Fnl_Name
                    } into grouped
                    select new rptFALedgerTrn
                    {
                        Led_Id = grouped.Key.Led_Id,
                        Led_SlNo = grouped.Key.Led_SlNo,
                        Led_Name = grouped.Key.Led_Name,
                        Grp_SlNo = grouped.Key.Grp_SlNo,
                        Grp_Name = grouped.Key.Grp_Name,
                        Fnl_Id = grouped.Key.Fnl_Id,
                        Fnl_Name = grouped.Key.Fnl_Name,
                        OB_Amt = grouped.Sum(x => x.d.OB),
                        Tot_Rpt_Amt = grouped.Sum(x => x.d.Voc_Rpt),
                        Tot_Pmt_Amt = grouped.Sum(x => x.d.Voc_Pmt)
                    }).ToListAsync();
                #endregion 

                foreach (var led in ledList)
                {
                    ledgerbalance = 0;
                    totalReceipts = led.Tot_Rpt_Amt;
                    totalPayments = led.Tot_Pmt_Amt;
                    switch (led.Fnl_Id)
                    {
                        case 1:
                        case 4:
                            if (led.Led_Id == cashLedId)
                                ledgerbalance = led.OB_Amt + led.Tot_Rpt_Amt - led.Tot_Pmt_Amt;
                            else
                                ledgerbalance = led.OB_Amt + led.Tot_Pmt_Amt - led.Tot_Rpt_Amt;
                            break;
                        case 2:
                        case 3:
                            ledgerbalance = led.OB_Amt + led.Tot_Rpt_Amt - led.Tot_Pmt_Amt;
                            break;
                    }
                    led.CB_Amt = ledgerbalance;
                }
                if (ledList != null) ledListFnl = ledList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching ledger shedule data " + ex.Message);
                ledListFnl = new();
            }
            return ledListFnl;
        }

        public async Task<List<rptFAFDIntPaidPayable>> GetFDInterestPaidAndPayable(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptFAFDIntPaidPayable> tdList = new List<rptFAFDIntPaidPayable>();
            double intCalc = 0;
            DateTime maxIntCalcDate = DateTime.MinValue;
            try
            {
                #region sql server query
                //tdList = await CSISContext.Database.SqlQueryRaw<rptFAFDIntPaidPayable>(
                //            @"With Db1
                //            AS
                //            (
                //            SELECT TermDeposit_Trn.TD_Id,Max(TermDeposit_Trn.InterestAppliedDate) AS IntAppliedDate, CAST(SUM(TermDeposit_Trn.InterestPaidAmount) AS Float) AS IntPaidBeforeBegining,CAST(0 AS Float) AS IntPaidDuringPeriod , CAST(0 AS Float) AS IntPayable, Sum(DepositPaidAmount) AS DepositPaidAmount
                //            FROM    TermDeposit_Trn 
                //            WHERE  (TermDeposit_Trn.Trn_Date < CONVERT(DATETIME, @fromDate, 102)) AND (TermDeposit_Trn.TD_Delete = 0) 
                //            GROUP BY TermDeposit_Trn.TD_Id 
                //            HAVING  (SUM(TermDeposit_Trn.DepositReceiptAmount) -  SUM(TermDeposit_Trn.DepositPaidAmount ) >0)
                //            UNION
                //            SELECT TermDeposit_Trn.TD_Id,Max(TermDeposit_Trn.InterestAppliedDate) AS IntAppliedDate, CAST(0 AS Float) AS IntPaidBeforeBegining, CAST(SUM(TermDeposit_Trn.InterestPaidAmount) AS Float) AS IntPaidDuringPeriod, CAST(0 AS Float) AS IntPayable, Sum(DepositPaidAmount) AS DepositPaidAmount
                //            FROM    TermDeposit_Trn 
                //            WHERE (TermDeposit_Trn.InterestPaidAmount >0) AND (TermDeposit_Trn.Trn_Date BETWEEN CONVERT(DATETIME, @fromDate, 102) AND  CONVERT(DATETIME, @toDate, 102)) AND (TermDeposit_Trn.TD_Delete = 0) 
                //            GROUP BY TermDeposit_Trn.TD_Id 
                //            )
                //            SELECT Db1.TD_Id, Mem_Master.memberNo, Mem_Master.memberName,TermDeposit_Master.TDH_Name,TermDeposit_Master.TD_No,  TermDeposit_Master.DepositAmount, TermDeposit_Master.ValueDate, 
                //             TermDeposit_Master.MaturityDate, TermDeposit_Master.RateOfInterest, TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays, 
                //             Max(Db1.IntAppliedDate) AS InterestAppliedDate,Sum(Db1.IntPaidBeforeBegining) AS IntPaidBeforePeriod, sum(Db1.IntPaidDuringPeriod) AS IntPaidDuringPeriod, sum(Db1.IntPayable) AS IntPayable, Sum(Db1.DepositPaidAmount) AS DepositPaidAmount
                //                FROM Db1 INNER JOIN TermDeposit_Master ON Db1.TD_Id = TermDeposit_Master.TD_Id 
                //             INNER JOIN Mem_Master ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id 
                //             WHERE TermDeposit_Master.TDScheme_Id <40000
                //             GROUP BY Db1.TD_Id, TermDeposit_Master.TD_No, Mem_Master.memberNo, Mem_Master.memberName, TermDeposit_Master.TDH_Name,TermDeposit_Master.DepositAmount,TermDeposit_Master.ValueDate,TermDeposit_Master.MaturityDate,
                //                  TermDeposit_Master.RateOfInterest, TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays
                //             ORDER BY  TermDeposit_Master.TD_No"
                //            , new NpgsqlParameter("@fromDate", fromDate)
                //            , new NpgsqlParameter("@toDate", toDate)).ToListAsync();
                #endregion

                #region postgresql query
                tdList = await CSISContext.Database.SqlQueryRaw<rptFAFDIntPaidPayable>
                (
                @"WITH Db1 AS(
                    SELECT
                        TermDeposit_Trn.TD_Id,
                        MAX(TermDeposit_Trn.InterestAppliedDate) AS IntAppliedDate,
                        SUM(TermDeposit_Trn.InterestPaidAmount)::DOUBLE PRECISION AS IntPaidBeforeBegining,
                        0::DOUBLE PRECISION AS IntPaidDuringPeriod,
                        0::DOUBLE PRECISION AS IntPayable,
                        SUM(DepositPaidAmount) AS DepositPaidAmount
                    FROM TermDeposit_Trn
                    WHERE TermDeposit_Trn.Trn_Date < TO_DATE(@fromDate, 'YYYY.MM.DD')
                      AND TermDeposit_Trn.TD_Delete = false AND TermDeposit_Trn.brcode = @brCode AND TermDeposit_Trn.voc_status = 'V'
                    GROUP BY TermDeposit_Trn.TD_Id
                    HAVING SUM(TermDeposit_Trn.DepositReceiptAmount) - SUM(TermDeposit_Trn.DepositPaidAmount) > 0

                UNION

                    SELECT
                        TermDeposit_Trn.TD_Id,
                        MAX(TermDeposit_Trn.InterestAppliedDate) AS IntAppliedDate,
                        0::DOUBLE PRECISION AS IntPaidBeforeBegining,
                        SUM(TermDeposit_Trn.InterestPaidAmount)::DOUBLE PRECISION AS IntPaidDuringPeriod,
                        0::DOUBLE PRECISION AS IntPayable,
                        SUM(DepositPaidAmount) AS DepositPaidAmount
                    FROM TermDeposit_Trn
                    WHERE TermDeposit_Trn.InterestPaidAmount > 0
                      AND TermDeposit_Trn.Trn_Date BETWEEN TO_DATE(@fromDate, 'YYYY.MM.DD') AND TO_DATE(@toDate, 'YYYY.MM.DD')
                      AND TermDeposit_Trn.TD_Delete = false AND TermDeposit_Trn.brCode = @brCode  AND TermDeposit_Trn.voc_status = 'V'
                    GROUP BY TermDeposit_Trn.TD_Id
                )

                    SELECT
                        Db1.TD_Id,
                        Mem_Master.memberNo,
                        Mem_Master.memberName,
                        TermDeposit_Master.TDH_Name,
                        TermDeposit_Master.TD_No,
                        TermDeposit_Master.DepositAmount,
                        TermDeposit_Master.ValueDate,
                        TermDeposit_Master.MaturityDate,
                        TermDeposit_Master.RateOfInterest,
                        TermDeposit_Master.PeriodInMonths,
                        TermDeposit_Master.PeriodInDays,
                        MAX(Db1.IntAppliedDate) AS InterestAppliedDate,
                        SUM(Db1.IntPaidBeforeBegining) AS IntPaidBeforePeriod,
                        SUM(Db1.IntPaidDuringPeriod) AS IntPaidDuringPeriod,
                        SUM(Db1.IntPayable) AS IntPayable,
                        SUM(Db1.DepositPaidAmount) AS DepositPaidAmount
                    FROM Db1
                    INNER JOIN TermDeposit_Master ON Db1.TD_Id = TermDeposit_Master.TD_Id
                    INNER JOIN Mem_Master ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
                    WHERE TermDeposit_Master.TDScheme_Id < 40000 
                    GROUP BY
                        Db1.TD_Id,
                        TermDeposit_Master.TD_No,
                        Mem_Master.memberNo,
                        Mem_Master.memberName,
                        TermDeposit_Master.TDH_Name,
                        TermDeposit_Master.DepositAmount,
                        TermDeposit_Master.ValueDate,
                        TermDeposit_Master.MaturityDate,
                        TermDeposit_Master.RateOfInterest,
                        TermDeposit_Master.PeriodInMonths,
                        TermDeposit_Master.PeriodInDays
                    ORDER BY TermDeposit_Master.TD_No"
                    , new NpgsqlParameter("@fromDate", fromDate)
                    , new NpgsqlParameter("@toDate", toDate)
                    , new NpgsqlParameter("@brCode",brCode)).ToListAsync();

                #endregion 

                //tdList = tdList.Where(x => x.Td_Id == 4286).ToList();
                foreach (var td in tdList)
                {
                    if (td.InterestAppliedDate != null)
                        maxIntCalcDate = (DateTime)td.InterestAppliedDate;
                    else
                        maxIntCalcDate = td.ValueDate;
                    if (td.DepositAmount - td.DepositPaidAmount > 0)
                    {
                        if (Utilities.GetNoOfDays(toDate, td.MaturityDate) > 0)
                        {
                            intCalc = Utilities.Calculate_Interest(td.DepositAmount, td.RateOfInterest, Utilities.GetNoOfDays(td.MaturityDate, maxIntCalcDate));
                        }
                        else
                        {
                            intCalc = Utilities.Calculate_Interest(td.DepositAmount, td.RateOfInterest, Utilities.GetNoOfDays(toDate, maxIntCalcDate) + 1);
                        }
                        //td.IntPayable += intCalc;
                    }
                    else
                        intCalc = 0;
                    td.IntPayable += intCalc;
                }

            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<List<rptFALoanOutstanding2>> GetLoanOutstandingForFA_HL_WithRceiptDates(DateTime fromDate, DateTime toDate, int loanType, string brCode)
        {
            List<rptFALoanOutstanding2> loanList = new List<rptFALoanOutstanding2>();
            DateTime? lastPrlCollDate = null;
            DateTime? lastIntCollDate = null;
            try
            {
                #region sql server query
       //         loanList = await CSISContext.Database.SqlQueryRaw<rptFALoanOutstanding2>(
       //                 @"With Dt1
       //                 AS
       //                 (
       //                     SELECT Loan_Trn.Loan_id, 
       //                     Loan_Master.Loan_No,
       //                     Loan_Master.Mem_Id,
       //                     Loan_Master.roi,
       //                     Loan_Master.San_date,
       //                     Mem_Master.memberNo,
       //                     Mem_Master.PerNo,
       //                     Mem_Master.memberName,
       //                     Mem_Master.Token_PersonNo,
       //                     Loan_Master.Loan_Type,
       //                     Loan_Master.Scheme_Id,
       //                     Loan_Schemes.Scheme_Name,
       //                     Sum(Loan_Trn.Disb_Amt)  - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OS,
       //                     Sum(Loan_Trn.Disb_Amt)  - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OB,
       //                     Sum(Loan_Trn.IntCalc_Amt)  - Sum(Loan_Trn.IntColl_Amt) AS Int_OB,
       //                     Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) AS PI_OB,
       //                     ISNULL(Sum(Loan_Trn.IODCalc_Amt) - Sum(Loan_Trn.IODColl_Amt),0) AS IOD_OB,
       //                     CAST(NULL AS datetime) AS LastPrlCollDate,
							//CAST(NULL AS datetime) As LastIntCollDate,
       //                     CAST(0 AS float) AS Prl_Sched,
       //                     Sum(Loan_Trn.Disb_Amt) AS Disb_Amt,
       //                     CAST(0 AS float) AS PrlPayment, 
       //                     CAST(0 AS float) AS PrlColl_Amt,
       //                     CAST(0 AS float) AS IntCalc_Amt,
       //                     CAST(0 AS float) AS IntColl_Amt,
       //                     CAST(0 AS float) AS PICalc_Amt,
       //                     CAST(0 AS float) AS PIColl_Amt,
       //                     CAST(0 AS float) AS IODCalc_Amt,
       //                     CAST(0 AS float) AS IODColl_Amt,
       //                     Sum(Loan_Trn.Prl_Sched) - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OD,
       //                     CAST(0 AS float) AS Int_Bal,
       //                     CAST(0 AS float) AS PI_Bal,
       //                     CAST(0 AS float) AS IOD_Bal
       //                     From Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id 
       //                     INNER JOIN Mem_Master ON Loan_Master.Mem_Id = Mem_Master.mem_Id 
       //                     INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id 
       //                     WHERE Loan_Trn.TrnTr_Delete = 0 AND CAST(Loan_Trn.Trn_Date as date) <@fromDate
       //                     GROUP BY Loan_Trn.Loan_id,Loan_Master.Loan_Type,Loan_Master.Loan_No,Loan_Master.Mem_Id,Loan_Master.roi,Loan_Master.San_date,Mem_Master.memberNo,Mem_Master.PerNo,Mem_Master.memberName,Mem_Master.Token_PersonNo,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name  
       //                     HAVING Loan_Master.Loan_Type = @loanType AND (Sum(Loan_Trn.Disb_Amt) - Sum(Loan_Trn.PrlColl_Amt) >0 
       //                     OR Sum(Loan_Trn.IntCalc_Amt) - Sum(Loan_Trn.IntColl_Amt) >0 
       //                     OR Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) >0 
       //                     OR Sum(Loan_Trn.IODCalc_Amt) - Sum(Loan_Trn.IODColl_Amt) >0)
       //                 UNION
       //                     SELECT Loan_Trn.Loan_id, 
       //                     Loan_Master.Loan_No,
       //                     Loan_Master.Mem_Id,
       //                     Loan_Master.roi,
       //                     Loan_Master.San_date,
       //                     Mem_Master.memberNo,
       //                     Mem_Master.PerNo,
       //                     Mem_Master.memberName,
       //                     Mem_Master.Token_PersonNo,
       //                     Loan_Master.Loan_Type,
       //                     Loan_Master.Scheme_Id,
       //                     Loan_Schemes.Scheme_Name,
       //                     Sum(Loan_Trn.Disb_Amt)  - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OS,
       //                     CAST(0 as float) AS Prl_OB,
       //                     CAST(0 as float) AS Int_OB,
       //                     CAST(0 as float) AS PI_OB,
       //                     CAST(0 as float) AS IOD_OB,
       //                     CAST(NULL AS datetime) AS LastPrlCollDate,
							//CAST(NULL AS datetime) As LastIntCollDate,
       //                     Sum(Loan_Trn.Prl_Sched) AS Prl_Sched,
       //                     Sum(Loan_Trn.Disb_Amt) AS Disb_Amt, 
       //                     Sum(Loan_Trn.Disb_Amt) AS PrlPayment,
       //                     Sum(Loan_Trn.PrlColl_Amt) AS PrlColl_Amt,
       //                     Sum(Loan_Trn.IntCalc_Amt) AS IntCalc_Amt,
       //                     Sum(Loan_Trn.IntColl_Amt) AS IntColl_Amt,
       //                     Sum(Loan_Trn.PICalc_Amt) AS PICalc_Amt,
       //                     Sum(Loan_Trn.PIColl_Amt) AS PIColl_Amt,
       //                     ISNULL(Sum(Loan_Trn.IODCalc_Amt),0) AS IODCalc_Amt,
       //                     ISNULL(Sum(Loan_Trn.IODColl_Amt),0) AS IODColl_Amt,
       //                     Sum(Loan_Trn.Prl_Sched) - Sum(Loan_Trn.PrlColl_Amt) AS Prl_OD,
       //                     Sum(Loan_Trn.IntCalc_Amt) - Sum(Loan_Trn.IntColl_Amt) AS Int_Bal,
       //                     Sum(Loan_Trn.PICalc_Amt) - Sum(Loan_Trn.PIColl_Amt) AS PI_Bal,
       //                     ISNULL(Sum(Loan_Trn.IODCalc_Amt)  - Sum(Loan_Trn.IODColl_Amt),0) AS IOD_Bal
       //                     FROM Loan_Trn INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id 
       //                     INNER JOIN Mem_Master ON Loan_Master.Mem_Id = Mem_Master.mem_Id 
       //                     INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id 
       //                     Where Loan_Trn.TrnTr_Delete = 0 AND CAST(Loan_Trn.Trn_Date as date) BETWEEN @fromDate AND @toDate AND Loan_Master.Loan_Type = @loanType 
       //                     GROUP BY Loan_Trn.Loan_id,Loan_Master.Loan_Type,Loan_Master.Loan_No,Loan_Master.Mem_Id,Loan_Master.roi,Loan_Master.San_date,Mem_Master.memberNo,Mem_Master.PerNo,Mem_Master.memberName,Mem_Master.Token_PersonNo,Loan_Master.Scheme_Id,Loan_Schemes.Scheme_Name
       //                 )
       //                 SELECT Loan_id, Loan_No,San_date,Mem_Id, roi,memberNo, PerNo, memberNo,memberName, Token_PersonNo, Scheme_Id, Scheme_Name, 
       //                     Sum(Prl_OS) AS Prl_OS, Sum(Prl_OB) AS Prl_OB, Sum(Int_OB) AS Int_OB, Sum(PI_OB) AS PI_OB, Sum(IOD_OB) AS IOD_OB, 
       //                     Max(LastPrlCollDate) AS LastPrlCollDate, Max(LastIntCollDate) AS LastIntCollDate,
       //                     Sum(Prl_Sched) AS Prl_Sched,
       //                     Sum(Disb_Amt) AS Disb_Amt, Sum(PrlPayment) AS PrlPayment, Sum(PrlColl_Amt) AS PrlColl_Amt, 
       //                     Sum(IntCalc_Amt) AS IntCalc_Amt, Sum(IntColl_Amt) AS IntColl_Amt , 
       //                     Sum(PICalc_Amt) AS PICalc_Amt, Sum(PIColl_Amt)AS PIColl_Amt,
       //                     Sum(IODCalc_Amt) AS IODCalc_Amt, Sum(IODColl_Amt)AS IODColl_Amt,
       //                     Sum(Prl_OD) AS Prl_OD,
       //                     Sum(Int_Bal) AS Int_Bal,
       //                     Sum(PI_Bal) AS PI_Bal,
       //                     Sum(IOD_Bal) AS IOD_Bal
       //                         FROM Dt1
       //                         GROUP BY Loan_id,Loan_Type,Loan_No,San_date,Mem_Id,roi,memberNo,PerNo,memberName,Token_PersonNo,Scheme_Id,Scheme_Name"
       //                 , new NpgsqlParameter("@fromDate", fromDate.ToString("yyyy-MM-dd"))
       //                 , new NpgsqlParameter("@toDate", toDate.ToString("yyyy-MM-dd"))
       //                 , new NpgsqlParameter("@loanType", loanType)).ToListAsync();
                #endregion

                #region postgresql query
                string query = @"WITH Dt1 AS(
                    SELECT
                        Loan_Trn.Loan_id,
                        Loan_Master.Loan_No,
                        Loan_Master.Mem_Id,
                        Loan_Master.roi,
                        Loan_Master.San_date,
                        Mem_Master.memberNo,
                        Mem_Master.PerNo,
                        Mem_Master.memberName,
                        Mem_Master.Token_PersonNo,
                        Loan_Master.Loan_Type,
                        Loan_Master.Scheme_Id,
                        Loan_Schemes.Scheme_Name,
                        SUM(Loan_Trn.Disb_Amt) - SUM(Loan_Trn.PrlColl_Amt) AS Prl_OS,
                        SUM(Loan_Trn.Disb_Amt) - SUM(Loan_Trn.PrlColl_Amt) AS Prl_OB,
                        SUM(Loan_Trn.IntCalc_Amt) - SUM(Loan_Trn.IntColl_Amt) AS Int_OB,
                        SUM(Loan_Trn.PICalc_Amt) - SUM(Loan_Trn.PIColl_Amt) AS PI_OB,
                        COALESCE(SUM(Loan_Trn.IODCalc_Amt) - SUM(Loan_Trn.IODColl_Amt), 0) AS IOD_OB,
                        CAST(NULL AS TIMESTAMP) AS LastPrlCollDate,
                        CAST(NULL AS TIMESTAMP) AS LastIntCollDate,
                        0::DOUBLE PRECISION AS Prl_Sched,
                        SUM(Loan_Trn.Disb_Amt) AS Disb_Amt,
                        0::DOUBLE PRECISION AS PrlPayment,
                        0::DOUBLE PRECISION AS PrlColl_Amt,
                        0::DOUBLE PRECISION AS IntCalc_Amt,
                        0::DOUBLE PRECISION AS IntColl_Amt,
                        0::DOUBLE PRECISION AS PICalc_Amt,
                        0::DOUBLE PRECISION AS PIColl_Amt,
                        0::DOUBLE PRECISION AS IODCalc_Amt,
                        0::DOUBLE PRECISION AS IODColl_Amt,
                        SUM(Loan_Trn.Prl_Sched) - SUM(Loan_Trn.PrlColl_Amt) AS Prl_OD,
                        0::DOUBLE PRECISION AS Int_Bal,
                        0::DOUBLE PRECISION AS PI_Bal,
                        0::DOUBLE PRECISION AS IOD_Bal
                    FROM Loan_Trn
                    INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id
                    INNER JOIN Mem_Master ON Loan_Master.Mem_Id = Mem_Master.mem_Id
                    INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id
                    WHERE Loan_Master.loan_delete = FALSE AND Loan_Master.brcode = @brCode AND Loan_Master.voc_status = 'V'
                    AND Loan_Trn.TrnTr_Delete = false AND Loan_Trn.Trn_Date::DATE < @fromDate
                    AND Loan_Trn.BrCode = @brCode AND Loan_Trn.voc_status = 'V'
                    AND Mem_Master.brcode = @brCode
                    AND Loan_Schemes.brcode = @brCode
                    GROUP BY
                        Loan_Trn.Loan_id, Loan_Master.Loan_Type, Loan_Master.Loan_No,
                        Loan_Master.Mem_Id, Loan_Master.roi, Loan_Master.San_date,
                        Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName,
                        Mem_Master.Token_PersonNo, Loan_Master.Scheme_Id, Loan_Schemes.Scheme_Name
                    HAVING
                        Loan_Master.Loan_Type = @loanType AND(
                            SUM(Loan_Trn.Disb_Amt) - SUM(Loan_Trn.PrlColl_Amt) > 0 OR
                            SUM(Loan_Trn.IntCalc_Amt) - SUM(Loan_Trn.IntColl_Amt) > 0 OR
                            SUM(Loan_Trn.PICalc_Amt) - SUM(Loan_Trn.PIColl_Amt) > 0 OR
                            SUM(Loan_Trn.IODCalc_Amt) - SUM(Loan_Trn.IODColl_Amt) > 0
                        )

                    UNION

                    SELECT
                        Loan_Trn.Loan_id,
                        Loan_Master.Loan_No,
                        Loan_Master.Mem_Id,
                        Loan_Master.roi,
                        Loan_Master.San_date,
                        Mem_Master.memberNo,
                        Mem_Master.PerNo,
                        Mem_Master.memberName,
                        Mem_Master.Token_PersonNo,
                        Loan_Master.Loan_Type,
                        Loan_Master.Scheme_Id,
                        Loan_Schemes.Scheme_Name,
                        SUM(Loan_Trn.Disb_Amt) - SUM(Loan_Trn.PrlColl_Amt) AS Prl_OS,
                        0::DOUBLE PRECISION AS Prl_OB,
                        0::DOUBLE PRECISION AS Int_OB,
                        0::DOUBLE PRECISION AS PI_OB,
                        0::DOUBLE PRECISION AS IOD_OB,
                        CAST(NULL AS TIMESTAMP) AS LastPrlCollDate,
                        CAST(NULL AS TIMESTAMP) AS LastIntCollDate,
                        SUM(Loan_Trn.Prl_Sched) AS Prl_Sched,
                        SUM(Loan_Trn.Disb_Amt) AS Disb_Amt,
                        SUM(Loan_Trn.Disb_Amt) AS PrlPayment,
                        SUM(Loan_Trn.PrlColl_Amt) AS PrlColl_Amt,
                        SUM(Loan_Trn.IntCalc_Amt) AS IntCalc_Amt,
                        SUM(Loan_Trn.IntColl_Amt) AS IntColl_Amt,
                        SUM(Loan_Trn.PICalc_Amt) AS PICalc_Amt,
                        SUM(Loan_Trn.PIColl_Amt) AS PIColl_Amt,
                        COALESCE(SUM(Loan_Trn.IODCalc_Amt), 0) AS IODCalc_Amt,
                        COALESCE(SUM(Loan_Trn.IODColl_Amt), 0) AS IODColl_Amt,
                        SUM(Loan_Trn.Prl_Sched) - SUM(Loan_Trn.PrlColl_Amt) AS Prl_OD,
                        SUM(Loan_Trn.IntCalc_Amt) - SUM(Loan_Trn.IntColl_Amt) AS Int_Bal,
                        SUM(Loan_Trn.PICalc_Amt) - SUM(Loan_Trn.PIColl_Amt) AS PI_Bal,
                        COALESCE(SUM(Loan_Trn.IODCalc_Amt) - SUM(Loan_Trn.IODColl_Amt), 0) AS IOD_Bal
                    FROM Loan_Trn
                    INNER JOIN Loan_Master ON Loan_Trn.Loan_id = Loan_Master.Loan_id
                    INNER JOIN Mem_Master ON Loan_Master.Mem_Id = Mem_Master.mem_Id
                    INNER JOIN Loan_Schemes ON Loan_Master.Scheme_Id = Loan_Schemes.Scheme_Id
                    WHERE Loan_Trn.TrnTr_Delete = FALSE
                      AND Loan_Trn.Trn_Date::DATE BETWEEN @fromDate AND @toDate AND Loan_Trn.voc_status ='V'
                      AND Loan_Master.Loan_Type = @loanType 
                      AND Loan_Master.BrCode = @brCode  AND Loan_Master.voc_status ='V'
                      AND Loan_Schemes.brcode = @brCode
                    GROUP BY
                        Loan_Trn.Loan_id, Loan_Master.Loan_Type, Loan_Master.Loan_No,
                        Loan_Master.Mem_Id, Loan_Master.roi, Loan_Master.San_date,
                        Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName,
                        Mem_Master.Token_PersonNo, Loan_Master.Scheme_Id, Loan_Schemes.Scheme_Name
                )

                SELECT
                    Loan_id, Loan_No, San_date, Mem_Id, roi, memberNo, PerNo, memberName, Token_PersonNo,
                    Scheme_Id, Scheme_Name,
                    SUM(Prl_OS) AS Prl_OS,
                    SUM(Prl_OB) AS Prl_OB,
                    SUM(Int_OB) AS Int_OB,
                    SUM(PI_OB) AS PI_OB,
                    SUM(IOD_OB) AS IOD_OB,
                    MAX(LastPrlCollDate) AS LastPrlCollDate,
                    MAX(LastIntCollDate) AS LastIntCollDate,
                    SUM(Prl_Sched) AS Prl_Sched,
                    SUM(Disb_Amt) AS Disb_Amt,
                    SUM(PrlPayment) AS PrlPayment,
                    SUM(PrlColl_Amt) AS PrlColl_Amt,
                    SUM(IntCalc_Amt) AS IntCalc_Amt,
                    SUM(IntColl_Amt) AS IntColl_Amt,
                    SUM(PICalc_Amt) AS PICalc_Amt,
                    SUM(PIColl_Amt) AS PIColl_Amt,
                    SUM(IODCalc_Amt) AS IODCalc_Amt,
                    SUM(IODColl_Amt) AS IODColl_Amt,
                    SUM(Prl_OD) AS Prl_OD,
                    SUM(Int_Bal) AS Int_Bal,
                    SUM(PI_Bal) AS PI_Bal,
                    SUM(IOD_Bal) AS IOD_Bal
                FROM Dt1
                GROUP BY
                    Loan_id, Loan_Type, Loan_No, San_date, Mem_Id, roi, memberNo,
                    PerNo, memberName, Token_PersonNo, Scheme_Id, Scheme_Name";

                loanList = await CSISContext.Database
                    .SqlQueryRaw<rptFALoanOutstanding2>(query,
                        new NpgsqlParameter("@fromDate", fromDate.ToString("yyyy-MM-dd")),
                        new NpgsqlParameter("@toDate", toDate.ToString("yyyy-MM-dd")),
                        new NpgsqlParameter("@loanType", loanType),
                       new NpgsqlParameter ("@brCode",brCode ))
                    .ToListAsync();

                #endregion 

                //loanList = loanList.Where(x => x.Loan_Id == 2098).ToList();

                foreach (var os in loanList)
                {
                    //os.Prl_OS = os.Prl_OB +  os.Disb_Amt - os.PrlColl_Amt;
                    os.PI_Bal = os.PI_OB + os.PICalc_Amt - os.PIColl_Amt;
                    os.IOD_Bal = os.IOD_OB + os.IODCalc_Amt - os.IODColl_Amt;
                    if (os.Prl_OD < 0) os.Prl_OD = 0;
                    if (os.Int_Bal < 0) os.Int_Bal = 0;
                    if (os.PI_Bal < 0) os.PI_Bal = 0;
                    if (os.IOD_Bal < 0) os.IOD_Bal = 0;
                    if (os.Int_OB < 0) os.Int_OB = 0;
                    if (os.PI_OB < 0) os.PI_OB = 0;
                    if (os.IOD_OB < 0) os.IOD_OB = 0;
                    lastPrlCollDate = CSISContext.Database.SqlQueryRaw<DateTime?>(
                        @"select Max(trn_Date) AS LastPrlCollDate  from loan_trn where loan_Id = @loanId and TrnTr_Delete = 0 and PrlColl_Amt >0 and Trn_Date <=@toDate"
                        , new NpgsqlParameter("@loanId", os.Loan_Id)
                        , new NpgsqlParameter("@toDate", toDate.ToString("yyyy-MM-dd"))).FirstOrDefault();
                    os.LastPrlCollDate = lastPrlCollDate;
                    lastIntCollDate = CSISContext.Database.SqlQueryRaw<DateTime?>(
                        @"select Max(trn_Date) AS LastPrlCollDate  from loan_trn where loan_Id = @loanId and TrnTr_Delete = 0 and IntColl_Amt >0 and Trn_Date <=@toDate"
                        , new NpgsqlParameter("@loanId", os.Loan_Id)
                        , new NpgsqlParameter("@toDate", toDate.ToString("yyyy-MM-dd"))).FirstOrDefault();
                    os.LastIntCollDate = lastIntCollDate;
                }

                /// get prl, int,iod and pi overdue upto 1st of todate.(i.e., todate = 31-03-2021 then get overdue upto 01-03-2021
                DateTime LastDueDate = new DateTime(toDate.Year, toDate.Month, 1);
            }
            catch (Exception)
            {
                throw;
            }
            return loanList;
        }

        public async Task<List<rptFAShareCapitalDevidend>> GetFAShareCapitalDividend(DateTime fromDate, DateTime todate, string brCode)
        {
            List<rptFAShareCapitalDevidend> scDividendList = new List<rptFAShareCapitalDevidend>();
            try
            {
                #region sql server query
                //      scDividendList = await CSISContext.Database.SqlQueryRaw<rptFAShareCapitalDevidend>(
                //              @"With Db1
                //              AS
                //              (
                //              SELECT Mem_Trn.Mem_Id, 
                //                  CAST(Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) AS float) AS Amt_OB,
                //CAST(Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) AS float) AS Int_OB,
                //                  CAST(0 as float)  AS Rpt_Amt,
                //                  CAST(0 as float)  AS Pmt_Amt,
                //CAST(0 as float) AS IntCalc_Amt,
                //CAST(0 as float) AS IntPaid_Amt
                //                  FROM Mem_Trn 
                //                  WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date < @fromDate And Mem_Trn.Trn_Type = 3
                //                  GROUP BY Mem_Trn.Mem_Id 
                //                  HAVING (Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) >0  or Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) >0)
                //              UNION
                //              SELECT Mem_Trn.Mem_Id, 
                //                  CAST(0 as float) AS Amt_OB,
                //CAST(0 as float) AS Int_OB,
                //                  Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, 
                //                  Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt,
                //CAST(Sum(Mem_Trn.IntCalc_Amt) as float) AS IntCalc_Amt, 
                //CAST(Sum(Mem_Trn.IntPaid_Amt) as float) AS IntPaid_Amt 
                //                  FROM Mem_Trn INNER  JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id 
                //                  INNER JOIN Fin_Ledger ON Mem_Trn.Led_Id = Fin_Ledger.Led_Id 
                //                  WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date BETWEEN @fromDate AND @toDate And Mem_Trn.Trn_Type = 3
                //                  GROUP BY Mem_Trn.Mem_Id, Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName, Mem_Trn.Trn_Type,Mem_Trn.Led_Id,Fin_Ledger.Led_Name 
                //                  HAVING  Sum(Mem_Trn.Rpt_Amt) >0 OR Sum(Mem_Trn.Pmt_Amt) >0 OR Sum(Mem_Trn.IntCalc_Amt) >0 OR Sum(Mem_Trn.IntPaid_Amt) >0
                //              )
                //              SELECT Db1.Mem_Id,memberNo, PerNo, memberName, Sum(Amt_OB) AS Amt_OB, Sum(Int_OB) AS Int_OB, Sum(Rpt_Amt) AS Rpt_Amt, Sum(Pmt_Amt) AS Pmt_Amt, Sum(IntCalc_Amt) AS IntCalc_Amt, Sum(IntPaid_Amt) AS IntPaid_Amt  FROM Db1 
                //              INNER  JOIN Mem_Master ON Db1.Mem_Id = Mem_Master.mem_Id
                //              GROUP BY Db1.Mem_Id, memberNo, PerNo, memberName 
                //              order by memberNo "
                //              , new NpgsqlParameter("@fromDate", fromDate)
                //              , new NpgsqlParameter("@toDate", todate)).ToListAsync();
                #endregion

                #region postgresql query
                scDividendList = await CSISContext.Database.SqlQueryRaw<rptFAShareCapitalDevidend>(
                        @"With Db1
                        AS
                        (
                        SELECT Mem_Trn.Mem_Id, 
                            Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) :: DOUBLE PRECISION AS Amt_OB,
						    Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) ::DOUBLE PRECISION AS Int_OB,
                            0 ::DOUBLE PRECISION AS Rpt_Amt,
                            0 ::DOUBLE PRECISION AS Pmt_Amt,
						    0 ::DOUBLE PRECISION AS IntCalc_Amt,
						    0 ::DOUBLE PRECISION AS IntPaid_Amt
                            FROM Mem_Trn 
                            WHERE Mem_Trn.MemTrn_Delete = false And Mem_Trn.Trn_Date < @fromDate And Mem_Trn.Trn_Type = 3
                            AND Mem_Trn.BrCode = @brCode and Mem_Trn.voc_status = 'V'
                            GROUP BY Mem_Trn.Mem_Id 
                            HAVING (Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) >0  or Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) >0)
                        UNION
                        SELECT Mem_Trn.Mem_Id, 
                            0 ::DOUBLE PRECISION AS Amt_OB,
						    0 ::DOUBLE PRECISION AS Int_OB,
                            Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, 
                            Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt,
						    Sum(Mem_Trn.IntCalc_Amt) :: DOUBLE PRECISION AS IntCalc_Amt, 
						    Sum(Mem_Trn.IntPaid_Amt) :: DOUBLE PRECISION AS IntPaid_Amt 
                            FROM Mem_Trn INNER  JOIN Mem_Master ON Mem_Trn.Mem_Id = Mem_Master.mem_Id 
                            INNER JOIN Fin_Ledger ON Mem_Trn.Led_Id = Fin_Ledger.Led_Id 
                            WHERE Mem_Trn.MemTrn_Delete = false 
                            And Mem_Trn.Trn_Date BETWEEN @fromDate AND @toDate And Mem_Trn.Trn_Type = 3
                            AND Mem_Trn.BrCode = @brCode AND Mem_Trn.voc_status = 'V'
                            GROUP BY Mem_Trn.Mem_Id, Mem_Master.memberNo, Mem_Master.PerNo, Mem_Master.memberName, Mem_Trn.Trn_Type,Mem_Trn.Led_Id,Fin_Ledger.Led_Name 
                            HAVING  Sum(Mem_Trn.Rpt_Amt) >0 OR Sum(Mem_Trn.Pmt_Amt) >0 OR Sum(Mem_Trn.IntCalc_Amt) >0 OR Sum(Mem_Trn.IntPaid_Amt) >0
                        )
                        SELECT Db1.Mem_Id,memberNo, PerNo, memberName, Sum(Amt_OB) AS Amt_OB, Sum(Int_OB) AS Int_OB, Sum(Rpt_Amt) AS Rpt_Amt, Sum(Pmt_Amt) AS Pmt_Amt, Sum(IntCalc_Amt) AS IntCalc_Amt, Sum(IntPaid_Amt) AS IntPaid_Amt  FROM Db1 
                        INNER  JOIN Mem_Master ON Db1.Mem_Id = Mem_Master.mem_Id
                        GROUP BY Db1.Mem_Id, memberNo, PerNo, memberName 
                        order by memberNo "
                        , new NpgsqlParameter("@fromDate", fromDate)
                        , new NpgsqlParameter("@toDate", todate)
                        , new NpgsqlParameter ("@brCode",brCode)).ToListAsync();
                #endregion 

                foreach (var scDividend in scDividendList)
                {
                    scDividend.Total_Amt = scDividend.Amt_OB + scDividend.Rpt_Amt;
                    scDividend.Amt_CB = scDividend.Amt_OB + scDividend.Rpt_Amt - scDividend.Pmt_Amt;
                    scDividend.Int_CB = scDividend.Int_OB + scDividend.IntCalc_Amt - scDividend.IntPaid_Amt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return scDividendList;
        }

        public async Task<List<rptMemberSBAcTrn>> GetSBAccountTrn(DateTime fromDate, DateTime toDate, string brCode)
        {
            List<rptMemberSBAcTrn> sbList = new List<rptMemberSBAcTrn>();
            try
            {
                #region sql server query
                //          sbList = await CSISContext.Database.SqlQueryRaw<rptMemberSBAcTrn>(
                //                  @"With Db1
                //                  AS
                //                  (
                //                  SELECT Mem_Trn.Mem_Id, 
                //	Mem_Trn.Acc_Id,
                //                      CAST(Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) AS float) AS Amt_OB,
                //    CAST(Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) AS float) AS Int_OB,
                //                      CAST(0 as float)  AS Rpt_Amt,
                //                      CAST(0 as float)  AS Pmt_Amt,
                //    CAST(0 as float) AS IntCalc_Amt,
                //    CAST(0 as float) AS IntPaid_Amt,
                //	CAST(0 as float) AS Amt_CB,
                //	CAST(0 as float) AS Int_CB
                //                      FROM Mem_Trn 
                //                      WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date < @fromDate And Mem_Trn.Trn_Type = 7
                //                      GROUP BY Mem_Trn.Mem_Id ,Mem_Trn.Acc_Id
                //                      HAVING (Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) >0  or Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) >0)
                //                  UNION
                //                  SELECT Mem_Trn.Mem_Id, 
                //	Mem_Trn.Acc_Id,
                //                      CAST(0 as float) AS Amt_OB,
                //    CAST(0 as float) AS Int_OB,
                //                      Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, 
                //                      Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt,
                //    CAST(Sum(Mem_Trn.IntCalc_Amt) as float) AS IntCalc_Amt, 
                //    CAST(Sum(Mem_Trn.IntPaid_Amt) as float) AS IntPaid_Amt,
                //	CAST(0 as float) AS Amt_CB,
                //	CAST(0 as float) AS Int_CB
                //                      FROM Mem_Trn 
                //                      WHERE Mem_Trn.MemTrn_Delete = 0 And Mem_Trn.Trn_Date BETWEEN @fromDate AND @toDate And Mem_Trn.Trn_Type = 7
                //                      GROUP BY Mem_Trn.Mem_Id,Mem_Trn.Acc_Id
                //                      HAVING  (Sum(Mem_Trn.Rpt_Amt) >0 OR Sum(Mem_Trn.Pmt_Amt) >0 OR Sum(Mem_Trn.IntCalc_Amt) >0 OR Sum(Mem_Trn.IntPaid_Amt) >0)
                //                  )
                //                  SELECT Db1.Mem_Id,Db1.Acc_Id,memberNo, PerNo, memberName, Acc_No, Sum(Amt_OB) AS Amt_OB, Sum(Int_OB) AS Int_OB, Sum(Rpt_Amt) AS Rpt_Amt, Sum(Pmt_Amt) AS Pmt_Amt, Sum(IntCalc_Amt) AS IntCalc_Amt, Sum(IntPaid_Amt) AS IntPaid_Amt,
                //Sum(Int_CB) AS Int_CB, Sum(Amt_CB) AS Amt_CB  
                //FROM Db1 
                //                  INNER  JOIN Mem_Master ON Db1.Mem_Id = Mem_Master.mem_Id
                //INNER JOIN SBCA_Master ON Db1.Acc_Id =  SBCA_Master.Acc_Id 
                //                  GROUP BY Db1.Mem_Id,Db1.Acc_Id, memberNo, PerNo, memberName ,Acc_No
                //                  order by Acc_No"
                //                  , new NpgsqlParameter("@fromDate", fromDate)
                //                  , new NpgsqlParameter("@toDate", toDate)).ToListAsync();
                #endregion

                #region postgresql query
                sbList = await CSISContext.Database.SqlQueryRaw<rptMemberSBAcTrn>(
                        @"With Db1
                        AS
                        (
                        SELECT Mem_Trn.Mem_Id, 
							Mem_Trn.Acc_Id,
                            Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) :: DOUBLE PRECISION AS Amt_OB,
						    Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) :: DOUBLE PRECISION AS Int_OB,
                            0 :: DOUBLE PRECISION  AS Rpt_Amt,
                            0 :: DOUBLE PRECISION  AS Pmt_Amt,
						    0 :: DOUBLE PRECISION AS IntCalc_Amt,
						    0 :: DOUBLE PRECISION AS IntPaid_Amt,
							0 :: DOUBLE PRECISION AS Amt_CB,
							0 :: DOUBLE PRECISION  AS Int_CB
                            FROM Mem_Trn 
                            WHERE Mem_Trn.MemTrn_Delete = FALSE And Mem_Trn.Trn_Date < @fromDate And Mem_Trn.Trn_Type = 7
                            AND Mem_Trn.BrCode = @brCode AND Mem_Trn.voc_status = 'V'
                            GROUP BY Mem_Trn.Mem_Id ,Mem_Trn.Acc_Id
                            HAVING (Sum(Mem_Trn.Rpt_Amt) - Sum(Mem_Trn.Pmt_Amt) >0  or Sum(Mem_Trn.IntCalc_Amt) - Sum(Mem_Trn.IntPaid_Amt) >0)
                        UNION
                        SELECT Mem_Trn.Mem_Id, 
							Mem_Trn.Acc_Id,
                            0 :: DOUBLE PRECISION AS Amt_OB,
						    0 :: DOUBLE PRECISION AS Int_OB,
                            Sum(Mem_Trn.Rpt_Amt) AS Rpt_Amt, 
                            Sum(Mem_Trn.Pmt_Amt) AS Pmt_Amt,
						    Sum(Mem_Trn.IntCalc_Amt) :: DOUBLE PRECISION AS IntCalc_Amt, 
						    Sum(Mem_Trn.IntPaid_Amt) :: DOUBLE PRECISION AS IntPaid_Amt,
							0 :: DOUBLE PRECISION AS Amt_CB,
							0 :: DOUBLE PRECISION AS Int_CB
                            FROM Mem_Trn 
                            WHERE Mem_Trn.MemTrn_Delete = false And Mem_Trn.Trn_Date BETWEEN @fromDate AND @toDate And Mem_Trn.Trn_Type = 7
                            AND  Mem_Trn.BrCode = @brCode AND Mem_Trn.voc_status = 'V'
                            GROUP BY Mem_Trn.Mem_Id,Mem_Trn.Acc_Id
                            HAVING  (Sum(Mem_Trn.Rpt_Amt) >0 OR Sum(Mem_Trn.Pmt_Amt) >0 OR Sum(Mem_Trn.IntCalc_Amt) >0 OR Sum(Mem_Trn.IntPaid_Amt) >0)
                        )
                        SELECT Db1.Mem_Id,Db1.Acc_Id,memberNo, PerNo, memberName, Acc_No, Sum(Amt_OB) AS Amt_OB, Sum(Int_OB) AS Int_OB, Sum(Rpt_Amt) AS Rpt_Amt, Sum(Pmt_Amt) AS Pmt_Amt, Sum(IntCalc_Amt) AS IntCalc_Amt, Sum(IntPaid_Amt) AS IntPaid_Amt,
						Sum(Int_CB) AS Int_CB, Sum(Amt_CB) AS Amt_CB  
						FROM Db1 
                        INNER  JOIN Mem_Master ON Db1.Mem_Id = Mem_Master.mem_Id
						INNER JOIN SBCA_Master ON Db1.Acc_Id =  SBCA_Master.Acc_Id 
                        GROUP BY Db1.Mem_Id,Db1.Acc_Id, memberNo, PerNo, memberName ,Acc_No
                        order by Acc_No"
                        , new NpgsqlParameter("@fromDate", fromDate)
                        , new NpgsqlParameter("@toDate", toDate)
                        , new NpgsqlParameter("@brCode",brCode )).ToListAsync();
                #endregion 

                foreach (var item in sbList)
                {
                    item.Amt_CB = item.Amt_OB + item.Rpt_Amt - item.Pmt_Amt;
                    item.Total_Amt = item.Amt_OB + item.Rpt_Amt;
                    item.Int_OB = item.Int_OB + item.IntCalc_Amt - item.IntPaid_Amt;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return sbList;
        }
    }
}
