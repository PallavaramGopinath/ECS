using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Infin8.Coapp.Repository
{
    public class ReportsTermDepositsRepository : Repository<Reports_Master>, IReportsTermDepositsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsTermDepositsRepository(CSISContext context) : base(context)
        {
        }

        public async Task< List<DropdownItem>> GetTDNos( string TDSchemeType, DateTime fromDate, DateTime toDate,string brCode)
        {
            List<DropdownItem> tdNos = new();

            try
            {
                #region query
                //tdNos = CSISContext.Database.SqlQueryRaw<DropdownItem>(
                //    @"With Db1
                //        AS
                //        (
                //        SELECT 
                //            CAST(f.TD_Id AS VARCHAR) as [Value], 
                //            f.TD_No as [Text]
                //            FROM TermDeposit_Master f 
                //            INNER JOIN TermDeposit_Schemes g ON f.TDScheme_Id = g.TDScheme_Id
	               //         INNER JOIN TermDeposit_Trn as trn ON f.TD_Id = trn.TD_Id 
                //                WHERE (f.TD_Delete = 0) AND (trn.Trn_Date <  @fromDate)
                //            AND (g.TDSchemeType = @SchemeType)
	               //         GROUP BY f.TD_Id, f.TD_No 
	               //         HAVING SUM(trn.DepositReceiptAmount) - Sum(trn.DepositPaidAmount) >0
                //        UNION
                //        SELECT  CAST(f.TD_Id AS VARCHAR) as [Value], 
                //            f.TD_No as [Text]
                //            FROM TermDeposit_Master f 
	               //         INNER JOIN TermDeposit_Schemes g ON f.TDScheme_Id = g.TDScheme_Id
	               //         WHERE (f.TD_Delete = 0) AND (f.ValueDate  BETWEEN  @fromDate AND @toDate) AND (g.TDSchemeType = @SchemeType)
                //        )
                //        SELECT * FROM Db1 ORDER BY Db1.Text"
                //    , new NpgsqlParameter("@SchemeType", TDSchemeType)
                //    , new NpgsqlParameter("@fromDate", fromDate)
                //    , new NpgsqlParameter("@toDate", toDate)).ToList();
                #endregion

                #region linq
                // First part of the UNION query
                var query1 = await  (from f in CSISContext.TermDeposit_Master
                              join g in CSISContext.TermDeposit_Schemes on f.TDScheme_Id equals g.TDScheme_Id
                              join trn in CSISContext.TermDeposit_Trn on f.TD_Id equals trn.TD_Id
                              where f.TD_Delete == false && trn.Trn_Date < fromDate && g.TDSchemeType == TDSchemeType && f.BrCode == brCode 
                              group trn by new { f.TD_Id, f.TD_No } into grp
                              where grp.Sum(x => x.DepositReceiptAmount) - grp.Sum(x => x.DepositPaidAmount) > 0
                              select new DropdownItem
                              {
                                  Value = grp.Key.TD_Id.ToString(),
                                  Text = grp.Key.TD_No
                              }).ToListAsync ();

                // Second part of the UNION query
                var query2 = await (from f in CSISContext.TermDeposit_Master
                              join g in CSISContext.TermDeposit_Schemes on f.TDScheme_Id equals g.TDScheme_Id
                              where f.TD_Delete == false && f.ValueDate >= fromDate && f.ValueDate <= toDate && g.TDSchemeType == TDSchemeType && f.BrCode == brCode 
                              select new DropdownItem
                              {
                                  Value = f.TD_Id.ToString(),
                                  Text = f.TD_No
                              }).ToListAsync();

                // Union the two queries and order the final result
                var tdNosData =  query1.Union(query2).OrderBy(item => item.Text).ToList();

                if (tdNosData != null && tdNosData.Any()) tdNos = tdNosData.ToList();
                return tdNos;
                #endregion 
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error in fetching Term deposit Nos " + ex.Message);
                tdNos = new();
            }
            return tdNos;

        }

        public async Task<List<DropdownItem>> GetTDNos(decimal memId, string TDSchemeType, DateTime fromDate, DateTime toDate,string brCode)
        {
            List<DropdownItem> tdNos = new();

            try
            {
                #region linq
                // First part of the UNION query
                var query1 = await (from f in CSISContext.TermDeposit_Master
                                    join g in CSISContext.TermDeposit_Schemes on f.TDScheme_Id equals g.TDScheme_Id
                                    join trn in CSISContext.TermDeposit_Trn on f.TD_Id equals trn.TD_Id
                                    where f.Mem_Id == memId && f.TD_Delete == false && trn.Trn_Date < fromDate && g.TDSchemeType == TDSchemeType && f.BrCode == brCode
                                    group trn by new { f.TD_Id, f.TD_No } into grp
                                    where grp.Sum(x => x.DepositReceiptAmount) - grp.Sum(x => x.DepositPaidAmount) > 0
                                    select new DropdownItem
                                    {
                                        Value = grp.Key.TD_Id.ToString(),
                                        Text = grp.Key.TD_No
                                    }).ToListAsync();

                // Second part of the UNION query
                var query2 = await (from f in CSISContext.TermDeposit_Master
                                    join g in CSISContext.TermDeposit_Schemes on f.TDScheme_Id equals g.TDScheme_Id
                                    where f.Mem_Id == memId && f.TD_Delete == false && f.ValueDate >= fromDate && f.ValueDate <= toDate && g.TDSchemeType == TDSchemeType && f.BrCode == brCode
                                    select new DropdownItem
                                    {
                                        Value = f.TD_Id.ToString(),
                                        Text = f.TD_No
                                    }).ToListAsync();

                // Union the two queries and order the final result
                var tdNosData =  query1.Union(query2).OrderBy(item => item.Text).ToList();

                if (tdNosData != null && tdNosData.Any()) tdNos = tdNosData.ToList();
                return tdNos;
                #endregion 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetching Term deposit Nos " + ex.Message);
                tdNos = new();
            }
            return tdNos;

        }

        public async Task<List<rptTermDepositRegister>> GetTermDepositRegister(List<decimal> TDIdList, DateTime fromDate, DateTime toDate, string TDSchemeType)
        {
            List<rptTermDepositRegister> tdList = new List<rptTermDepositRegister>();
            try
            {
                string fetchIdList = string.Join(",", TDIdList);
                #region query
                //var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositRegister>(
                //            @"SELECT x.*, y.* , z.* 
                //            FROM 
                //                (SELECT TD_Id, 
                //                Max(Trn_Date) AS Trn_Date,
                //                Sum(DepositReceiptAmount) AS DepositReceiptAmount,  Sum(DepositPaidAmount) AS DepositPaidAmount, 
                //                Sum(InterestCalculatedAmount) AS InterestCalculatedAmount, Max(InterestAppliedDate) AS InterestAppliedDate,Sum(InterestPaidAmount) AS InterestPaidAmount,
                //                Sum(PenalCalculatedAmount) AS PenalCalculatedAmount,  Max(PenalAppliedDate) AS PenalAppliedDate, 
                //                Sum(PenalReceivedAmount) AS PenalReceivedAmount
                //                FROM TermDeposit_Trn 
                //                WHERE Trn_Date < @fromDate And TD_Delete = false AND TermDeposit_Trn.voc_status ='V'
                //                GROUP BY TD_Id
                //                UNION
                //                SELECT TD_Id,
                //                Trn_Date,
                //                DepositReceiptAmount, 
                //                DepositPaidAmount, 
                //                InterestCalculatedAmount, 
                //                InterestAppliedDate, 
                //                InterestPaidAmount,  
                //                PenalCalculatedAmount, 
                //                PenalAppliedDate, 
                //                PenalReceivedAmount 
                //            FROM TermDeposit_Trn 
                //            Where Trn_Date BETWEEN @fromDate AND  @toDate AND TermDeposit_Trn.voc_status = 'V'
                //            And TD_Delete = false AND (DepositReceiptAmount >0 OR DepositPaidAmount >0 OR InterestCalculatedAmount >0 OR InterestPaidAmount >0 OR PenalCalculatedAmount >0 OR PenalReceivedAmount >0 )) as x, 
                //          (SELECT TD_Id AS TDMas_TD_Id, Mem_Id, TD_No, TDScheme_Id, TDH_Name, COALESCE(TDH_Age ,0) AS TDH_Age, ModeOfOperation, ValueDate, DepositAmount, 
                //            PeriodInMonths, PeriodInDays, RateOfInterest, PenalRate, MaturityDate, MaturityAmount, 
                //            InterestPayableFrequency, IsDiscountRate, Nominee1Name, Nominee1Age, Nominee1Relationship, 
                //            Nominee2Name, Nominee2Age, Nominee2Relationship, Status,RenewalTD_No from TermDeposit_Master ) as y,
                //        (SELECT mem_Id  AS MemMas_mem_Id, memberNo, PerNo, memberName,FatherName, PerAdd1,PerAdd2,PerAdd3,mobileNo,PANNo,AadharNo,SmartCardNo,memberphoto  FROM Mem_Master ) as z,
                //        (SELECT TDScheme_Id AS schemeId,TDScheme_Name, TDSchemeType from TermDeposit_Schemes where TDSchemeType = @schemeType) as a
                //            Where x.TD_Id  = y.TDMas_TD_Id  AND y.mem_Id = z.MemMas_mem_Id AND y.TDScheme_Id =  a.schemeId AND x.TD_Id in (" + fetchIdList + ") "
                //        , new NpgsqlParameter("@fromDate", fromDate)
                //        , new NpgsqlParameter("@toDate", toDate)
                //        , new NpgsqlParameter("@schemeType", TDSchemeType)).ToListAsync();
                #endregion

                #region linq
                // First part of the UNION query (Trn_Date < fromDate)
                var preFromDateData =
                    from t in CSISContext.TermDeposit_Trn
                    where t.Trn_Date < fromDate && !t.TD_Delete
                    group t by t.TD_Id into g
                    select new
                    {
                        TD_Id = g.Key,
                        Trn_Date = g.Max(t => t.Trn_Date),
                        DepositReceiptAmount = g.Sum(t => t.DepositReceiptAmount),
                        DepositPaidAmount = g.Sum(t => t.DepositPaidAmount),
                        InterestCalculatedAmount = g.Sum(t => t.InterestCalculatedAmount),
                        InterestAppliedDate = g.Max(t => t.InterestAppliedDate),
                        InterestPaidAmount = g.Sum(t => t.InterestPaidAmount),
                        PenalCalculatedAmount = g.Sum(t => t.PenalCalculatedAmount),
                        PenalAppliedDate = g.Max(t => t.PenalAppliedDate),
                        PenalReceivedAmount = g.Sum(t => t.PenalReceivedAmount)
                    };

                // Second part of the UNION query (Trn_Date between fromDate and toDate)
                var inDateRangeData =
                    from t in CSISContext.TermDeposit_Trn
                    where t.Trn_Date >= fromDate && t.Trn_Date <= toDate && !t.TD_Delete &&
                        (t.DepositReceiptAmount > 0 || t.DepositPaidAmount > 0 || t.InterestCalculatedAmount > 0 ||
                         t.InterestPaidAmount > 0 || t.PenalCalculatedAmount > 0 || t.PenalReceivedAmount > 0)
                    select new
                    {
                        t.TD_Id,
                        t.Trn_Date,
                        t.DepositReceiptAmount,
                        t.DepositPaidAmount,
                        t.InterestCalculatedAmount,
                        t.InterestAppliedDate,
                        t.InterestPaidAmount,
                        t.PenalCalculatedAmount,
                        t.PenalAppliedDate,
                        t.PenalReceivedAmount
                    };

                // Combine the two parts using the Union() method
                var combinedTransactions = preFromDateData.Union(inDateRangeData);

                // Filter scheme types
                var tdSchemes =
                    from s in CSISContext.TermDeposit_Schemes
                    where s.TDSchemeType == TDSchemeType
                    select s;

                // Perform the final join operation
                var result = await (
                    from x in combinedTransactions
                    join y in CSISContext.TermDeposit_Master on x.TD_Id equals y.TD_Id
                    join z in CSISContext.mem_master on y.Mem_Id equals z.mem_id
                    join a in tdSchemes on y.TDScheme_Id equals a.TDScheme_Id
                    where TDIdList.Contains(x.TD_Id)
                    select new rptTermDepositRegister
                    {
                        // Map properties from x, y, z, and a to rptTermDepositRegister
                        TD_Id = x.TD_Id,
                        Trn_Date = (DateTime)x.Trn_Date!,
                        DepositReceiptAmount = x.DepositReceiptAmount,
                        DepositPaidAmount = x.DepositPaidAmount,
                        InterestCalculatedAmount = x.InterestCalculatedAmount,
                        InterestAppliedDate = x.InterestAppliedDate,
                        InterestPaidAmount = x.InterestPaidAmount,
                        PenalCalculatedAmount = x.PenalCalculatedAmount,
                        PenalAppliedDate = x.PenalAppliedDate,
                        PenalReceivedAmount = x.PenalReceivedAmount,
                        Mem_Id = y.Mem_Id,
                        TD_No = y.TD_No,
                        TDScheme_Id = y.TDScheme_Id,
                        TDH_Name = y.TDH_Name,
                        TDH_Age = y.TDH_Age, // Using null-coalescing operator for COALESCE
                        ModeOfOperation = y.ModeOfOperation,
                        ValueDate = y.ValueDate,
                        DepositAmount = y.DepositAmount,
                        PeriodInMonths = y.PeriodInMonths,
                        PeriodInDays = y.PeriodInDays,
                        RateOfInterest = y.RateOfInterest,
                        PenalRate = y.PenalRate,
                        MaturityDate = y.MaturityDate,
                        MaturityAmount = y.MaturityAmount,
                        InterestPayableFrequency = y.InterestPayableFrequency,
                        IsDiscountRate = y.IsDiscountRate,
                        Nominee1Name = y.Nominee1Name,
                        Nominee1Age = y.Nominee1Age,
                        Nominee1Relationship = y.Nominee1Relationship,
                        Nominee2Name = y.Nominee2Name,
                        Nominee2Age = y.Nominee2Age,
                        Nominee2Relationship = y.Nominee2Relationship,
                        Status = y.Status,
                        RenewalTD_No = y.RenewalTD_No,
                        MemberNo = z.memberno,
                        PerNo = z.perno,
                        MemberName = z.membername,
                        FatherName = z.fathername,
                        PerAdd1 = z.peradd1,
                        PerAdd2 = z.peradd2,
                        PerAdd3 = z.peradd3,
                        MobileNo = z.mobileno,
                        PANNo = z.panno,
                        AadharNo = z.aadharno,
                        SmartCardNo = z.smartcardno,
                        memberphoto = z.memberphoto,
                        TDScheme_Name = a.TDScheme_Name,
                        TDSchemeType = a.TDSchemeType
                    }).ToListAsync();
                #endregion 

                if (result != null && result.Any()) tdList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetching term deposit ledger data " + ex.ToString());
                tdList = new();
            }
            return tdList;
        }

        public async Task<List<rptFDOutstanding>> GetTermDepositOutstanding(DateTime asOnDate, string TDSchemeType, string brCode)
        {
            List<rptFDOutstanding> tdList = new List<rptFDOutstanding>();
            try
            {
                #region query
                //var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptFDOutstanding>(
                //        @"SELECT
                //        TermDeposit_Master.TDScheme_Id,
                //        TermDeposit_Schemes.TDScheme_Name,
                //        TermDeposit_Master.TD_No,
                //        TermDeposit_Master.TDH_Name, 
                //        TermDeposit_Master.ValueDate, 
                //        TermDeposit_Master.DepositAmount,
                //        TermDeposit_Master.PeriodInMonths, 
                //        TermDeposit_Master.PeriodInDays, 
                //        TermDeposit_Master.RateOfInterest, 
                //        TermDeposit_Master.MaturityDate,
	               //     Mem_Master.memberNo, TermDeposit_Schemes.TDSchemeType,
                //        TermDeposit_Master.MaturityAmount, 
                //        Sum(TermDeposit_Trn.DepositReceiptAmount) AS DepositReceiptAmount,
	               //     Sum(TermDeposit_Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, 
                //        Max(TermDeposit_Trn.InterestAppliedDate) AS InterestAppliedDate,
                //        Sum(TermDeposit_Trn.InterestPaidAmount) AS InterestPaidAmount
                //    From
                //        TermDeposit_Master  INNER JOIN TermDeposit_Trn TermDeposit_Trn ON TermDeposit_Master.TD_Id = TermDeposit_Trn.TD_Id
                //        INNER JOIN Mem_Master  ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
                //        INNER JOIN TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                //        WHERE TermDeposit_Master.TD_Delete = false AND TermDeposit_Trn.TD_Delete= false  AND TermDeposit_Trn.Trn_Date <=@asOnDate 
                //        AND TermDeposit_Master.brcode = @brCode AND TermDeposit_Master.voc_status = 'V'
                //        AND Mem_Master.brcode = @brCode
                //        AND TermDeposit_Schemes.brcode = @brCode
                //        AND TermDeposit_Master.TD_Id in 
                //            (SELECT TermDeposit_Trn.TD_Id From TermDeposit_Trn INNER JOIN TermDeposit_Master ON TermDeposit_Trn.TD_Id = TermDeposit_Master.TD_Id 
                //            Where TermDeposit_Trn.Trn_Date <=@asOnDate AND TermDeposit_Trn.TD_Delete = false
                //            AND TermDeposit_Master.TD_Delete = false AND TermDeposit_Trn.TD_Delete = false
                //            GROUP BY TermDeposit_Trn.TD_Id 
                //            HAVING Sum(TermDeposit_Trn.DepositReceiptAmount) - Sum(TermDeposit_Trn.DepositPaidAmount) >0) AND  TermDeposit_Schemes.TDSchemeType = @schemeType
                //        GROUP BY TermDeposit_Master.TDScheme_Id,TermDeposit_Schemes.TDScheme_Name,TermDeposit_Master.TD_No,TermDeposit_Master.TDH_Name, 
                //        TermDeposit_Master.ValueDate, TermDeposit_Master.DepositAmount,TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays, 
                //        TermDeposit_Master.RateOfInterest, TermDeposit_Master.MaturityDate,Mem_Master.memberNo, TermDeposit_Schemes.TDSchemeType,TermDeposit_Master.MaturityAmount
                //    Order By
                //    TermDeposit_Schemes.TDSchemeType ASC,
                //    TermDeposit_Master.TD_No ASC"
                //        , new NpgsqlParameter("@asOnDate", asOnDate)
                //        , new NpgsqlParameter("@schemeType", TDSchemeType)
                //        , new NpgsqlParameter("@brCode", brCode)).ToListAsync();
                #endregion

                #region linq
                // First, get the list of TD_Id's that have an outstanding balance.
                // This corresponds to the subquery in the SQL.
                var outstandingTDIds = CSISContext.TermDeposit_Trn
                    .Where(t => t.Trn_Date <= asOnDate && !t.TD_Delete)
                    .GroupBy(t => t.TD_Id)
                    .Where(g => g.Sum(t => t.DepositReceiptAmount) - g.Sum(t => t.DepositPaidAmount) > 0)
                    .Select(g => g.Key);

                // Now, perform the main query with the joins and filters.
                var query = CSISContext.TermDeposit_Master
                    .Where(master => master.TD_Delete == false &&
                                     master.BrCode == brCode )
                    .Join(CSISContext.TermDeposit_Trn.Where(trn => trn.Trn_Date <= asOnDate && !trn.TD_Delete),
                          master => master.TD_Id,
                          trn => trn.TD_Id,
                          (master, trn) => new { master, trn })
                    .Join(CSISContext.mem_master.Where(mem => mem.brcode == brCode),
                          combined => combined.master.Mem_Id,
                          mem => mem.mem_id,
                          (combined, mem) => new { combined.master, combined.trn, mem })
                    .Join(CSISContext.TermDeposit_Schemes.Where(scheme => scheme.BrCode == brCode && scheme.TDSchemeType == TDSchemeType),
                          combined => combined.master.TDScheme_Id,
                          scheme => scheme.TDScheme_Id,
                          (combined, scheme) => new { combined.master, combined.trn, combined.mem, scheme })
                    .Where(x => outstandingTDIds.Contains(x.master.TD_Id))
                    .GroupBy(x => new
                    {
                        x.master.TDScheme_Id,
                        x.scheme.TDScheme_Name,
                        x.master.TD_No,
                        x.master.TDH_Name,
                        x.master.ValueDate,
                        x.master.DepositAmount,
                        x.master.PeriodInMonths,
                        x.master.PeriodInDays,
                        x.master.RateOfInterest,
                        x.master.MaturityDate,
                        x.mem.memberno,
                        x.scheme.TDSchemeType,
                        x.master.MaturityAmount
                    })
                    .Select(g => new rptFDOutstanding
                    {
                        TDScheme_Id = g.Key.TDScheme_Id,
                        TDScheme_Name = g.Key.TDScheme_Name,
                        TD_No = g.Key.TD_No,
                        TDH_Name = g.Key.TDH_Name,
                        ValueDate = g.Key.ValueDate,
                        DepositAmount = g.Key.DepositAmount,
                        PeriodInMonths = g.Key.PeriodInMonths,
                        PeriodInDays = g.Key.PeriodInDays,
                        RateOfInterest = g.Key.RateOfInterest,
                        MaturityDate = g.Key.MaturityDate,
                        MemberNo = g.Key.memberno,
                        TDSchemeType = g.Key.TDSchemeType,
                        MaturityAmount = g.Key.MaturityAmount,
                        DepositReceiptAmount = g.Sum(x => x.trn.DepositReceiptAmount),
                        InterestCalculatedAmount = g.Sum(x => x.trn.InterestCalculatedAmount),
                        InterestAppliedDate = g.Max(x => x.trn.InterestAppliedDate),
                        InterestPaidAmount = g.Sum(x => x.trn.InterestPaidAmount)
                    })
                    .OrderBy(r => r.TDSchemeType)
                    .ThenBy(r => r.TD_No);

                var tdListTmp = await query.ToListAsync();
                #endregion 

                if (tdListTmp != null) tdList = tdListTmp;
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetching Term deposit outstanding data " + ex.Message);
                tdList = new();
            }
            return tdList;
        }

        public async Task<List<rptFDOutstanding>> GetTermDepositOutstandingIndividual(decimal memId, DateTime asOnDate, string TDSchemeType,string brCode)
        {
            List<rptFDOutstanding> tdList = new List<rptFDOutstanding>();
            try
            {
                #region query
                //var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptFDOutstanding>(
                //        @"SELECT
                //         Scheme.TDScheme_Id,Scheme.TDScheme_Name,
                //         Mem.memberNo, 
                //         TD.TD_No,TD.TDH_Name, TD.ValueDate, TD.DepositAmount,
                //            TD.PeriodInMonths, TD.PeriodInDays, TD.RateOfInterest, TD.MaturityDate,
                //            TD.MaturityAmount, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, Max(Trn.InterestAppliedDate) AS InterestAppliedDate ,
                //            Sum(Trn.InterestPaidAmount) AS InterestPaidAmount
                //        From
                //            TermDeposit_Master TD INNER JOIN TermDeposit_Trn Trn ON
                //                TD.TD_Id = Trn.TD_Id
                //            INNER JOIN Mem_Master Mem ON
                //                TD.Mem_Id = Mem.mem_Id
                //            INNER JOIN TermDeposit_Schemes Scheme ON
                //                TD.TDScheme_Id = Scheme.TDScheme_Id
                //        WHERE  TD.voc_status ='V' AND
                //        TD.TD_Id in 
                //        (
                //            SELECT TD.TD_Id FROM TermDeposit_Trn AS Trn INNER JOIN TermDeposit_Master AS TD ON Trn.TD_Id =  TD.TD_Id 
                //            WHERE TD.Mem_Id = @memId AND  Trn.Trn_Date <=@asOnDate AND Scheme.TDSchemeType = @schemeType AND TD.TD_Delete = FALSE AND Trn.TD_Delete = FALSE 
                //            AND TD.voc_status = 'V' AND Trn.voc_status = 'V'
                //            GROUP BY TD.TD_Id
                //            HAVING Sum(Trn.DepositReceiptAmount) - Sum(Trn.DepositPaidAmount ) >0
                //        )
                //        AND Scheme.TDSchemeType = @schemeType AND TD.TD_Delete = FALSE AND Trn.TD_Delete = FALSE 
                //         GROUP BY Scheme.TDScheme_Id,Scheme.TDScheme_Name,
                //         Mem.memberNo,  TD.TD_No,TD.TDH_Name, TD.ValueDate, TD.DepositAmount,   TD.PeriodInMonths, TD.PeriodInDays, TD.RateOfInterest, TD.MaturityDate,TD.MaturityAmount
                //        ORDER BY Scheme.TDScheme_Id, TD.TD_No "
                //    , new NpgsqlParameter("@memId", memId)
                //    , new NpgsqlParameter("@asOnDate", asOnDate)
                //    , new NpgsqlParameter("@schemeType", TDSchemeType)).ToListAsync();
                #endregion

                #region linq
                // First, get the subquery for TD_Ids with outstanding amounts
                var outstandingTdIds = await (from trn in CSISContext.TermDeposit_Trn
                                              join td in CSISContext.TermDeposit_Master on trn.TD_Id equals td.TD_Id
                                              join scheme in CSISContext.TermDeposit_Schemes on td.TDScheme_Id equals scheme.TDScheme_Id
                                              where td.Mem_Id == memId
                                                 && trn.BrCode == brCode 
                                                 && td.BrCode == brCode 
                                                 && td.BrCode == brCode
                                                 && trn.Trn_Date <= asOnDate
                                                 && scheme.TDSchemeType == TDSchemeType
                                                 && td.TD_Delete == false
                                                 && trn.TD_Delete == false
                                              group trn by td.TD_Id into g
                                              where g.Sum(x => x.DepositReceiptAmount) - g.Sum(x => x.DepositPaidAmount) > 0
                                              select g.Key).ToListAsync();

                // Main query using the outstanding TD_Ids
                var tdListTmp = await (from td in CSISContext.TermDeposit_Master
                                       join trn in CSISContext.TermDeposit_Trn on td.TD_Id equals trn.TD_Id
                                       join mem in CSISContext.mem_master on td.Mem_Id equals mem.mem_id
                                       join scheme in CSISContext.TermDeposit_Schemes on td.TDScheme_Id equals scheme.TDScheme_Id
                                       where outstandingTdIds.Contains(td.TD_Id)
                                          && scheme.TDSchemeType == TDSchemeType
                                          && td.TD_Delete == false
                                          && trn.TD_Delete == false
                                          && td.BrCode == brCode
                                          && trn.BrCode == brCode 
                                          && mem.brcode == brCode 
                                          && scheme.BrCode == brCode 
                                       group trn by new
                                       {
                                           scheme.TDScheme_Id,
                                           scheme.TDScheme_Name,
                                           mem.memberno,
                                           td.TD_No,
                                           td.TDH_Name,
                                           td.ValueDate,
                                           td.DepositAmount,
                                           td.PeriodInMonths,
                                           td.PeriodInDays,
                                           td.RateOfInterest,
                                           td.MaturityDate,
                                           td.MaturityAmount
                                       } into g
                                       orderby g.Key.TDScheme_Id, g.Key.TD_No
                                       select new rptFDOutstanding
                                       {
                                           TDScheme_Id = g.Key.TDScheme_Id,
                                           TDScheme_Name = g.Key.TDScheme_Name,
                                           MemberNo = g.Key.memberno,
                                           TD_No = g.Key.TD_No,
                                           TDH_Name = g.Key.TDH_Name,
                                           ValueDate = g.Key.ValueDate,
                                           DepositAmount = g.Key.DepositAmount,
                                           PeriodInMonths = g.Key.PeriodInMonths,
                                           PeriodInDays = g.Key.PeriodInDays,
                                           RateOfInterest = g.Key.RateOfInterest,
                                           MaturityDate = g.Key.MaturityDate,
                                           MaturityAmount = g.Key.MaturityAmount,
                                           InterestCalculatedAmount = g.Sum(x => x.InterestCalculatedAmount),
                                           InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                                           InterestPaidAmount = g.Sum(x => x.InterestPaidAmount)
                                       }).ToListAsync();
                #endregion 
                if (tdListTmp != null) { tdList = tdListTmp; }
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<List<rptTermDepositPayable>> GetTermDepositPayable(DateTime asOnDate, string tdSchemeType,string brCode)
        {
            List<rptTermDepositPayable> tdList = new List<rptTermDepositPayable>();
            try
            {
                #region query
                //var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositPayable>(
                //        @"With Db1
                //        AS
                //        (
                //        select Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate,
                //            Max(Trn.Trn_Date) AS Trn_Date, SUM(Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                //            Max(Trn.InterestAppliedDate) AS InterestAppliedDate, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount,
                //            Sum(Trn.InterestPaidAmount) AS InterestPaidAmount, Sum(Trn.InterestCalculatedAmount - Trn.InterestPaidAmount) AS IntPayable
                //            FROM TermDeposit_Trn AS Trn
                //            INNER JOIN TermDeposit_Master AS Mas ON Trn.TD_Id = Mas.TD_Id
                //            WHERE Trn.TD_Delete = FALSE AND Mas.td_delete = FALSE
                //            AND Trn.voc_status ='V' AND Mas.voc_status ='V' 
                //            and  CAST(Trn.Trn_Date AS date) <= @asOnDate
                //            GROUP BY Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate
                //            HAVING Sum(Trn.InterestCalculatedAmount - Trn.InterestPaidAmount) > 0
                //        UNION
                //            SELECT Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate,
                //            Max(Trn.Trn_Date) AS Trn_Date, Sum(Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                //            Max(Trn.InterestAppliedDate) AS InterestAppliedDate, 0 :: DOUBLE PRECISION AS InterestCalculatedAmount,
                //            0 :: DOUBLE PRECISION AS InterestPaidAmount, 0 :: DOUBLE PRECISION AS IntPayable
                //            FROM TermDeposit_Master AS Mas
                //            INNER JOIN TermDeposit_Trn AS Trn ON Mas.TD_Id = Trn.TD_Id
                //            WHERE  Mas.voc_status = 'V' AND Trn.voc_status = 'V' AND Mas.MaturityDate :: DATE <= @asOnDate AND Trn.TD_Delete = FALSE AND Mas.TD_Delete = FALSE
                //            GROUP BY Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TD_No, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate
                //            HAVING Sum(Trn.DepositReceiptAmount) - Sum(Trn.DepositPaidAmount) > 0
                //        )
                //        SELECT Db1.TD_Id, Db1.TD_No,Db1.ValueDate,Db1.PeriodInMonths,Db1.PeriodInDays,Db1.RateOfInterest, Db1.TDScheme_Id, Db1.TDH_Name, Db1.DepositAmount, Db1.MaturityDate, Db1.Trn_Date,
                //            Sum(Db1.DepositReceiptAmount) AS DepositReceiptAmount,
                //            Sum(Db1.InterestCalculatedAmount) AS InterestCalculatedAmount, Sum(Db1.InterestPaidAmount) AS InterestPaidAmount,
                //            Sum(Db1.IntPayable) AS IntPayable, Max(Db1.InterestAppliedDate) AS InterestAppliedDate,
                //            Sch.TDScheme_Name, Sum(Db1.IntPayable) AS TotalPayable
                //        FROM Db1
                //            INNER JOIN TermDeposit_Schemes AS Sch ON Db1.TDScheme_Id = Sch.TDScheme_Id
                //            GROUP BY Db1.TD_Id, Db1.TD_No,Db1.ValueDate,Db1.PeriodInMonths,Db1.PeriodInDays,Db1.RateOfInterest, Db1.TDScheme_Id, Db1.TDH_Name, Db1.DepositAmount, Db1.MaturityDate, Db1.Trn_Date, Sch.TDScheme_Name
                //            HAVING Sum(Db1.IntPayable) > 0
                //            ORDER BY Db1.TDScheme_Id, Db1.TD_No"
                //            , new NpgsqlParameter("@asOnDate", asOnDate)).ToListAsync();
                #endregion

                #region linq
                // First part of the UNION - Interest Payable records
                var db1Part1 = from trn in CSISContext.TermDeposit_Trn
                               join mas in CSISContext.TermDeposit_Master on trn.TD_Id equals mas.TD_Id
                               join scheme in CSISContext.TermDeposit_Schemes on mas.TDScheme_Id equals scheme.TDScheme_Id
                               where trn.TD_Delete == false
                                  && mas.TD_Delete == false
                                  && trn.Trn_Date <= asOnDate.Date
                                  && scheme.TDSchemeType == tdSchemeType
                               group trn by new
                               {
                                   trn.TD_Id,
                                   mas.TD_No,
                                   mas.ValueDate,
                                   mas.PeriodInMonths,
                                   mas.PeriodInDays,
                                   mas.TDScheme_Id,
                                   mas.TDH_Name,
                                   mas.DepositAmount,
                                   mas.RateOfInterest,
                                   mas.MaturityDate
                               } into g
                               where g.Sum(x => x.InterestCalculatedAmount - x.InterestPaidAmount) > 0
                               select new
                               {
                                   TD_Id = g.Key.TD_Id,
                                   TD_No = g.Key.TD_No,
                                   ValueDate = g.Key.ValueDate,
                                   PeriodInMonths = g.Key.PeriodInMonths,
                                   PeriodInDays = g.Key.PeriodInDays,
                                   TDScheme_Id = g.Key.TDScheme_Id,
                                   TDH_Name = g.Key.TDH_Name,
                                   DepositAmount = g.Key.DepositAmount,
                                   RateOfInterest = g.Key.RateOfInterest,
                                   MaturityDate = g.Key.MaturityDate,
                                   Trn_Date = g.Max(x => x.Trn_Date),
                                   DepositReceiptAmount = g.Sum(x => x.DepositReceiptAmount),
                                   InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                                   InterestCalculatedAmount = g.Sum(x => x.InterestCalculatedAmount),
                                   InterestPaidAmount = g.Sum(x => x.InterestPaidAmount),
                                   IntPayable = g.Sum(x => x.InterestCalculatedAmount - x.InterestPaidAmount)
                               };

                // Second part of the UNION - Maturity records
                var db1Part2 = from mas in CSISContext.TermDeposit_Master
                               join trn in CSISContext.TermDeposit_Trn on mas.TD_Id equals trn.TD_Id
                               join scheme in CSISContext.TermDeposit_Schemes on mas.TDScheme_Id equals scheme.TDScheme_Id
                               where mas.MaturityDate.Date <= asOnDate.Date
                                  && trn.TD_Delete == false
                                  && mas.TD_Delete == false
                                  && scheme.TDSchemeType == tdSchemeType 
                               group trn by new
                               {
                                   trn.TD_Id,
                                   mas.TD_No,
                                   mas.ValueDate,
                                   mas.PeriodInMonths,
                                   mas.PeriodInDays,
                                   mas.TDScheme_Id,
                                   mas.TDH_Name,
                                   mas.DepositAmount,
                                   mas.RateOfInterest,
                                   mas.MaturityDate
                               } into g
                               where g.Sum(x => x.DepositReceiptAmount) - g.Sum(x => x.DepositPaidAmount) > 0
                               select new
                               {
                                   TD_Id = g.Key.TD_Id,
                                   TD_No = g.Key.TD_No,
                                   ValueDate = g.Key.ValueDate,
                                   PeriodInMonths = g.Key.PeriodInMonths,
                                   PeriodInDays = g.Key.PeriodInDays,
                                   TDScheme_Id = g.Key.TDScheme_Id,
                                   TDH_Name = g.Key.TDH_Name,
                                   DepositAmount = g.Key.DepositAmount,
                                   RateOfInterest = g.Key.RateOfInterest,
                                   MaturityDate = g.Key.MaturityDate,
                                   Trn_Date = g.Max(x => x.Trn_Date),
                                   DepositReceiptAmount = g.Sum(x => x.DepositReceiptAmount),
                                   InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                                   InterestCalculatedAmount = 0.0,
                                   InterestPaidAmount = 0.0,
                                   IntPayable = 0.0
                               };

                // Union the two parts (equivalent to the CTE)
                var db1 = db1Part1.Union(db1Part2);

                // Final query joining with schemes and grouping
                var tdListTmp = await (from d in db1
                                       join sch in CSISContext.TermDeposit_Schemes on d.TDScheme_Id equals sch.TDScheme_Id
                                       group d by new
                                       {
                                           d.TD_Id,
                                           d.TD_No,
                                           d.ValueDate,
                                           d.PeriodInMonths,
                                           d.PeriodInDays,
                                           d.RateOfInterest,
                                           d.TDScheme_Id,
                                           d.TDH_Name,
                                           d.DepositAmount,
                                           d.MaturityDate,
                                           d.Trn_Date,
                                           sch.TDScheme_Name
                                       } into g
                                       where g.Sum(x => x.IntPayable) > 0
                                       orderby g.Key.TDScheme_Id, g.Key.TD_No
                                       select new rptTermDepositPayable
                                       {
                                           TD_Id = g.Key.TD_Id,
                                           TD_No = g.Key.TD_No,
                                           ValueDate = g.Key.ValueDate,
                                           PeriodInMonths = g.Key.PeriodInMonths,
                                           PeriodInDays = g.Key.PeriodInDays,
                                           RateOfInterest = g.Key.RateOfInterest,
                                           TDScheme_Id = g.Key.TDScheme_Id,
                                           TDH_Name = g.Key.TDH_Name,
                                           DepositAmount = g.Key.DepositAmount,
                                           MaturityDate = g.Key.MaturityDate,
                                           Trn_Date = g.Key.Trn_Date!,
                                           DepositReceiptAmount = g.Sum(x => x.DepositReceiptAmount),
                                           InterestCalculatedAmount = g.Sum(x => x.InterestCalculatedAmount),
                                           InterestPaidAmount = g.Sum(x => x.InterestPaidAmount),
                                           IntPayable = g.Sum(x => x.IntPayable),
                                           InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                                           TDScheme_Name = g.Key.TDScheme_Name,
                                           TotalPayable = g.Sum(x => x.IntPayable)
                                       }).ToListAsync();

                #endregion 
                if (tdListTmp != null) { tdList = tdListTmp; }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in fetching term deposit payable data " + ex.Message );
                tdList = new();
            }
            return tdList;
        }

        public async Task<List<rptTermDepositPayable>> GetTermDepositMaturityPayable(DateTime asOnDate, string tdSchemeType, string brCode)
        {
            List<rptTermDepositPayable> tdList = new List<rptTermDepositPayable>();
            try
            {
                #region query
                //         if (tdSchemeType == "F")
                //         {
                //             var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositPayable>(
                //                 @" SELECT Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id,Mas.TDH_Name,Sch.TDSchemeType,Sch.TDScheme_Name ,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate, 
                //                     Max(Trn.Trn_Date) AS Trn_Date, Sum(Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                //                     Max(Trn.InterestAppliedDate) AS InterestAppliedDate, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, 
                //                     Sum(Trn.InterestPaidAmount) AS InterestPaidAmount, Sum(Trn.InterestCalculatedAmount) - Sum(Trn.InterestPaidAmount) AS IntPayable ,
                //                     Sum(Trn.DepositReceiptAmount) + (Sum(Trn.InterestCalculatedAmount) - Sum(Trn.InterestPaidAmount) ) AS TotalPayable
                //   FROM TermDeposit_Master AS Mas
                //                     INNER JOIN TermDeposit_Trn AS Trn ON Mas.TD_Id = Trn.TD_Id 
                //INNER JOIN TermDeposit_Schemes AS Sch ON Mas.TDScheme_Id = Sch.TDScheme_Id 
                //                     WHERE Mas.brcode = @brCode AND Mas.voc_status = 'V' 
                //                     AND Trn.brcode = @brCode AND Trn.voc_status ='V'
                //                     AND Sch.brcode =@brCode
                //                     AND Mas.MaturityDate :: DATE <=@asOnDate AND Trn.TD_Delete = FALSE AND Mas.TD_Delete = FALSE AND Sch.TDSchemeType = @tdSchemeType
                //                     GROUP BY Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays,Mas.TDScheme_Id,Sch.TDSchemeType,Sch.TDScheme_Name,Mas.TD_No, Mas.TDH_Name,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate
                //                     HAVING Sum(Trn.DepositReceiptAmount)- Sum(Trn.DepositPaidAmount) >0 
                //ORDER BY Mas.TDScheme_Id,Mas.TD_No "
                //                 , new NpgsqlParameter("@asOnDate", asOnDate.Date)
                //                 , new NpgsqlParameter("@tdSchemeType", tdSchemeType)
                //                 , new NpgsqlParameter("@brCode", brCode)).ToListAsync();
                //             if (tdListTmp != null) { tdList = tdListTmp; }
                //         }
                //         if (tdSchemeType == "R")
                //         {
                //             var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositPayable>(
                //                 @" SELECT Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id,Mas.TDH_Name,Sch.TDSchemeType,Sch.TDScheme_Name ,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate, 
                //                     Max(Trn.Trn_Date) AS Trn_Date, Sum(Trn.DepositReceiptAmount) AS DepositReceiptAmount,Sum(Trn.DepositPaidAmount) AS DepositPaidAmount,
                //                     Max(Trn.InterestAppliedDate) AS InterestAppliedDate, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, 
                //                     Sum(Trn.InterestPaidAmount) AS InterestPaidAmount, (Mas.MaturityAmount) - (Mas.DepositAmount  * Mas.PeriodInMonths)  AS IntPayable ,
                //                     Mas.MaturityAmount AS TotalPayable
                //   FROM TermDeposit_Master AS Mas
                //                     INNER JOIN TermDeposit_Trn AS Trn ON Mas.TD_Id = Trn.TD_Id 
                //INNER JOIN TermDeposit_Schemes AS Sch ON Mas.TDScheme_Id = Sch.TDScheme_Id 
                //                     WHERE  Mas.brcode = @brCode AND Mas.voc_status = 'V' 
                //                     AND Trn.brcode = @brCode AND Trn.voc_status ='V'
                //                     AND Sch.brcode =@brCode
                //                     and Mas.MaturityDate :: DATE <=@asOnDate AND Trn.TD_Delete = FALSE AND Mas.TD_Delete = FALSE AND Sch.TDSchemeType = @tdSchemeType
                //                     GROUP BY Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays,Mas.TDScheme_Id,Sch.TDSchemeType,Sch.TDScheme_Name,Mas.TD_No, Mas.TDH_Name,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate,Mas.MaturityAmount
                //                     HAVING  Sum(Trn.DepositPaidAmount) = 0
                //ORDER BY Mas.TDScheme_Id,Mas.TD_No  "
                //                 , new NpgsqlParameter("@asOnDate", asOnDate.Date)
                //                 , new NpgsqlParameter("@tdSchemeType", tdSchemeType)).ToListAsync();
                //             if (tdListTmp != null) { tdList = tdListTmp; }
                //         }
                #endregion

                #region linq
                var tdListTmp = await (from mas in CSISContext.TermDeposit_Master
                                       join trn in CSISContext.TermDeposit_Trn on mas.TD_Id equals trn.TD_Id
                                       join sch in CSISContext.TermDeposit_Schemes on mas.TDScheme_Id equals sch.TDScheme_Id
                                       where mas.BrCode == brCode
                                          && trn.BrCode == brCode
                                          && sch.BrCode == brCode
                                          && mas.MaturityDate.Date <= asOnDate.Date
                                          && trn.TD_Delete == false
                                          && mas.TD_Delete == false
                                          && sch.TDSchemeType == tdSchemeType
                                       group trn by new
                                       {
                                           trn.TD_Id,
                                           mas.TD_No,
                                           mas.ValueDate,
                                           mas.PeriodInMonths,
                                           mas.PeriodInDays,
                                           mas.TDScheme_Id,
                                           sch.TDSchemeType,
                                           sch.TDScheme_Name,
                                           mas.TDH_Name,
                                           mas.DepositAmount,
                                           mas.RateOfInterest,
                                           mas.MaturityDate
                                       } into g
                                       where g.Sum(x => x.DepositReceiptAmount) - g.Sum(x => x.DepositPaidAmount) > 0
                                       orderby g.Key.TDScheme_Id, g.Key.TD_No
                                       select new rptTermDepositPayable
                                       {
                                           TD_Id = g.Key.TD_Id,
                                           TD_No = g.Key.TD_No,
                                           ValueDate = g.Key.ValueDate,
                                           PeriodInMonths = g.Key.PeriodInMonths,
                                           PeriodInDays = g.Key.PeriodInDays,
                                           TDScheme_Id = g.Key.TDScheme_Id,
                                           TDH_Name = g.Key.TDH_Name,
                                           TDSchemeType = g.Key.TDSchemeType,
                                           TDScheme_Name = g.Key.TDScheme_Name,
                                           DepositAmount = g.Key.DepositAmount,
                                           RateOfInterest = g.Key.RateOfInterest,
                                           MaturityDate = g.Key.MaturityDate,
                                           Trn_Date = g.Max(x => x.Trn_Date),
                                           DepositReceiptAmount = g.Sum(x => x.DepositReceiptAmount),
                                           InterestAppliedDate = g.Max(x => x.InterestAppliedDate),
                                           InterestCalculatedAmount = g.Sum(x => x.InterestCalculatedAmount),
                                           InterestPaidAmount = g.Sum(x => x.InterestPaidAmount),
                                           IntPayable = g.Sum(x => x.InterestCalculatedAmount) - g.Sum(x => x.InterestPaidAmount),
                                           TotalPayable = g.Sum(x => x.DepositReceiptAmount) + (g.Sum(x => x.InterestCalculatedAmount) - g.Sum(x => x.InterestPaidAmount))
                                       }).ToListAsync();
                if (tdListTmp != null && tdListTmp.Any()) tdList = tdListTmp.ToList();
                #endregion 
            }
            catch (Exception ex)
            {
                Console.Write ("Error in fetching maturity payable data " + ex.Message);
                tdList = new();
            }
            return tdList;
        }

        //  public async Task<rptFDBond> GetFDBondPreprinted(decimal vocId)
        //  {
        //      rptFDBond fDBond = new rptFDBond();
        //      try
        //      {
        //          fDBond = await GetFDBondData(vocId);

        //          switch (fDBond.InterestPayableFrequency)
        //          {
        //              case 1:
        //                  fDBond.TDFrequency = "Monthly";
        //                  break;
        //              case 3:
        //                  fDBond.TDFrequency = "Quarterly";
        //                  break;
        //              case 6:
        //                  fDBond.TDFrequency = "Half Yearly";
        //                  break;
        //              case 12:
        //                  fDBond.TDFrequency = "Annual";
        //                  break;
        //              case 0:
        //                  fDBond.TDFrequency = "On Maturity";
        //                  break;
        //          }

        //          switch (fDBond.ModeOfOperation)
        //          {
        //              case 1:
        //                  fDBond.ModeOfOperationString = "Single";
        //                  break;
        //              case 2:
        //                  fDBond.ModeOfOperationString = "Either or Survivor";
        //                  break;
        //              case 3:
        //                  fDBond.ModeOfOperationString = "Any one or Survivor/s";
        //                  break;
        //              case 4:
        //                  fDBond.ModeOfOperationString = "Any Two or Survivior/s";
        //                  break;
        //              case 5:
        //                  fDBond.ModeOfOperationString = "All Parents Jointly";
        //                  break;
        //              case 6:
        //                  fDBond.ModeOfOperationString = "Any two jointly";
        //                  break;
        //              case 7:
        //                  fDBond.ModeOfOperationString = "Former or Survivior";
        //                  break;
        //              case 8:
        //                  fDBond.ModeOfOperationString = "Later or Survivor";
        //                  break;
        //              case 9:
        //                  fDBond.ModeOfOperationString = "By Guardian";
        //                  break;
        //              case 10:
        //                  fDBond.ModeOfOperationString = "By Mandate";
        //                  break;
        //              case 11:
        //                  fDBond.ModeOfOperationString = "Power of attorney";
        //                  break;
        //              case 12:
        //                  fDBond.ModeOfOperationString = "Proprietor";
        //                  break;
        //          }
        //          //fDBond.RsInWords = Utilities.RupeesInWords(fDBond.DepositAmount);
        //          fDBond.RateOfInterest = fDBond.RateOfInterest / 100;
        //      }
        //      catch (Exception)
        //      {
        //          throw;
        //      }
        //      return fDBond;
        //  }

        //  private async Task<rptFDBond> GetFDBondData(decimal vocId)
        //  {
        //      rptFDBond _bond = new rptFDBond();
        //      try
        //      {

        //          var _bondTmp = await CSISContext.Database.SqlQueryRaw<rptFDBond>(
        //              @"SELECT
        //                  TermDeposit_Master.TD_No, TermDeposit_Master.TDH_Name, TermDeposit_Master.AccountOpenDate, 
        //                  TermDeposit_Master.ValueDate,TermDeposit_Master.DepositAmount, TermDeposit_Master.PeriodInMonths, 
        //                  TermDeposit_Master.PeriodInDays, TermDeposit_Master.RateOfInterest, 
        //                  TermDeposit_Master.MaturityDate, TermDeposit_Master.MaturityAmount, TermDeposit_Master.InterestPayableFrequency, 
        //                  TermDeposit_Master.Nominee1Name, TermDeposit_Master.Nominee1Age, TermDeposit_Master.Nominee1Relationship, 
        //                  TermDeposit_Master.Nominee2Name, TermDeposit_Master.Nominee2Age, TermDeposit_Master.Nominee2Relationship, 
        //                  TermDeposit_Schemes.TDScheme_Name, TermDeposit_Master.RenewalTD_No,
        //                  Fin_Voucher.voc_rpt_No, Fin_Voucher.Voc_No,
        //                  Mem_Master.memberNo,Mem_Master.memberName,Mem_Master.mobileNo, 
        //                  CONCAT (
        //    COALESCE(Mem_Master.PreAdd1, ''),
        //    ',',
        //    COALESCE(Mem_Master.PreAdd2 || ',', ''),
        //    COALESCE(Mem_Master.PreAdd3 || ',', ''),
        //    COALESCE(Mem_Master.PrePin, '') 
        //                  ) AS Address,
        //              ModeOfOperation 
        //              From 
        //                  TermDeposit_Master TermDeposit_Master INNER JOIN TermDeposit_Schemes TermDeposit_Schemes ON 
        //                      TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
        //                   INNER JOIN Mem_Master Mem_Master ON
        //                      TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
        //                   LEFT OUTER JOIN Fin_Voucher Fin_Voucher ON
        //                      TermDeposit_Master.Voc_Id = Fin_Voucher.Voc_Id
        //                  WHERE TermDeposit_Master.Voc_Id = @vocId
        //                  AND TermDeposit_Master.td_delete = FALSE
        //AND Fin_Voucher.voc_delete = FALSE AND
        //                  TermDeposit_Master.voc_status ='V' AND Fin_Voucher.voc_status ='V'"
        //              , new NpgsqlParameter("@vocId", vocId)).FirstOrDefaultAsync();
        //          if (_bondTmp != null) { _bond = _bondTmp; }

        //      }
        //      catch (Exception)
        //      {
        //      }
        //      return _bond;
        //  }

        public async Task<List<rptTDNewBetweenDates>> GetTermDepositReceivedDuringPeriod(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode)
        {
            List<rptTDNewBetweenDates> tdList = new List<rptTDNewBetweenDates>();
            try
            {
                tdList = await GetTermDepositrReceivedDuringPeriodData(fromDate, toDate, TDSchemeType, brCode);
                foreach (var td in tdList)
                {
                    switch (td.InterestPayableFrequency)
                    {
                        case 0:
                            td.TDFrequency = "On Maturity";
                            break;
                        case 1:
                            td.TDFrequency = "Monthly";
                            break;
                        case 3:
                            td.TDFrequency = "Quarterly";
                            break;
                        case 6:
                            td.TDFrequency = "Half Yealy";
                            break;
                        case 12:
                            td.TDFrequency = "Annual";
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        private async Task<List<rptTDNewBetweenDates>> GetTermDepositrReceivedDuringPeriodData(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode)
        {
            List<rptTDNewBetweenDates> tdList = new();
            //TransactionOptions options = new TransactionOptions();
            //options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            //options.Timeout = new TimeSpan(5, 00, 0);
            try
            {
                #region query
                //          var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTDNewBetweenDates>(
                //              @"SELECT
                //                  TermDeposit_Master.TDScheme_Id,
                //                  TermDeposit_Schemes.TDScheme_Name,
                //                  TermDeposit_Schemes.TDSchemeType,
                //                  TermDeposit_Master.TD_No,
                //                  Mem_Master.memberNo, 
                //                  Mem_Master.memberName,
                //                  TermDeposit_Master.TDH_Name, 
                //TermDeposit_Master.AccountOpenDate,
                //                  TermDeposit_Master.ValueDate, 
                //                  TermDeposit_Master.DepositAmount,
                //                  TermDeposit_Master.PeriodInMonths, 
                //                  TermDeposit_Master.PeriodInDays, 
                //                  TermDeposit_Master.RateOfInterest, 
                //                  TermDeposit_Master.InterestPayableFrequency,
                //                  TermDeposit_Master.MaturityDate,
                //                  TermDeposit_Master.MaturityAmount
                //              From
                //                  TermDeposit_Master  INNER JOIN TermDeposit_Trn TermDeposit_Trn ON TermDeposit_Master.Voc_Id = TermDeposit_Trn.Voc_Id
                //                  INNER JOIN Mem_Master  ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
                //                  INNER JOIN TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                //                  WHERE TermDeposit_Master.TD_Delete = FALSE AND TermDeposit_Trn.TD_Delete= FALSE
                //                  AND TermDeposit_Master.brcode = @brCode AND TermDeposit_Master.voc_status ='V'
                //                  AND TermDeposit_Trn.brcode = @brCode AND TermDeposit_Trn.voc_status ='V'
                //                  AND Mem_Master.brcode = @brCode 
                //                  AND TermDeposit_Schemes.brcode = @brCode
                //                  AND TermDeposit_Master.AccountOpenDate BETWEEN @fromDate AND @toDate 
                //                  AND TermDeposit_Schemes.TDSchemeType = @schemeType
                //                  GROUP BY TermDeposit_Master.TDScheme_Id,TermDeposit_Schemes.TDScheme_Name,TermDeposit_Schemes.TDSchemeType,TermDeposit_Master.TD_No,
                //                  Mem_Master.memberNo, Mem_Master.memberName,TermDeposit_Master.TDH_Name, 
                //                  TermDeposit_Master.AccountOpenDate,TermDeposit_Master.ValueDate, TermDeposit_Master.DepositAmount,TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays, 
                //                  TermDeposit_Master.RateOfInterest, TermDeposit_Master.InterestPayableFrequency,TermDeposit_Master.MaturityDate, TermDeposit_Master.MaturityAmount
                //HAVING Sum(TermDeposit_Trn.DepositPaidAmount) = 0
                //              Order By
                //              TermDeposit_Schemes.TDSchemeType ASC,
                //              TermDeposit_Master.TD_No ASC"
                //          , new NpgsqlParameter("@fromDate", fromDate)
                //          , new NpgsqlParameter("@toDate", toDate)
                //          , new NpgsqlParameter("@schemeType", TDSchemeType)).ToListAsync();
                #endregion

                #region linq
                var result = await (from tdMaster in CSISContext.TermDeposit_Master
                                    join tdTrn in CSISContext.TermDeposit_Trn on tdMaster.Voc_Id equals tdTrn.Voc_Id
                                    join mem in CSISContext.mem_master on tdMaster.Mem_Id equals mem.mem_id
                                    join tdScheme in CSISContext.TermDeposit_Schemes on tdMaster.TDScheme_Id equals tdScheme.TDScheme_Id
                                    where tdMaster.TD_Delete == false
                                       && tdTrn.TD_Delete == false
                                       && tdMaster.BrCode == brCode
                                       && tdTrn.BrCode == brCode
                                       && mem.brcode == brCode
                                       && tdScheme.BrCode == brCode
                                       && tdMaster.AccountOpenDate >= fromDate 
                                       && tdMaster.AccountOpenDate <= toDate 
                                       && tdScheme.TDSchemeType == TDSchemeType
                                    group tdTrn by new
                                    {
                                        tdMaster.TDScheme_Id,
                                        tdScheme.TDScheme_Name,
                                        tdScheme.TDSchemeType,
                                        tdMaster.TD_No,
                                        mem.memberno,
                                        mem.membername,
                                        tdMaster.TDH_Name,
                                        tdMaster.AccountOpenDate,
                                        tdMaster.ValueDate,
                                        tdMaster.DepositAmount,
                                        tdMaster.PeriodInMonths,
                                        tdMaster.PeriodInDays,
                                        tdMaster.RateOfInterest,
                                        tdMaster.InterestPayableFrequency,
                                        tdMaster.MaturityDate,
                                        tdMaster.MaturityAmount
                                    } into g
                                    where g.Sum(x => x.DepositPaidAmount) == 0
                                    orderby g.Key.TDSchemeType ascending, g.Key.TD_No ascending
                                    select new rptTDNewBetweenDates // Replace with your actual class name
                                    {
                                        TDScheme_Id = g.Key.TDScheme_Id,
                                        TDScheme_Name = g.Key.TDScheme_Name,
                                        TDSchemeType = g.Key.TDSchemeType,
                                        TD_No = g.Key.TD_No,
                                        MemberNo = g.Key.memberno,
                                        MemberName = g.Key.membername,
                                        TDH_Name = g.Key.TDH_Name,
                                        AccountOpenDate = g.Key.AccountOpenDate,
                                        ValueDate = g.Key.ValueDate,
                                        DepositAmount = g.Key.DepositAmount,
                                        PeriodInMonths = g.Key.PeriodInMonths,
                                        PeriodInDays = g.Key.PeriodInDays,
                                        RateOfInterest = g.Key.RateOfInterest,
                                        InterestPayableFrequency = g.Key.InterestPayableFrequency,
                                        MaturityDate = g.Key.MaturityDate,
                                        MaturityAmount = g.Key.MaturityAmount
                                    }).ToListAsync();
                #endregion 

                if (result != null && result.Any()) { tdList = result.ToList() ; }
            }
            catch (Exception ex)
            {
                Console.Write("Error in fetching term deposit received during the period " + ex.Message);
                tdList = new();
            }
            return tdList;
        }

        public async Task<List<rptTDRefundBetweenDates>> GetFDRefundBetweenDated(DateTime fromDate, DateTime toDate, string TDSchemeType, string brCode)
        {
            List<rptTDRefundBetweenDates> tdList = new List<rptTDRefundBetweenDates>();
            try
            {
                decimal TDLedId = CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.FD_Led_Id).First();

                #region query
                //var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTDRefundBetweenDates>(
                //            @"SELECT TermDeposit_Trn.Trn_Date, TermDeposit_Trn.Voc_Id, TermDeposit_Master.TD_Id, TermDeposit_Master.TD_No, TermDeposit_Master.TDScheme_Id, TermDeposit_Schemes.TDScheme_Name, TermDeposit_Master.Mem_Id, 
                //              Mem_Master.memberNo, Mem_Master.memberName, TermDeposit_Trn.DepositPaidAmount
                //            FROM TermDeposit_Master INNER JOIN
                //                TermDeposit_Trn ON TermDeposit_Master.TD_Id = TermDeposit_Trn.TD_Id INNER JOIN
                //                Fin_voucher_trn ON TermDeposit_Trn.Voc_Id = Fin_voucher_trn.voc_id INNER JOIN
                //                Mem_Master ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id INNER JOIN
                //                TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                //            WHERE (Fin_voucher_trn.Status = 'FR') AND (TermDeposit_Trn.DepositPaidAmount > 0) AND (TermDeposit_Master.TD_Delete = FALSE) 
                //          AND (TermDeposit_Trn.TD_Delete = FALSE) 
                //          AND (TermDeposit_Trn.Trn_Date >= @fromDate AND TermDeposit_Trn.Trn_Date <=  @toDate)
                //          AND (Fin_voucher_trn.voc_pmt >0)
                //          AND (Fin_voucher_trn.led_id = @tdLedId)
                //                AND  TermDeposit_Schemes.TDSchemeType = @schemeType
                //                AND TermDeposit_Master.brcode = @brCode AND TermDeposit_Master.voc_status = 'V'
                //                AND TermDeposit_Trn.brcode = @brCode AND TermDeposit_Trn.voc_status = 'V'
                //                AND Fin_Voucher_Trn.brcode = @brcode AND Fin_voucher_trn.voc_status = 'V'
                //                AND Mem_Master.brcode = @brCode
                //                AND TermDeposit_Schemes.brcode = @brCode
                //            ORDER BY TermDeposit_Trn.Trn_Date, TermDeposit_Master.TD_No"
                //        , new NpgsqlParameter("@fromDate", fromDate)
                //        , new NpgsqlParameter("@toDate", toDate)
                //        , new NpgsqlParameter("@schemeType", TDSchemeType)
                //        , new NpgsqlParameter("@tdLedId", TDLedId)
                //        , new NpgsqlParameter("@brcode", brCode)).ToListAsync();
                #endregion

                #region linq
                var result = await (from tdMaster in CSISContext.TermDeposit_Master
                join tdTrn in CSISContext.TermDeposit_Trn on tdMaster.TD_Id equals tdTrn.TD_Id
                join finVoucherTrn in CSISContext.Fin_Voucher_Trn on tdTrn.Voc_Id equals finVoucherTrn.Voc_Id
                join mem in CSISContext.mem_master on tdMaster.Mem_Id equals mem.mem_id
                join tdScheme in CSISContext.TermDeposit_Schemes on tdMaster.TDScheme_Id equals tdScheme.TDScheme_Id
                where finVoucherTrn.Status == "FR"
                   && tdTrn.DepositPaidAmount > 0
                   && tdMaster.TD_Delete == false
                   && tdTrn.TD_Delete == false
                   && tdTrn.Trn_Date >= fromDate 
                   && tdTrn.Trn_Date <= toDate 
                   && finVoucherTrn.Voc_Pmt > 0
                   && finVoucherTrn.Led_Id == TDLedId
                   && tdScheme.TDSchemeType == TDSchemeType
                   && tdMaster.BrCode == brCode
                   && tdTrn.BrCode == brCode
                   && finVoucherTrn.BrCode == brCode
                   && mem.brcode == brCode
                   && tdScheme.BrCode == brCode
                    orderby tdTrn.Trn_Date, tdMaster.TD_No
                    select new rptTDRefundBetweenDates // Replace with your actual class name
                    {
                        Trn_Date = tdTrn.Trn_Date,
                        Voc_Id = tdTrn.Voc_Id,
                        TD_Id = tdMaster.TD_Id,
                        TD_No = tdMaster.TD_No,
                        TDScheme_Id = tdMaster.TDScheme_Id,
                        TDScheme_Name = tdScheme.TDScheme_Name,
                        Mem_Id = tdMaster.Mem_Id,
                        MemberNo = mem.memberno,
                        MemberName = mem.membername,
                        DepositPaidAmount = tdTrn.DepositPaidAmount
                    }).ToListAsync();

                #endregion 
                if (result != null && result.Any()) { tdList = result; }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in fetching term deposit refund data " + ex.Message);
                tdList = new();
            }
            return tdList;
        }

    }
}
