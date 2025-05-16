using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<rptTermDepositRegister>> GetTermDepositRegister(List<decimal> TDIdList, DateTime fromDate, DateTime toDate, string TDSchemeType)
        {
            List<rptTermDepositRegister> tdList = new List<rptTermDepositRegister>();
            try
            {
                string fetchIdList = string.Join(",", TDIdList);
                #region query
                var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositRegister>(
                            @"SELECT x.*, y.* , z.* 
                            FROM 
                                (SELECT TD_Id, 
                                Max(Trn_Date) AS Trn_Date,
                                Sum(DepositReceiptAmount) AS DepositReceiptAmount,  Sum(DepositPaidAmount) AS DepositPaidAmount, 
                                Sum(InterestCalculatedAmount) AS InterestCalculatedAmount, Max(InterestAppliedDate) AS InterestAppliedDate,Sum(InterestPaidAmount) AS InterestPaidAmount,
                                Sum(PenalCalculatedAmount) AS PenalCalculatedAmount,  Max(PenalAppliedDate) AS PenalAppliedDate, 
                                Sum(PenalReceivedAmount) AS PenalReceivedAmount
                                FROM TermDeposit_Trn 
                                WHERE Trn_Date < @fromDate And TD_Delete = false AND TermDeposit_Trn.voc_status ='V'
                                GROUP BY TD_Id
                                UNION
                                SELECT TD_Id,
                                Trn_Date,
                                DepositReceiptAmount, 
                                DepositPaidAmount, 
                                InterestCalculatedAmount, 
                                InterestAppliedDate, 
                                InterestPaidAmount,  
                                PenalCalculatedAmount, 
                                PenalAppliedDate, 
                                PenalReceivedAmount 
                            FROM TermDeposit_Trn 
                            Where Trn_Date BETWEEN @fromDate AND  @toDate AND TermDeposit_Trn.voc_status = 'V'
                            And TD_Delete = false AND (DepositReceiptAmount >0 OR DepositPaidAmount >0 OR InterestCalculatedAmount >0 OR InterestPaidAmount >0 OR PenalCalculatedAmount >0 OR PenalReceivedAmount >0 )) as x, 
                          (SELECT TD_Id AS TDMas_TD_Id, Mem_Id, TD_No, TDScheme_Id, TDH_Name, COALESCE(TDH_Age ,0) AS TDH_Age, ModeOfOperation, ValueDate, DepositAmount, 
                            PeriodInMonths, PeriodInDays, RateOfInterest, PenalRate, MaturityDate, MaturityAmount, 
                            InterestPayableFrequency, IsDiscountRate, Nominee1Name, Nominee1Age, Nominee1Relationship, 
                            Nominee2Name, Nominee2Age, Nominee2Relationship, Status,RenewalTD_No from TermDeposit_Master ) as y,
                        (SELECT mem_Id  AS MemMas_mem_Id, memberNo, PerNo, memberName,FatherName, PerAdd1,PerAdd2,PerAdd3,mobileNo,PANNo,AadharNo,SmartCardNo,memberphoto  FROM Mem_Master ) as z,
                        (SELECT TDScheme_Id AS schemeId,TDScheme_Name, TDSchemeType from TermDeposit_Schemes where TDSchemeType = @schemeType) as a
                            Where x.TD_Id  = y.TDMas_TD_Id  AND y.mem_Id = z.MemMas_mem_Id AND y.TDScheme_Id =  a.schemeId AND x.TD_Id in (" + fetchIdList + ") "
                        , new NpgsqlParameter("@fromDate", fromDate)
                        , new NpgsqlParameter("@toDate", toDate)
                        , new NpgsqlParameter("@schemeType", TDSchemeType)).ToListAsync();
                #endregion 

                if (tdListTmp != null) tdList = tdListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<List<rptFDOutstanding>> GetTermDepositOutstanding(DateTime asOnDate, string TDSchemeType,string brCode)
        {
            List<rptFDOutstanding> tdList = new List<rptFDOutstanding>();
            try
            {
                var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptFDOutstanding>(
                        @"SELECT
                        TermDeposit_Master.TDScheme_Id,
                        TermDeposit_Schemes.TDScheme_Name,
                        TermDeposit_Master.TD_No,
                        TermDeposit_Master.TDH_Name, 
                        TermDeposit_Master.ValueDate, 
                        TermDeposit_Master.DepositAmount,
                        TermDeposit_Master.PeriodInMonths, 
                        TermDeposit_Master.PeriodInDays, 
                        TermDeposit_Master.RateOfInterest, 
                        TermDeposit_Master.MaturityDate,
	                    Mem_Master.memberNo, TermDeposit_Schemes.TDSchemeType,
                        TermDeposit_Master.MaturityAmount, 
                        Sum(TermDeposit_Trn.DepositReceiptAmount) AS DepositReceiptAmount,
	                    Sum(TermDeposit_Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, 
                        Max(TermDeposit_Trn.InterestAppliedDate) AS InterestAppliedDate,
                        Sum(TermDeposit_Trn.InterestPaidAmount) AS InterestPaidAmount
                    From
                        TermDeposit_Master  INNER JOIN TermDeposit_Trn TermDeposit_Trn ON TermDeposit_Master.TD_Id = TermDeposit_Trn.TD_Id
                        INNER JOIN Mem_Master  ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
                        INNER JOIN TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                        WHERE TermDeposit_Master.TD_Delete = false AND TermDeposit_Trn.TD_Delete= false  AND TermDeposit_Trn.Trn_Date <=@asOnDate 
                        AND TermDeposit_Master.brcode = @brCode AND TermDeposit_Master.voc_status = 'V'
                        AND Mem_Master.brcode = @brCode
                        AND TermDeposit_Schemes.brcode = @brCode
                        AND TermDeposit_Master.TD_Id in 
                            (SELECT TermDeposit_Trn.TD_Id From TermDeposit_Trn INNER JOIN TermDeposit_Master ON TermDeposit_Trn.TD_Id = TermDeposit_Master.TD_Id 
                            Where TermDeposit_Trn.Trn_Date <=@asOnDate AND TermDeposit_Trn.TD_Delete = false
                            AND TermDeposit_Master.TD_Delete = false AND TermDeposit_Trn.TD_Delete = false
                            GROUP BY TermDeposit_Trn.TD_Id 
                            HAVING Sum(TermDeposit_Trn.DepositReceiptAmount) - Sum(TermDeposit_Trn.DepositPaidAmount) >0) AND  TermDeposit_Schemes.TDSchemeType = @schemeType
                        GROUP BY TermDeposit_Master.TDScheme_Id,TermDeposit_Schemes.TDScheme_Name,TermDeposit_Master.TD_No,TermDeposit_Master.TDH_Name, 
                        TermDeposit_Master.ValueDate, TermDeposit_Master.DepositAmount,TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays, 
                        TermDeposit_Master.RateOfInterest, TermDeposit_Master.MaturityDate,Mem_Master.memberNo, TermDeposit_Schemes.TDSchemeType,TermDeposit_Master.MaturityAmount
                    Order By
                    TermDeposit_Schemes.TDSchemeType ASC,
                    TermDeposit_Master.TD_No ASC"
                        , new NpgsqlParameter("@asOnDate", asOnDate)
                        , new NpgsqlParameter("@schemeType", TDSchemeType)
                        , new NpgsqlParameter ("@brCode",brCode)).ToListAsync();
                if (tdListTmp != null) tdList = tdListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<List<rptFDOutstanding>> GetTermDepositOutstandingIndividual(decimal memId, DateTime asOnDate, string TDSchemeType)
        {
            List<rptFDOutstanding> tdList = new List<rptFDOutstanding>();
            try
            {
                #region query
                var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptFDOutstanding>(
                        @"SELECT
	                        Scheme.TDScheme_Id,Scheme.TDScheme_Name,
	                        Mem.memberNo, 
	                        TD.TD_No,TD.TDH_Name, TD.ValueDate, TD.DepositAmount,
                            TD.PeriodInMonths, TD.PeriodInDays, TD.RateOfInterest, TD.MaturityDate,
                            TD.MaturityAmount, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, Max(Trn.InterestAppliedDate) AS InterestAppliedDate ,
                            Sum(Trn.InterestPaidAmount) AS InterestPaidAmount
                        From
                            TermDeposit_Master TD INNER JOIN TermDeposit_Trn Trn ON
                                TD.TD_Id = Trn.TD_Id
                            INNER JOIN Mem_Master Mem ON
                                TD.Mem_Id = Mem.mem_Id
                            INNER JOIN TermDeposit_Schemes Scheme ON
                                TD.TDScheme_Id = Scheme.TDScheme_Id
                        WHERE  TD.voc_status ='V' AND
                        TD.TD_Id in 
                        (
                            SELECT TD.TD_Id FROM TermDeposit_Trn AS Trn INNER JOIN TermDeposit_Master AS TD ON Trn.TD_Id =  TD.TD_Id 
                            WHERE TD.Mem_Id = @memId AND  Trn.Trn_Date <=@asOnDate AND Scheme.TDSchemeType = @schemeType AND TD.TD_Delete = FALSE AND Trn.TD_Delete = FALSE 
                            AND TD.voc_status = 'V' AND Trn.voc_status = 'V'
                            GROUP BY TD.TD_Id
                            HAVING Sum(Trn.DepositReceiptAmount) - Sum(Trn.DepositPaidAmount ) >0
                        )
                        AND Scheme.TDSchemeType = @schemeType AND TD.TD_Delete = FALSE AND Trn.TD_Delete = FALSE 
                         GROUP BY Scheme.TDScheme_Id,Scheme.TDScheme_Name,
	                        Mem.memberNo,  TD.TD_No,TD.TDH_Name, TD.ValueDate, TD.DepositAmount,   TD.PeriodInMonths, TD.PeriodInDays, TD.RateOfInterest, TD.MaturityDate,TD.MaturityAmount
                        ORDER BY Scheme.TDScheme_Id, TD.TD_No "
                    , new NpgsqlParameter("@memId", memId)
                    , new NpgsqlParameter("@asOnDate", asOnDate)
                    , new NpgsqlParameter("@schemeType", TDSchemeType)).ToListAsync();
                #endregion 

                if (tdListTmp != null) { tdList = tdListTmp; }
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<List<rptTermDepositPayable>> GetTermDepositPayable(DateTime asOnDate, string brCode)
        {
            List<rptTermDepositPayable> tdList = new List<rptTermDepositPayable>();
            try
            {
                var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositPayable>(
                        @"With Db1
                        AS
                        (
                        select Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate,
                            Max(Trn.Trn_Date) AS Trn_Date, SUM(Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                            Max(Trn.InterestAppliedDate) AS InterestAppliedDate, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount,
                            Sum(Trn.InterestPaidAmount) AS InterestPaidAmount, Sum(Trn.InterestCalculatedAmount - Trn.InterestPaidAmount) AS IntPayable
                            FROM TermDeposit_Trn AS Trn
                            INNER JOIN TermDeposit_Master AS Mas ON Trn.TD_Id = Mas.TD_Id
                            WHERE Trn.TD_Delete = FALSE AND Mas.td_delete = FALSE
                            AND Trn.voc_status ='V' AND Mas.voc_status ='V' 
                            and  CAST(Trn.Trn_Date AS date) <= @asOnDate
                            GROUP BY Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate
                            HAVING Sum(Trn.InterestCalculatedAmount - Trn.InterestPaidAmount) > 0
                        UNION
                            SELECT Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate,
                            Max(Trn.Trn_Date) AS Trn_Date, Sum(Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                            Max(Trn.InterestAppliedDate) AS InterestAppliedDate, 0 :: DOUBLE PRECISION AS InterestCalculatedAmount,
                            0 :: DOUBLE PRECISION AS InterestPaidAmount, 0 :: DOUBLE PRECISION AS IntPayable
                            FROM TermDeposit_Master AS Mas
                            INNER JOIN TermDeposit_Trn AS Trn ON Mas.TD_Id = Trn.TD_Id
                            WHERE  Mas.voc_status = 'V' AND Trn.voc_status = 'V' AND Mas.MaturityDate :: DATE <= @asOnDate AND Trn.TD_Delete = FALSE AND Mas.TD_Delete = FALSE
                            GROUP BY Trn.TD_Id, Mas.TD_No,Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id, Mas.TD_No, Mas.TDH_Name, Mas.DepositAmount, Mas.RateOfInterest, Mas.MaturityDate
                            HAVING Sum(Trn.DepositReceiptAmount) - Sum(Trn.DepositPaidAmount) > 0
                        )
                        SELECT Db1.TD_Id, Db1.TD_No,Db1.ValueDate,Db1.PeriodInMonths,Db1.PeriodInDays,Db1.RateOfInterest, Db1.TDScheme_Id, Db1.TDH_Name, Db1.DepositAmount, Db1.MaturityDate, Db1.Trn_Date,
                            Sum(Db1.DepositReceiptAmount) AS DepositReceiptAmount,
                            Sum(Db1.InterestCalculatedAmount) AS InterestCalculatedAmount, Sum(Db1.InterestPaidAmount) AS InterestPaidAmount,
                            Sum(Db1.IntPayable) AS IntPayable, Max(Db1.InterestAppliedDate) AS InterestAppliedDate,
                            Sch.TDScheme_Name, Sum(Db1.IntPayable) AS TotalPayable
                        FROM Db1
                            INNER JOIN TermDeposit_Schemes AS Sch ON Db1.TDScheme_Id = Sch.TDScheme_Id
                            GROUP BY Db1.TD_Id, Db1.TD_No,Db1.ValueDate,Db1.PeriodInMonths,Db1.PeriodInDays,Db1.RateOfInterest, Db1.TDScheme_Id, Db1.TDH_Name, Db1.DepositAmount, Db1.MaturityDate, Db1.Trn_Date, Sch.TDScheme_Name
                            HAVING Sum(Db1.IntPayable) > 0
                            ORDER BY Db1.TDScheme_Id, Db1.TD_No"
                            , new NpgsqlParameter("@asOnDate", asOnDate)).ToListAsync();
                if (tdListTmp != null) { tdList = tdListTmp; }
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<List<rptTermDepositPayable>> GetTermDepositMaturityPayable(DateTime asOnDate, string tdSchemeType, string brCode)
        {
            List<rptTermDepositPayable> tdList = new List<rptTermDepositPayable>();
            try
            {
                if (tdSchemeType == "F")
                {
                    var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositPayable>(
                        @" SELECT Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id,Mas.TDH_Name,Sch.TDSchemeType,Sch.TDScheme_Name ,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate, 
                            Max(Trn.Trn_Date) AS Trn_Date, Sum(Trn.DepositReceiptAmount) AS DepositReceiptAmount,
                            Max(Trn.InterestAppliedDate) AS InterestAppliedDate, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, 
                            Sum(Trn.InterestPaidAmount) AS InterestPaidAmount, Sum(Trn.InterestCalculatedAmount) - Sum(Trn.InterestPaidAmount) AS IntPayable ,
                            Sum(Trn.DepositReceiptAmount) + (Sum(Trn.InterestCalculatedAmount) - Sum(Trn.InterestPaidAmount) ) AS TotalPayable
						    FROM TermDeposit_Master AS Mas
                            INNER JOIN TermDeposit_Trn AS Trn ON Mas.TD_Id = Trn.TD_Id 
							INNER JOIN TermDeposit_Schemes AS Sch ON Mas.TDScheme_Id = Sch.TDScheme_Id 
                            WHERE Mas.brcode = @brCode AND Mas.voc_status = 'V' 
                            AND Trn.brcode = @brCode AND Trn.voc_status ='V'
                            AND Sch.brcode =@brCode
                            AND Mas.MaturityDate :: DATE <=@asOnDate AND Trn.TD_Delete = FALSE AND Mas.TD_Delete = FALSE AND Sch.TDSchemeType = @tdSchemeType
                            GROUP BY Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays,Mas.TDScheme_Id,Sch.TDSchemeType,Sch.TDScheme_Name,Mas.TD_No, Mas.TDH_Name,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate
                            HAVING Sum(Trn.DepositReceiptAmount)- Sum(Trn.DepositPaidAmount) >0 
							ORDER BY Mas.TDScheme_Id,Mas.TD_No "
                        , new NpgsqlParameter("@asOnDate", asOnDate.Date)
                        , new NpgsqlParameter("@tdSchemeType", tdSchemeType)
                        , new NpgsqlParameter("@brCode",brCode )).ToListAsync();
                    if (tdListTmp != null) { tdList = tdListTmp; }
                }
                if (tdSchemeType == "R")
                {
                    var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTermDepositPayable>(
                        @" SELECT Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays, Mas.TDScheme_Id,Mas.TDH_Name,Sch.TDSchemeType,Sch.TDScheme_Name ,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate, 
                            Max(Trn.Trn_Date) AS Trn_Date, Sum(Trn.DepositReceiptAmount) AS DepositReceiptAmount,Sum(Trn.DepositPaidAmount) AS DepositPaidAmount,
                            Max(Trn.InterestAppliedDate) AS InterestAppliedDate, Sum(Trn.InterestCalculatedAmount) AS InterestCalculatedAmount, 
                            Sum(Trn.InterestPaidAmount) AS InterestPaidAmount, (Mas.MaturityAmount) - (Mas.DepositAmount  * Mas.PeriodInMonths)  AS IntPayable ,
                            Mas.MaturityAmount AS TotalPayable
						    FROM TermDeposit_Master AS Mas
                            INNER JOIN TermDeposit_Trn AS Trn ON Mas.TD_Id = Trn.TD_Id 
							INNER JOIN TermDeposit_Schemes AS Sch ON Mas.TDScheme_Id = Sch.TDScheme_Id 
                            WHERE  Mas.brcode = @brCode AND Mas.voc_status = 'V' 
                            AND Trn.brcode = @brCode AND Trn.voc_status ='V'
                            AND Sch.brcode =@brCode
                            and Mas.MaturityDate :: DATE <=@asOnDate AND Trn.TD_Delete = FALSE AND Mas.TD_Delete = FALSE AND Sch.TDSchemeType = @tdSchemeType
                            GROUP BY Trn.TD_Id,Mas.TD_No, Mas.ValueDate,Mas.PeriodInMonths,Mas.PeriodInDays,Mas.TDScheme_Id,Sch.TDSchemeType,Sch.TDScheme_Name,Mas.TD_No, Mas.TDH_Name,Mas.DepositAmount,Mas.RateOfInterest, Mas.MaturityDate,Mas.MaturityAmount
                            HAVING  Sum(Trn.DepositPaidAmount) = 0
							ORDER BY Mas.TDScheme_Id,Mas.TD_No  "
                        , new NpgsqlParameter("@asOnDate", asOnDate.Date)
                        , new NpgsqlParameter("@tdSchemeType", tdSchemeType)).ToListAsync();
                    if (tdListTmp != null) { tdList = tdListTmp; }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

        public async Task<rptFDBond> GetFDBondPreprinted(decimal vocId)
        {
            rptFDBond fDBond = new rptFDBond();
            try
            {
                fDBond = await GetFDBondData(vocId);

                switch (fDBond.InterestPayableFrequency)
                {
                    case 1:
                        fDBond.TDFrequency = "Monthly";
                        break;
                    case 3:
                        fDBond.TDFrequency = "Quarterly";
                        break;
                    case 6:
                        fDBond.TDFrequency = "Half Yearly";
                        break;
                    case 12:
                        fDBond.TDFrequency = "Annual";
                        break;
                    case 0:
                        fDBond.TDFrequency = "On Maturity";
                        break;
                }

                switch (fDBond.ModeOfOperation)
                {
                    case 1:
                        fDBond.ModeOfOperationString = "Single";
                        break;
                    case 2:
                        fDBond.ModeOfOperationString = "Either or Survivor";
                        break;
                    case 3:
                        fDBond.ModeOfOperationString = "Any one or Survivor/s";
                        break;
                    case 4:
                        fDBond.ModeOfOperationString = "Any Two or Survivior/s";
                        break;
                    case 5:
                        fDBond.ModeOfOperationString = "All Parents Jointly";
                        break;
                    case 6:
                        fDBond.ModeOfOperationString = "Any two jointly";
                        break;
                    case 7:
                        fDBond.ModeOfOperationString = "Former or Survivior";
                        break;
                    case 8:
                        fDBond.ModeOfOperationString = "Later or Survivor";
                        break;
                    case 9:
                        fDBond.ModeOfOperationString = "By Guardian";
                        break;
                    case 10:
                        fDBond.ModeOfOperationString = "By Mandate";
                        break;
                    case 11:
                        fDBond.ModeOfOperationString = "Power of attorney";
                        break;
                    case 12:
                        fDBond.ModeOfOperationString = "Proprietor";
                        break;
                }
                //fDBond.RsInWords = Utilities.RupeesInWords(fDBond.DepositAmount);
                fDBond.RateOfInterest = fDBond.RateOfInterest / 100;
            }
            catch (Exception)
            {
                throw;
            }
            return fDBond;
        }

        private async Task<rptFDBond> GetFDBondData(decimal vocId)
        {
            rptFDBond _bond = new rptFDBond();
            try
            {

                var _bondTmp = await CSISContext.Database.SqlQueryRaw<rptFDBond>(
                    @"SELECT
                        TermDeposit_Master.TD_No, TermDeposit_Master.TDH_Name, TermDeposit_Master.AccountOpenDate, 
                        TermDeposit_Master.ValueDate,TermDeposit_Master.DepositAmount, TermDeposit_Master.PeriodInMonths, 
                        TermDeposit_Master.PeriodInDays, TermDeposit_Master.RateOfInterest, 
                        TermDeposit_Master.MaturityDate, TermDeposit_Master.MaturityAmount, TermDeposit_Master.InterestPayableFrequency, 
                        TermDeposit_Master.Nominee1Name, TermDeposit_Master.Nominee1Age, TermDeposit_Master.Nominee1Relationship, 
                        TermDeposit_Master.Nominee2Name, TermDeposit_Master.Nominee2Age, TermDeposit_Master.Nominee2Relationship, 
                        TermDeposit_Schemes.TDScheme_Name, TermDeposit_Master.RenewalTD_No,
                        Fin_Voucher.voc_rpt_No, Fin_Voucher.Voc_No,
                        Mem_Master.memberNo,Mem_Master.memberName,Mem_Master.mobileNo, 
                        CONCAT (
						    COALESCE(Mem_Master.PreAdd1, ''),
						    ',',
						    COALESCE(Mem_Master.PreAdd2 || ',', ''),
						    COALESCE(Mem_Master.PreAdd3 || ',', ''),
						    COALESCE(Mem_Master.PrePin, '') 
                        ) AS Address,
                    ModeOfOperation 
                    From 
                        TermDeposit_Master TermDeposit_Master INNER JOIN TermDeposit_Schemes TermDeposit_Schemes ON 
                            TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                         INNER JOIN Mem_Master Mem_Master ON
                            TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
                         LEFT OUTER JOIN Fin_Voucher Fin_Voucher ON
                            TermDeposit_Master.Voc_Id = Fin_Voucher.Voc_Id
                        WHERE TermDeposit_Master.Voc_Id = @vocId
                        AND TermDeposit_Master.td_delete = FALSE
						AND Fin_Voucher.voc_delete = FALSE AND
                        TermDeposit_Master.voc_status ='V' AND Fin_Voucher.voc_status ='V'"
                    , new NpgsqlParameter("@vocId", vocId)).FirstOrDefaultAsync();
                if (_bondTmp != null) { _bond = _bondTmp; }

            }
            catch (Exception)
            {
            }
            return _bond;
        }

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
            List<rptTDNewBetweenDates> tdList = new List<rptTDNewBetweenDates>();
            //TransactionOptions options = new TransactionOptions();
            //options.IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted;
            //options.Timeout = new TimeSpan(5, 00, 0);
            try
            {
                var tdListTmp = await  CSISContext.Database.SqlQueryRaw<rptTDNewBetweenDates>(
                    @"SELECT
                        TermDeposit_Master.TDScheme_Id,
                        TermDeposit_Schemes.TDScheme_Name,
                        TermDeposit_Schemes.TDSchemeType,
                        TermDeposit_Master.TD_No,
                        Mem_Master.memberNo, 
                        Mem_Master.memberName,
                        TermDeposit_Master.TDH_Name, 
						TermDeposit_Master.AccountOpenDate,
                        TermDeposit_Master.ValueDate, 
                        TermDeposit_Master.DepositAmount,
                        TermDeposit_Master.PeriodInMonths, 
                        TermDeposit_Master.PeriodInDays, 
                        TermDeposit_Master.RateOfInterest, 
                        TermDeposit_Master.InterestPayableFrequency,
                        TermDeposit_Master.MaturityDate,
                        TermDeposit_Master.MaturityAmount
                    From
                        TermDeposit_Master  INNER JOIN TermDeposit_Trn TermDeposit_Trn ON TermDeposit_Master.Voc_Id = TermDeposit_Trn.Voc_Id
                        INNER JOIN Mem_Master  ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id
                        INNER JOIN TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                        WHERE TermDeposit_Master.TD_Delete = FALSE AND TermDeposit_Trn.TD_Delete= FALSE
                        AND TermDeposit_Master.brcode = @brCode AND TermDeposit_Master.voc_status ='V'
                        AND TermDeposit_Trn.brcode = @brCode AND TermDeposit_Trn.voc_status ='V'
                        AND Mem_Master.brcode = @brCode 
                        AND TermDeposit_Schemes.brcode = @brCode
                        AND TermDeposit_Master.AccountOpenDate BETWEEN @fromDate AND @toDate 
                        AND TermDeposit_Schemes.TDSchemeType = @schemeType
                        GROUP BY TermDeposit_Master.TDScheme_Id,TermDeposit_Schemes.TDScheme_Name,TermDeposit_Schemes.TDSchemeType,TermDeposit_Master.TD_No,
                        Mem_Master.memberNo, Mem_Master.memberName,TermDeposit_Master.TDH_Name, 
                        TermDeposit_Master.AccountOpenDate,TermDeposit_Master.ValueDate, TermDeposit_Master.DepositAmount,TermDeposit_Master.PeriodInMonths, TermDeposit_Master.PeriodInDays, 
                        TermDeposit_Master.RateOfInterest, TermDeposit_Master.InterestPayableFrequency,TermDeposit_Master.MaturityDate, TermDeposit_Master.MaturityAmount
						HAVING Sum(TermDeposit_Trn.DepositPaidAmount) = 0
                    Order By
                    TermDeposit_Schemes.TDSchemeType ASC,
                    TermDeposit_Master.TD_No ASC"
                , new NpgsqlParameter("@fromDate", fromDate)
                , new NpgsqlParameter("@toDate", toDate)
                , new NpgsqlParameter("@schemeType", TDSchemeType)).ToListAsync();
                if(tdListTmp != null ) {tdList = tdListTmp;}
            }
            catch (Exception)
            {
            }
            return tdList;
        }

        public async Task<List<rptTDRefundBetweenDates>> GetFDRefundBetweenDated(DateTime fromDate, DateTime toDate, string TDSchemeType,string brCode)
        {
            List<rptTDRefundBetweenDates> tdList = new List<rptTDRefundBetweenDates>();
            try
            {
                decimal TDLedId = CSISContext.Map_General.Where(x=> x.BrCode == brCode).Select(x=> x.FD_Led_Id).First();
                var tdListTmp = await CSISContext.Database.SqlQueryRaw<rptTDRefundBetweenDates>(
                            @"SELECT TermDeposit_Trn.Trn_Date, TermDeposit_Trn.Voc_Id, TermDeposit_Master.TD_Id, TermDeposit_Master.TD_No, TermDeposit_Master.TDScheme_Id, TermDeposit_Schemes.TDScheme_Name, TermDeposit_Master.Mem_Id, 
                              Mem_Master.memberNo, Mem_Master.memberName, TermDeposit_Trn.DepositPaidAmount
                            FROM TermDeposit_Master INNER JOIN
                                TermDeposit_Trn ON TermDeposit_Master.TD_Id = TermDeposit_Trn.TD_Id INNER JOIN
                                Fin_voucher_trn ON TermDeposit_Trn.Voc_Id = Fin_voucher_trn.voc_id INNER JOIN
                                Mem_Master ON TermDeposit_Master.Mem_Id = Mem_Master.mem_Id INNER JOIN
                                TermDeposit_Schemes ON TermDeposit_Master.TDScheme_Id = TermDeposit_Schemes.TDScheme_Id
                            WHERE (Fin_voucher_trn.Status = 'FR') AND (TermDeposit_Trn.DepositPaidAmount > 0) AND (TermDeposit_Master.TD_Delete = FALSE) 
		                        AND (TermDeposit_Trn.TD_Delete = FALSE) 
		                        AND (TermDeposit_Trn.Trn_Date >= @fromDate AND TermDeposit_Trn.Trn_Date <=  @toDate)
		                        AND (Fin_voucher_trn.voc_pmt >0)
		                        AND (Fin_voucher_trn.led_id = @tdLedId)
                                AND  TermDeposit_Schemes.TDSchemeType = @schemeType
                                AND TermDeposit_Master.brcode = @brCode AND TermDeposit_Master.voc_status = 'V'
                                AND TermDeposit_Trn.brcode = @brCode AND TermDeposit_Trn.voc_status = 'V'
                                AND Fin_Voucher_Trn.brcode = @brcode AND Fin_voucher_trn.voc_status = 'V'
                                AND Mem_Master.brcode = @brCode
                                AND TermDeposit_Schemes.brcode = @brCode
                            ORDER BY TermDeposit_Trn.Trn_Date, TermDeposit_Master.TD_No"
                        , new NpgsqlParameter("@fromDate", fromDate)
                        , new NpgsqlParameter("@toDate", toDate)
                        , new NpgsqlParameter("@schemeType", TDSchemeType)
                        , new NpgsqlParameter("@tdLedId", TDLedId)
                        , new NpgsqlParameter ("@brcode",brCode )).ToListAsync();
                if( tdListTmp != null ) { tdList = tdListTmp;}
            }
            catch (Exception)
            {
                throw;
            }
            return tdList;
        }

    }
}
