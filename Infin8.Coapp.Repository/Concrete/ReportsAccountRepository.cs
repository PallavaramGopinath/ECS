using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class ReportsAccountRepository : Repository<Reports_Master>, IReportsAccountRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsAccountRepository(CSISContext context) : base(context)
        {
        }

        public async Task<List<rptLedgerNameList>> GetLedgerNameList(string brCode)
        {
            List<rptLedgerNameList> ledList = new List<rptLedgerNameList>();
            try
            {
                #region query
                ledList = CSISContext.Database.SqlQueryRaw<rptLedgerNameList>(
                        @"SELECT Fin_Ledger_Fnl.Fnl_Id, 
                        Fin_Ledger_Fnl.Fnl_Name, 
                        Fin_Ledger_Grp.Grp_SlNo, 
                        Fin_Ledger_Grp.Grp_Id, 
                        Fin_Ledger_Grp.Grp_Name, 
                        Fin_Ledger.Led_SlNo, 
                        Fin_Ledger.Led_Id, 
                        Fin_Ledger.Led_Name
                        FROM  Fin_Ledger INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id 
                        INNER JOIN Fin_Ledger_Fnl  ON Fin_Ledger_Grp.Fnl_Id = Fin_Ledger_Fnl.Fnl_Id
                        WHERE (Fin_Ledger.Led_Delete = false) AND (Fin_Ledger_Grp.Grp_Delete = false)
                        ORDER BY Fin_Ledger_Fnl.Fnl_Id, Fin_Ledger_Grp.Grp_SlNo, Fin_Ledger.Led_SlNo").ToList();
                #endregion 

                var ledListTmp = await (from fl in CSISContext.Fin_Ledger // Replace YourFinLedgerEntity
                                        join flg in CSISContext.Fin_Ledger_Grp // Replace YourFinLedgerGrpEntity
                                            on fl.Grp_Id equals flg.Grp_Id
                                        join flf in CSISContext.Fin_Ledger_Fnl // Replace YourFinLedgerFnlEntity
                                            on flg.Fnl_Id equals flf.Fnl_Id
                                        where fl.Led_Delete == false && flg.Grp_Delete == false &&
                                            fl.BrCode == brCode &&
                                            flg.BrCode == brCode 
                                        orderby flf.Fnl_Id, flg.Grp_SlNo, fl.Led_SlNo
                                        select new rptLedgerNameList
                                        {
                                            Fnl_Id = flf.Fnl_Id,
                                            Fnl_Name = flf.Fnl_Name,
                                            Grp_SlNo = flg.Grp_SlNo,
                                            Grp_Id = flg.Grp_Id,
                                            Grp_Name = flg.Grp_Name,
                                            Led_SlNo = fl.Led_SlNo,
                                            Led_Id = fl.Led_Id,
                                            Led_Name = fl.Led_Name
                                        }).ToListAsync();
                if (ledListTmp.Any()) ledList = ledListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return ledList;
        }

        public async Task<List<rptDayBook2>> GetChittaBook(string fromDate, string toDate, decimal yrId, string brCode)
        {
            List<rptDayBook2> dayBookList = new List<rptDayBook2>();
            try
            {
                DateTime.TryParse(fromDate, out DateTime fromDateParse);
                DateTime.TryParse(toDate, out DateTime toDateParse);
                decimal cashLedId = 0;
                cashLedId = CSISContext.Map_General.Select(x => x.Cash_Led_Id).FirstOrDefault();

                #region query
                //dayBookList = CSISContext.Database.SqlQueryRaw<rptDayBook2>(
                //        @"SELECT vr.voc_rpt_No, 
                //        Vr.voc_pmt_No, 
                //        CONVERT(date, Vr.Voc_Date) AS Voc_Date , 
                //        Tr.voc_rpt, 
                //        Tr.voc_pmt, 
                //        Tr.voc_Trn_Type, 
                //        Tr.voc_narr
                //        FROM Fin_voucher_tr AS Tr INNER JOIN Fin_Voucher AS Vr ON  Tr.voc_Id = vr.Voc_Id  
                //        INNER JOIN Fin_Ledger AS Led ON Tr.led_id = Led.Led_Id 
                //        WHERE CAST(Vr.Voc_Date as date) BETWEEN @fromDate AND @toDate AND Tr.led_id = @cashId  AND  Tr.voc_Trn_Type = 1 AND  tr.FinVocTr_Delete = 0 AND vr.Voc_Delete = 0
                //        Order By
                //        Vr.Voc_Date ASC,Vr.voc_Rpt_slNo ASC ,Vr.voc_Pmt_slNo ASC"
                //        , new NpgsqlParameter("@fromDate", fromDate)
                //        , new NpgsqlParameter("@toDate", toDate)
                //        , new NpgsqlParameter("@cashId", cashLedId)).ToList();
                #endregion

                #region linq
                var dayBookListTmp = await (from tr in CSISContext.Fin_Voucher_Trn // Replace YourFinVoucherTrEntity
                                            join vr in CSISContext.Fin_Voucher // Replace YourFinVoucherEntity
                                                on tr.Voc_Id equals vr.Voc_Id
                                            join led in CSISContext.Fin_Ledger // Replace YourFinLedgerEntity
                                                on tr.Led_Id equals led.Led_Id
                                            where vr.Voc_Date.Date >= fromDateParse.Date &&
                                                  vr.Voc_Date.Date <= toDateParse.Date &&
                                                  tr.Led_Id == cashLedId &&
                                                  tr.Voc_Trn_Type == 1 &&
                                                  tr.FinVocTr_Delete == false &&
                                                  vr.Voc_Delete == false &&
                                                  tr.BrCode == brCode &&
                                                  vr.BrCode == brCode &&
                                                  led.BrCode == brCode &&
                                                  tr.Voc_Status == "V" &&
                                                  vr.Voc_Status == "V" 
                                            orderby vr.Voc_Date, vr.Voc_Rpt_SlNo, vr.Voc_Pmt_SlNo
                                            select new rptDayBook2
                                            {
                                                voc_rpt_No = vr.Voc_Rpt_No,
                                                voc_pmt_No = vr.Voc_Pmt_No,
                                                Voc_Date = vr.Voc_Date.Date,
                                                voc_rpt = tr.Voc_Rpt,
                                                voc_pmt = tr.Voc_Pmt,
                                                voc_Trn_Type = tr.Voc_Trn_Type,
                                                voc_narr = tr.Voc_Narr
                                            }).ToListAsync();
                #endregion 
                ///Led.Led_SlNo ASC,
                //double OBCashAmount = 0, CBCashAmount = 0;
                //string rsInWords = "";
                foreach (var dayBook in dayBookListTmp)
                {
                    if (dayBook.voc_Trn_Type == 1)
                    {
                        dayBook.voc_cash_rpt = dayBook.voc_rpt;
                        dayBook.voc_cash_pmt = dayBook.voc_pmt;
                    }
                    else
                    {
                        dayBook.voc_adj_rpt = dayBook.voc_rpt;
                        dayBook.voc_adj_pmt = dayBook.voc_pmt;
                    }

                    //OBCashAmount = 0; CBCashAmount = 0;
                    //dbAccount.GetLedgerOBAndCBAmount(cashLedId, yrId, dayBook.Voc_Date, out OBCashAmount, out CBCashAmount);
                    //dayBook.Fin_db_ob = OBCashAmount;
                    //dayBook.Fin_db_cb = CBCashAmount;
                    //if (CBCashAmount > 0)

                    //rsInWords = Utilities.RupeesInWords(CBCashAmount);
                    ////else
                    ////rsInWords = "Zero";
                    //dayBook.RsInWords = rsInWords;
                }
                if (dayBookListTmp.Any()) dayBookList = dayBookListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return dayBookList;
        }

        public async Task<List<rptDayBook2>> GetDayBook(string fromDate, string toDate, decimal yrId,string brCode)
        {
            List<rptDayBook2> dayBookList = new List<rptDayBook2>();
            try
            {
                decimal cashLedId = 0;
                cashLedId = CSISContext.Map_General.Select(x => x.Cash_Led_Id).FirstOrDefault();
                DateTime.TryParse(fromDate, out DateTime fromDateParse);
                DateTime.TryParse(toDate, out DateTime toDateParse);
                #region query
                //dayBookList = CSISContext.Database.SqlQueryRaw<rptDayBook2>(
                //            @"SELECT vr.voc_rpt_No, 
                //        Vr.voc_pmt_No, 
                //        CONVERT(date, Vr.Voc_Date) AS Voc_Date , 
                //        Tr.voc_rpt, Tr.voc_pmt, 
                //        Tr.voc_Trn_Type, 
                //        Tr.voc_narr, 
                //        Tr.led_id, 
                //        Led.Led_Name,
                //        Led.Led_SlNo,
                //        Vr.voc_Rpt_slNo,
                //        Vr.voc_Pmt_slNo
                //        FROM Fin_voucher_tr AS Tr INNER JOIN Fin_Voucher AS Vr ON  Tr.voc_Id = vr.Voc_Id  
                //        INNER JOIN Fin_Ledger AS Led ON Tr.led_id = Led.Led_Id 
                //        WHERE CAST(Vr.Voc_Date as date) BETWEEN @fromDate AND @toDate AND Tr.led_id != @cashId AND  tr.FinVocTr_Delete = 0 AND vr.Voc_Delete = 0
                //        Order By
                //        Vr.Voc_Date ASC,Led.Led_SlNo ASC,Vr.voc_Rpt_slNo ASC ,Vr.voc_Pmt_slNo ASC"
                //            , new NpgsqlParameter("@fromDate", fromDate)
                //            , new NpgsqlParameter("@toDate", toDate)
                //            , new NpgsqlParameter("@cashId", cashLedId)).ToList();
                #endregion

                #region linq
                var dayBookListTmp = await (from tr in CSISContext.Fin_Voucher_Trn // Replace YourFinVoucherTrEntity
                                            join vr in CSISContext.Fin_Voucher // Replace YourFinVoucherEntity
                                                on tr.Voc_Id equals vr.Voc_Id
                                            join led in CSISContext.Fin_Ledger // Replace YourFinLedgerEntity
                                                on tr.Led_Id equals led.Led_Id
                                            where vr.Voc_Date.Date >= fromDateParse.Date &&
                                                  vr.Voc_Date.Date <= toDateParse.Date &&
                                                  tr.Led_Id != cashLedId &&
                                                  tr.FinVocTr_Delete == false &&
                                                  vr.Voc_Delete == false &&
                                                  tr.BrCode == brCode &&
                                                  vr.BrCode == brCode &&
                                                  led.BrCode == brCode &&
                                                  tr.Voc_Status == "V" &&
                                                  vr.Voc_Status == "V"
                                            orderby vr.Voc_Date, led.Led_SlNo, vr.Voc_Rpt_SlNo, vr.Voc_Pmt_SlNo
                                            select new rptDayBook2
                                            {
                                                voc_rpt_No = vr.Voc_Rpt_No,
                                                voc_pmt_No = vr.Voc_Pmt_No,
                                                Voc_Date = vr.Voc_Date.Date,
                                                voc_rpt = tr.Voc_Rpt,
                                                voc_pmt = tr.Voc_Pmt,
                                                voc_Trn_Type = tr.Voc_Trn_Type,
                                                voc_narr = tr.Voc_Narr,
                                                Led_Id = tr.Led_Id,
                                                Led_Name = led.Led_Name,
                                                Led_SlNo = led.Led_SlNo,
                                                voc_Rpt_slNo = vr.Voc_Rpt_SlNo,
                                                voc_Pmt_slNo = vr.Voc_Pmt_SlNo
                                            }).ToListAsync();
                #endregion 

                //double OBCashAmount = 0, CBCashAmount = 0;
                //string rsInWords = "";
                if(dayBookListTmp !=null && dayBookListTmp.Any())
                {
                    dayBookList = dayBookListTmp.ToList();
                }
                foreach (var dayBook in dayBookList)
                {
                    if (dayBook.voc_Trn_Type == 1)
                    {
                        dayBook.voc_cash_rpt = dayBook.voc_rpt;
                        dayBook.voc_cash_pmt = dayBook.voc_pmt;
                    }
                    else
                    {
                        dayBook.voc_adj_rpt = dayBook.voc_rpt;
                        dayBook.voc_adj_pmt = dayBook.voc_pmt;
                    }

                    //OBCashAmount = 0; CBCashAmount = 0;
                    //dbAccount.GetLedgerOBAndCBAmount(cashId, yrId, dayBook.Voc_Date, out OBCashAmount, out CBCashAmount);
                    //dayBook.Fin_db_ob = OBCashAmount;
                    //dayBook.Fin_db_cb = CBCashAmount;
                    ////if (CBCashAmount > 0)
                    //rsInWords = Utilities.RupeesInWords(CBCashAmount);
                    ////else
                    ////rsInWords = "Zero";
                    //dayBook.RsInWords = rsInWords;
                }
                //if (dayBookListTmp.Any()) dayBookList = dayBookListTmp;
            }
            catch (Exception)
            {
                throw;
            }
            return dayBookList;
        }

        public async Task<List<rptFinGeneralLedger>> GetGeneralLedger(DateTime fromDate, DateTime toDate, List<decimal> glLedIdList, decimal yrId, string brCode)
        {
            List<rptFinGeneralLedger> glListFinal = new List<rptFinGeneralLedger>();
            List<rptFinGeneralLedger> glList = new List<rptFinGeneralLedger>();
            //int monthNo = 0, ledId = 0;
            //double previousRpt = 0, previousPmt = 0, openingBalance = 0, closingBalance = 0;
            try
            {
                //decimal cashLedId = CSISContext.Map_General.Where(x=> x.BrCode == brCode ).Select(x => x.Cash_Led_Id).FirstOrDefault();
                List<decimal> ledList = CSISContext.Fin_Ledger_Trn.Where(x => glLedIdList.Contains(x.Led_Id) && x.Yr_Id == yrId && x.LedgerTrn_Delete == false && x.BrCode == brCode).Select(x => x.Led_Id).ToList();
                foreach (var ledgerLedId in ledList)
                {
                    #region query
                    //            glList = CSISContext.Database.SqlQueryRaw<rptFinGeneralLedger>(
                    //            @"SELECT Trn.led_id, Grp.Fnl_Id,  Led.Led_Name, 
                    //                CONVERT(varchar(4), YEAR(Voc.Voc_Date)) + CONVERT(varchar(2), FORMAT(MONTH(Voc.Voc_Date),'0#')) AS GL_Month,
                    //          DATENAME(month,Voc.Voc_Date) + ' ' +  CONVERT(varchar(4), YEAR(Voc.Voc_Date)) AS GL_MonthName ,  
                    //          Voc.Voc_Date AS GL_Date,
                    //          CAST(0 AS float) As OpeningBalance,
                    //          Sum(Trn.voc_rpt) AS Receipts, Sum(Trn.voc_pmt) AS Payments, 
                    //          CAST(0 AS float) AS ClosingBalance,
                    //          CAST(0 AS float) AS PreviousRpt,
                    //          CAST(0 AS float) AS PreviousPmt,
                    //          CAST(0 AS float) AS TotalReceipts,
                    //          CAST(0 AS float) AS TotalPayments
                    //            FROM  Fin_Voucher_Tr AS Trn INNER JOIN Fin_Voucher AS Voc ON Trn.Voc_Id = Voc.voc_Id
                    //            INNER JOIN Fin_Ledger AS Led ON Trn.led_id = Led.Led_Id
                    //            INNER JOIN Fin_Ledger_Grp AS Grp ON Led.Grp_Id = Grp.Grp_Id
                    //            WHERE Voc.Voc_Date BETWEEN @fromDate AND @toDate 
                    //            AND Voc.Voc_Delete = 0 AND Trn.FinVocTr_Delete = 0 AND Trn.Led_Id = @ledId
                    //            GROUP BY Trn.led_id,Grp.Fnl_Id,Led.Led_Name, 
                    //                        CONVERT(varchar(4), YEAR(Voc.Voc_Date)) + CONVERT(varchar(2), FORMAT(MONTH(Voc.Voc_Date),'0#')),
                    //DATENAME(month,Voc.Voc_Date) + ' ' +  CONVERT(varchar(4), YEAR(Voc.Voc_Date)),
                    //                        Voc.Voc_Date
                    //            ORDER BY  Trn.led_id ,Voc.Voc_Date"
                    //            , new NpgsqlParameter("@fromDate", fromDate.Date)
                    //            , new NpgsqlParameter("@toDate", toDate.Date)
                    //            , new NpgsqlParameter("@ledId", led)).ToList();
                    #endregion

                    #region linq
                    //var glListTmp = await (from trn in CSISContext.Fin_Voucher_Trn
                    //                       join voc in CSISContext.Fin_Voucher on trn.Voc_Id equals voc.Voc_Id
                    //                       join led in CSISContext.Fin_Ledger on trn.Led_Id equals led.Led_Id
                    //                       join grp in CSISContext.Fin_Ledger_Grp on led.Grp_Id equals grp.Grp_Id
                    //                       where voc.Voc_Date >= fromDate.Date && voc.Voc_Date <= toDate.Date
                    //                             && voc.Voc_Delete == false && trn.FinVocTr_Delete == false && trn.Led_Id == ledgerLedId
                    //                       group new { trn, voc, led, grp } by new
                    //                       {
                    //                           trn.Led_Id,
                    //                           grp.Fnl_Id,
                    //                           led.Led_Name,
                    //                           GL_Month = voc.Voc_Date.Year.ToString() + voc.Voc_Date.Month.ToString("00"),
                    //                           GL_MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(voc.Voc_Date.Month) + " " + voc.Voc_Date.Year.ToString(),
                    //                           GL_Date = voc.Voc_Date
                    //                       } into g
                    //                       orderby g.Key.Led_Id, g.Key.GL_Date
                    //                       select new rptFinGeneralLedger
                    //                       {
                    //                           Led_Id = g.Key.Led_Id,
                    //                           Fnl_Id = g.Key.Fnl_Id,
                    //                           Led_Name = g.Key.Led_Name,
                    //                           GL_Month = g.Key.GL_Month,
                    //                           GL_MonthName = g.Key.GL_MonthName,
                    //                           GL_Date = g.Key.GL_Date,
                    //                           OpeningBalance = 0f,
                    //                           Receipts = g.Sum(x => x.trn.Voc_Rpt),
                    //                           Payments = g.Sum(x => x.trn.Voc_Pmt),
                    //                           ClosingBalance = 0f,
                    //                           PreviousRpt = 0f,
                    //                           PreviousPmt = 0f,
                    //                           TotalReceipts = 0f,
                    //                           TotalPayments = 0f
                    //                       }).ToListAsync();
                    //glListFinal.AddRange(glList);

                    #endregion

                    // Step 1: Write a translatable query to fetch the raw data from the database.
                    // We select into a temporary anonymous object.
                    var rawData = await (from trn in CSISContext.Fin_Voucher_Trn
                                         join voc in CSISContext.Fin_Voucher on trn.Voc_Id equals voc.Voc_Id
                                         join led in CSISContext.Fin_Ledger on trn.Led_Id equals led.Led_Id
                                         join grp in CSISContext.Fin_Ledger_Grp on led.Grp_Id equals grp.Grp_Id
                                         where voc.Voc_Date >= fromDate.Date && voc.Voc_Date <= toDate.Date
                                               && voc.Voc_Delete == false && trn.FinVocTr_Delete == false
                                               && trn.Led_Id == ledgerLedId
                                         // Order here if it helps the database, or order after grouping.
                                         // orderby trn.Led_Id, voc.Voc_Date
                                         select new
                                         {
                                             // Select all the fields you will need for the next step
                                             trn.Led_Id,
                                             grp.Fnl_Id,
                                             led.Led_Name,
                                             voc.Voc_Date,
                                             trn.Voc_Rpt,
                                             trn.Voc_Pmt
                                         }).ToListAsync(); // This executes the SQL query and brings data into memory.


                    // Step 2: Now that the data is in memory, group and project it using LINQ to Objects.
                    // All .NET methods (.ToString(), GetMonthName, etc.) are now perfectly fine to use.
                    var glListTmp = rawData
                        .GroupBy(data => new
                        {
                            // The grouping logic is now performed in your application
                            data.Led_Id,
                            data.Fnl_Id,
                            data.Led_Name,
                            GL_Month = data.Voc_Date.Year.ToString() + data.Voc_Date.Month.ToString("00"),
                            GL_MonthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(data.Voc_Date.Month) + " " + data.Voc_Date.Year.ToString(),
                            GL_Date = data.Voc_Date
                        })
                        .OrderBy(g => g.Key.Led_Id)
                        .ThenBy(g => g.Key.GL_Date)
                        .Select(g => new rptFinGeneralLedger
                        {
                            Led_Id = g.Key.Led_Id,
                            Fnl_Id = g.Key.Fnl_Id,
                            Led_Name = g.Key.Led_Name,
                            GL_Month = g.Key.GL_Month,
                            GL_MonthName = g.Key.GL_MonthName,
                            GL_Date = g.Key.GL_Date,
                            OpeningBalance = 0f, // These will be calculated later
                            Receipts = g.Sum(x => x.Voc_Rpt),
                            Payments = g.Sum(x => x.Voc_Pmt),
                            ClosingBalance = 0f,
                            PreviousRpt = 0f,
                            PreviousPmt = 0f,
                            TotalReceipts = 0f,
                            TotalPayments = 0f
                        }).ToList();

                    if (glListTmp == null )
                    {
                        int monthNo = 0;
                        double  openingBalance = 0, closingBalance = 0;
                        monthNo = fromDate.Month;
                        openingBalance = 0; closingBalance = 0;
                        //openingBalance = dbAccounts.GetLedgerBalance(led, yrId, fromDate);
                        closingBalance = openingBalance;
                        rptFinGeneralLedger glNoTransaction = new rptFinGeneralLedger();
                        glNoTransaction.OpeningBalance = openingBalance;
                        glNoTransaction.ClosingBalance = closingBalance;
                        glNoTransaction.PreviousPmt = 0;
                        glNoTransaction.PreviousRpt = 0;
                        glNoTransaction.GL_Month = fromDate.Month.ToString();
                        glNoTransaction.GL_MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(fromDate.Month);
                        glNoTransaction.Led_Id = ledgerLedId;
                        glNoTransaction.Led_Name = ""; // dbAccounts.GetLedgerName(led);
                        glNoTransaction.GL_Date = fromDate.Date;
                        glNoTransaction.Fnl_Id = 0;
                        glListFinal.Add(glNoTransaction);
                    }
                    else
                    {
                        glListFinal.AddRange(glListTmp);
                    }
                        
                    //if (glListTmp != null) glList = glListTmp;
                    #region to be developed in handler
                    //if (glList != null && glList.Count > 0)
                    //{
                    //    foreach (var gl in glList)
                    //    {
                    //        if (monthNo != gl.GL_Date.Month || ledId != gl.Led_Id)
                    //        {
                    //            previousRpt = dbAccounts.GetPreviousReceipt(gl.Led_Id, yrId, gl.GL_Date, context, out errorMessage);
                    //            if (errorMessage.Length > 0)
                    //            {
                    //                return glList;
                    //            }
                    //            previousPmt = dbAccounts.GetPreviousPayment(gl.Led_Id, yrId, gl.GL_Date, context, out errorMessage);
                    //            if (errorMessage.Length > 0)
                    //            {
                    //                return glList;
                    //            }
                    //            if (ledId == gl.Led_Id)
                    //            {
                    //                openingBalance = dbAccounts.GetLedgerBalance(gl.Led_Id, yrId, gl.GL_Date);

                    //            }
                    //            monthNo = gl.GL_Date.Month;
                    //            if (ledId != gl.Led_Id)
                    //            {
                    //                openingBalance = 0; closingBalance = 0;
                    //                openingBalance = dbAccounts.GetLedgerBalance(gl.Led_Id, yrId, gl.GL_Date);
                    //                closingBalance = openingBalance;
                    //                ledId = gl.Led_Id;
                    //            }
                    //        }

                    //        switch (gl.Fnl_Id)
                    //        {
                    //            case 1: /// Assets
                    //            case 4: /// Expenditure
                    //                if (gl.Led_Id == cashLedId)
                    //                    closingBalance += gl.Receipts - gl.Payments;
                    //                else
                    //                    closingBalance += gl.Payments - gl.Receipts;
                    //                break;
                    //            case 2: /// Liability
                    //            case 3: /// Income
                    //                closingBalance += gl.Receipts - gl.Payments;
                    //                break;
                    //        }
                    //        gl.OpeningBalance = openingBalance;
                    //        gl.ClosingBalance = closingBalance;
                    //        gl.PreviousPmt = previousPmt;
                    //        gl.PreviousRpt = previousRpt;
                    //    }
                    //}
                    //else
                    //{
                    //    monthNo = fromDate.Month;
                    //    openingBalance = 0; closingBalance = 0;
                    //    openingBalance = dbAccounts.GetLedgerBalance(ledgerLedId, yrId, fromDate);
                    //    closingBalance = openingBalance;
                    //    rptFinGeneralLedger glNoTransaction = new rptFinGeneralLedger();
                    //    glNoTransaction.OpeningBalance = openingBalance;
                    //    glNoTransaction.ClosingBalance = closingBalance;
                    //    glNoTransaction.PreviousPmt = 0;
                    //    glNoTransaction.PreviousRpt = 0;
                    //    glNoTransaction.GL_Month = fromDate.Month.ToString();
                    //    glNoTransaction.GL_MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(fromDate.Month);
                    //    glNoTransaction.Led_Id = ledgerLedId;
                    //    glNoTransaction.Led_Name = dbAccounts.GetLedgerName(ledgerLedId);
                    //    glNoTransaction.GL_Date = fromDate.Date;
                    //    glNoTransaction.Fnl_Id = 0;
                    //    glListFinal.Add(glNoTransaction);
                    //}
                    //glListFinal.AddRange(glList);
                    #endregion 
                }
                
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return glListFinal;
        }

        public async Task<List<rptFinReceiptAndCharges>> GetReceiptAndCharges(DateTime fromDate, DateTime toDate, decimal yrId,string brCode)
        {
            List<rptFinReceiptAndCharges> RnCList = new List<rptFinReceiptAndCharges>();
            //List<rptFinReceiptAndCharges> RncListFinal = new List<rptFinReceiptAndCharges>();
            try
            {
                decimal cashId = CSISContext.Map_General.Where(x=> x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
                #region query
                //List<rptFinReceiptAndCharges> RnCFromTo = CSISContext.Database.SqlQueryRaw<rptFinReceiptAndCharges>(
                //    @"SELECT Fin_voucher_tr.led_id AS Led_Id, 
                //        Sum(Fin_voucher_tr.voc_rpt) AS Dur_Rpt, 
                //        Sum(Fin_voucher_tr.voc_pmt) AS Dur_Pmt
                //        FROM Fin_Voucher INNER JOIN Fin_voucher_tr ON Fin_Voucher.Voc_Id = Fin_voucher_tr.voc_Id
                //        WHERE  CAST(CAST(Fin_Voucher.Voc_Date as date) AS varchar(10))  BETWEEN @fromDate AND @toDate
                //        AND Fin_voucher_tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Delete = false AND Fin_voucher_tr.led_id != @cashId 
                //        AND Fin_voucher_tr.yr_id = @yrId AND Fin_Voucher.Yr_Id = @yrId
                //        GROUP BY Fin_voucher_tr.led_id"
                //    , new NpgsqlParameter("@fromDate", fromDate.Date)
                //    , new NpgsqlParameter("@toDate", toDate.Date)
                //    , new NpgsqlParameter("@cashId", cashId)
                //    , new NpgsqlParameter("@yrId", yrId)).ToList();
                #endregion

                #region  linq
                //List<rptFinReceiptAndCharges> RnCFromTo = await (
                //    from fv in CSISContext.Fin_Voucher
                //    join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id
                //    join led in CSISContext.Fin_Ledger on fvt.Led_Id equals led.Led_Id
                //    where fv.Voc_Date.Date >= fromDate.Date
                //       && fv.Voc_Date.Date <= toDate.Date
                //       && fvt.FinVocTr_Delete == false
                //       && fv.Voc_Delete == false
                //       && fv.BrCode == brCode
                //       && fvt.BrCode == brCode
                //       && fvt.Led_Id != cashId
                //       && fvt.Yr_Id == yrId
                //       && fv.Yr_Id == yrId
                //       && fv.Voc_Status == "V"
                //       && fvt.Voc_Status == "V"
                //    group fvt by fvt.Led_Id into g
                //    select new rptFinReceiptAndCharges
                //    {
                //        Led_Id = g.Key,
                //        Dur_Rpt = g.Sum(x => x.Voc_Rpt),
                //        Dur_Pmt = g.Sum(x => x.Voc_Pmt)
                //    }).ToListAsync();
                List<rptFinReceiptAndCharges> RnCFromTo = await (
                   from fv in CSISContext.Fin_Voucher
                   join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id
                   join led in CSISContext.Fin_Ledger on fvt.Led_Id equals led.Led_Id
                   where fv.Voc_Date.Date >= fromDate.Date
                      && fv.Voc_Date.Date <= toDate.Date
                      && fvt.FinVocTr_Delete == false
                      && fv.Voc_Delete == false
                      && fv.BrCode == brCode
                      && fvt.BrCode == brCode
                      && fvt.Led_Id != cashId
                      && fvt.Yr_Id == yrId
                      && fv.Yr_Id == yrId
                      && fv.Voc_Status == "V"
                      && fvt.Voc_Status == "V"
                   group fvt by new
                   {
                       fvt.Led_Id,
                       led.Led_Name
                   } into g
                   select new rptFinReceiptAndCharges
                   {
                       Led_Id = g.Key.Led_Id,
                       Led_Name = g.Key.Led_Name,
                       Dur_Rpt = g.Sum(x => x.Voc_Rpt),
                       Dur_Pmt = g.Sum(x => x.Voc_Pmt)
                   }).ToListAsync();
                #endregion 

                if (RnCFromTo != null)
                {
                    RnCList.AddRange(RnCFromTo);
                }

                #region query
                //List<rptFinReceiptAndCharges> RnCBeg = CSISContext.Database.SqlQueryRaw<rptFinReceiptAndCharges>(
                //        @"SELECT Fin_voucher_tr.led_id AS Led_Id, 
                //        Sum(Fin_voucher_tr.voc_rpt) AS Beg_Rpt, 
                //        Sum(Fin_voucher_tr.voc_pmt) AS Beg_Pmt
                //        FROM Fin_Voucher INNER JOIN Fin_voucher_tr ON Fin_Voucher.Voc_Id = Fin_voucher_tr.voc_Id
                //        WHERE CAST(Fin_Voucher.Voc_Date AS date) < @fromDate
                //        AND Fin_voucher_tr.FinVocTr_Delete = false AND Fin_Voucher.Voc_Delete = false AND Fin_voucher_tr.led_id != @cashId
                //        AND Fin_voucher_tr.yr_id = @yrId AND Fin_Voucher.Yr_Id = @yrId
                //        GROUP BY Fin_voucher_tr.led_id"
                //        , new NpgsqlParameter("@fromDate", fromDate.Date)
                //        , new NpgsqlParameter("@cashId", cashId)
                //        , new NpgsqlParameter("@yrId", yrId)).ToList();
                #endregion

                #region linq
                List<rptFinReceiptAndCharges> RnCBeg = await (
                    from fv in CSISContext.Fin_Voucher
                    join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id
                    join led in CSISContext.Fin_Ledger on fvt.Led_Id equals led.Led_Id
                    where fv.Voc_Date.Date < fromDate.Date
                       && fvt.FinVocTr_Delete == false
                       && fv.Voc_Delete == false
                       && fv.BrCode == brCode
                       && fvt.BrCode == brCode
                       && fvt.Led_Id != cashId
                       && fvt.Yr_Id == yrId
                       && fv.Yr_Id == yrId
                       && fv.Voc_Status == "V"
                       && fvt.Voc_Status == "V"
                    group fvt by new
                    {
                        fvt.Led_Id,
                        led.Led_Name
                    } into g
                    select new rptFinReceiptAndCharges
                    {
                        Led_Id = g.Key.Led_Id,
                        Led_Name = g.Key.Led_Name,
                        Beg_Rpt = g.Sum(x => x.Voc_Rpt),
                        Beg_Pmt = g.Sum(x => x.Voc_Pmt)
                    }).ToListAsync();
                #endregion 

                if (RnCBeg != null)
                {
                    RnCList.AddRange(RnCBeg);
                }
                RnCList = (from x in RnCList
                           join led in CSISContext.Fin_Ledger on x.Led_Id equals led.Led_Id
                            where led.BrCode == brCode
                           group x by new
                           {
                               x.Led_Id,
                               x.Grp_Id,
                               x.Led_Name
                           } into g
                           select new rptFinReceiptAndCharges
                           {
                               Led_Id = g.Key.Led_Id,
                               Led_Name = g.Key.Led_Name,
                               Beg_Rpt = g.Sum(trn => trn.Beg_Rpt),
                               Beg_Pmt = g.Sum(trn => trn.Beg_Pmt),
                               Dur_Rpt = g.Sum(trn => trn.Dur_Rpt),
                               Dur_Pmt = g.Sum(trn => trn.Dur_Pmt)
                           }).ToList();

                foreach (var rnc in RnCList)
                {
                    rnc.Tot_Rpt = rnc.Beg_Rpt + rnc.Dur_Rpt;
                    rnc.Tot_Pmt = rnc.Beg_Pmt + rnc.Dur_Pmt;
                }

                #region to be developed in handler
                //double RptBegOB = context.FinLedgerTrns.Where(x => x.Led_Id.Value == cashId && x.LedgerTrn_Delete == false && x.Yr_Id.Value == yrId).Select(x => x.OB_Amt.Value).FirstOrDefault();
                //double RptDurOB = dbAccounts.GetLedgerBalance(cashId, yrId, fromDate);
                //double PmtDurCB = dbAccounts.GetLedgerBalance(cashId, yrId, toDate.AddDays(1));
                //RncListFinal = (from x in RnCList
                //                join led in context.FinLedgers on x.Led_Id equals led.Led_Id
                //                join grp in context.FinLedgerGrps on led.Grp_Id equals grp.Grp_Id
                //                select new rptFinReceiptAndCharges
                //                {
                //                    Led_Id = x.Led_Id,
                //                    Led_Name = led.Led_Name,
                //                    Fnl_Id = (int)grp.Fnl_Id,
                //                    Grp_Id = led.Grp_Id.Value,
                //                    Grp_Name = grp.Grp_Name,
                //                    Beg_Rpt = x.Beg_Rpt,
                //                    Beg_Pmt = x.Beg_Pmt,
                //                    Dur_Rpt = x.Dur_Rpt,
                //                    Dur_Pmt = x.Dur_Pmt,
                //                    Tot_Rpt = x.Tot_Rpt,
                //                    Tot_Pmt = x.Tot_Pmt,
                //                    Rpt_Dur_OB = RptDurOB,
                //                    Rpt_Beg_OB = RptBegOB,
                //                    Pmt_End_CB = PmtDurCB
                //                }).ToList();
                #endregion 
            }
            catch (Exception)
            {
                throw;
            }
            return RnCList;
        }

        public async Task<List<rptFinBalanceSheet>> GetBalanceSheet(DateTime fromDate, DateTime toDate, decimal yrId,string brCode)
        {
            List<rptFinBalanceSheet> balanceSheet = new List<rptFinBalanceSheet>();
            try
            {
                //DatabaseAccount dbAccounts = new DatabaseAccount();
                //if (!dbAccounts.UpdateLedgerBalance_New(yrId, fromDate, toDate, out errorMessage))
                //{
                //    return balanceSheet;
                //}
                List<rptFinBalanceSheet> lossOrProfit = new List<rptFinBalanceSheet>();

                #region query
                //lossOrProfit = CSISContext.Database.SqlQueryRaw<rptFinBalanceSheet>(
                //    @"SELECT Sum(Fin_Ledger_Trn.CB_Amt) AS CB_Amt, 
                //        Fin_Ledger_Grp.Fnl_Id
                //        FROM Fin_Ledger_Trn INNER JOIN Fin_Ledger ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id
                //        INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id 
                //        WHERE Fin_Ledger_Trn.Yr_Id = @yrId AND Fin_Ledger_Trn.LedgerTrn_Delete = 0 AND Fin_Ledger.Led_Delete = 0 AND Fin_Ledger_Grp.Grp_Delete = 0
                //        GROUP BY Fin_Ledger_Grp.Fnl_Id
                //        HAVING Fin_Ledger_Grp.Fnl_Id In (3,4)"
                //    , new NpgsqlParameter("@yrId", yrId)).ToList();
                #endregion

                #region linq
                lossOrProfit = await (
                    from flt in CSISContext.Fin_Ledger_Trn
                    join fl in CSISContext.Fin_Ledger on flt.Led_Id equals fl.Led_Id
                    join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                    where flt.Yr_Id == yrId
                       && flt.LedgerTrn_Delete == false
                       && fl.Led_Delete == false
                       && flg.Grp_Delete == false
                       && flt.BrCode == brCode
                       && fl.BrCode == brCode
                       && flg.BrCode == brCode  
                       && new[] { 3, 4 }.Contains(flg.Fnl_Id)
                    group flt by flg.Fnl_Id into g
                    select new rptFinBalanceSheet
                    {
                        CB_Amt = g.Sum(x => x.CB_Amt),
                        Fnl_Id = g.Key
                    }).ToListAsync();
                #endregion 

                double Profit = 0, Loss = 0;
                if (lossOrProfit != null)
                {
                    foreach (var single in lossOrProfit)
                    {
                        if (single.Fnl_Id == 3)
                            Profit += single.CB_Amt;
                        if (single.Fnl_Id == 4)
                            Loss += single.CB_Amt;
                    }
                }
                if (Profit >= Loss)
                {
                    Profit = Profit - Loss;
                    Loss = 0;

                }
                if (Loss >= Profit)
                {
                    Loss = Loss - Profit;
                    Profit = 0;
                }

                #region query
                //balanceSheet = CSISContext.Database.SqlQueryRaw<rptFinBalanceSheet>(
                //    @"SELECT
                //     Fin_Ledger_Trn.Led_Id,
                //     Fin_Ledger.Led_Name,
                //     Fin_Ledger_Grp.Grp_Id,
                //     Fin_Ledger_Grp.Grp_Name,
                //     Fin_Ledger_Fnl.Fnl_Id,
                //        Fin_Ledger_Fnl.Fnl_Name,
                //        Fin_Ledger_Trn.OB_Amt,
                //        Fin_Ledger_Trn.CB_Amt
                //    From
                //        Fin_Ledger_Trn Fin_Ledger_Trn INNER JOIN Fin_Ledger Fin_Ledger ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id
                //        INNER JOIN Fin_Ledger_Grp Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id
                //        INNER JOIN Fin_Ledger_Fnl Fin_Ledger_Fnl ON Fin_Ledger_Grp.Fnl_Id = Fin_Ledger_Fnl.Fnl_Id
                //    WHERE Fin_Ledger_Fnl.Fnl_ID <= 2 AND Fin_Ledger_Trn.LedgerTrn_Delete = 0 AND Fin_Ledger_Trn.Yr_Id = @yrId
                //    Order By
                //        Fin_Ledger_Fnl.Fnl_Id ASC,Fin_Ledger_Grp.Grp_SlNo ASC,Fin_Ledger.Led_SlNo ASC"
                //, new NpgsqlParameter("@yrId", yrId)).ToList();
                #endregion

                #region linq
                balanceSheet = await (
                        from flt in CSISContext.Fin_Ledger_Trn
                        join fl in CSISContext.Fin_Ledger on flt.Led_Id equals fl.Led_Id
                        join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                        join fln in CSISContext.Fin_Ledger_Fnl on flg.Fnl_Id equals fln.Fnl_Id
                        where fln.Fnl_Id <= 2
                           && flt.LedgerTrn_Delete == false
                           && flt.BrCode == brCode
                           && fl.BrCode == brCode
                           && flg.BrCode == brCode
                           && flt.Yr_Id == yrId
                        orderby fln.Fnl_Id ascending, flg.Grp_SlNo ascending, fl.Led_SlNo ascending
                        select new rptFinBalanceSheet
                        {
                            Led_Id = flt.Led_Id,
                            Led_Name = fl.Led_Name,
                            Grp_Id = flg.Grp_Id,
                            Grp_Name = flg.Grp_Name,
                            Fnl_Id = fln.Fnl_Id,
                            Fnl_Name = fln.Fnl_Name,
                            OB_Amt = flt.OB_Amt,
                            CB_Amt = flt.CB_Amt
                        }).ToListAsync();
                #endregion 

                foreach (var bs in balanceSheet)
                {
                    bs.Profit = Profit;
                    bs.Loss = Loss;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return balanceSheet;
        }

        public async Task<List<rptFinLossAndProfit>> GetLossAndProfit(DateTime fromDate, DateTime toDate, decimal yrId,string brCode)
        {
            List<rptFinLossAndProfit> lossAndProfitList = new List<rptFinLossAndProfit>();
            try
            {
                #region query
                //List<rptFinBalanceSheet> profitList = CSISContext.Database.SqlQueryRaw<rptFinBalanceSheet>(
                //    @"SELECT Fin_Ledger_Trn.Led_Id, 
                //        Fin_Ledger_Trn.CB_Amt,Fin_Ledger.Led_Name 
                //        FROM Fin_Ledger_Trn INNER JOIN Fin_Ledger ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id
                //        INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id
                //        WHERE Fin_Ledger_Trn.Yr_Id = @yrId AND Fin_Ledger.Led_Delete = 0 AND Fin_Ledger_Trn.LedgerTrn_Delete = 0 AND Fin_Ledger_Grp.Fnl_Id = 3
                //        ORDER BY Fin_Ledger_Grp.Fnl_Id, Fin_Ledger_Grp.Grp_Id, Fin_Ledger_Trn.Led_Id"
                //    , new NpgsqlParameter("@yrId", yrId)).ToList();
                #endregion

                #region linq
                List<rptFinBalanceSheet> profitList = await (
                    from flt in CSISContext.Fin_Ledger_Trn
                    join fl in CSISContext.Fin_Ledger on flt.Led_Id equals fl.Led_Id
                    join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                    where flt.Yr_Id == yrId
                       && fl.Led_Delete == false
                       && flt.LedgerTrn_Delete == false
                       && flt.BrCode == brCode
                       && fl.BrCode == brCode
                       && flg.BrCode == brCode
                       && flg.Fnl_Id == 3
                    orderby flg.Fnl_Id, flg.Grp_Id, flt.Led_Id
                    select new rptFinBalanceSheet
                    {
                        Led_Id = flt.Led_Id,
                        CB_Amt = flt.CB_Amt,
                        Led_Name = fl.Led_Name
                    }).ToListAsync();
                #endregion

                #region 
                //List<rptFinBalanceSheet> lossList = CSISContext.Database.SqlQueryRaw<rptFinBalanceSheet>(
                //    @"SELECT Fin_Ledger_Trn.Led_Id, 
                //        Fin_Ledger_Trn.CB_Amt,Fin_Ledger.Led_Name  
                //        FROM Fin_Ledger_Trn INNER JOIN Fin_Ledger ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id
                //        INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id
                //        WHERE Fin_Ledger_Trn.Yr_Id = @yrId AND Fin_Ledger.Led_Delete = 0 AND Fin_Ledger_Trn.LedgerTrn_Delete = 0 AND Fin_Ledger_Grp.Fnl_Id = 4
                //        ORDER BY Fin_Ledger_Grp.Fnl_Id, Fin_Ledger_Grp.Grp_Id, Fin_Ledger_Trn.Led_Id"
                //    , new NpgsqlParameter("@yrId", yrId)).ToList();
                #endregion

                #region linq
                List<rptFinBalanceSheet> lossList = await (
                        from flt in CSISContext.Fin_Ledger_Trn
                        join fl in CSISContext.Fin_Ledger on flt.Led_Id equals fl.Led_Id
                        join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                        where flt.Yr_Id == yrId
                           && fl.Led_Delete == false
                           && flt.LedgerTrn_Delete == false
                           && flt.BrCode == brCode
                           && fl.BrCode == brCode
                           && flg.BrCode == brCode
                           && flg.Fnl_Id == 4
                        orderby flg.Fnl_Id, flg.Grp_Id, flt.Led_Id
                        select new rptFinBalanceSheet
                        {
                            Led_Id = flt.Led_Id,
                            CB_Amt = flt.CB_Amt,
                            Led_Name = fl.Led_Name
                        }).ToListAsync();
                #endregion 

                //double TotRpt = 0, TotPmt = 0;
                if (lossList.Count <= profitList.Count)
                {
                    foreach (var lnp in lossList.Zip(profitList, Tuple.Create))
                    {
                        if (lnp.Equals(lossList.Count - 1))
                        {
                            rptFinLossAndProfit lossAndProfit = new rptFinLossAndProfit
                            {
                                LLed_Id = 0,
                                LLed_Name = "",
                                LAmount = 0,
                                PLed_Id = lnp.Item2.Led_Id,
                                PLed_Name = lnp.Item2.Led_Name,
                                PAmount = lnp.Item2.CB_Amt
                            };
                            lossAndProfitList.Add(lossAndProfit);
                        }
                        else
                        {
                            rptFinLossAndProfit lossAndProfit = new rptFinLossAndProfit
                            {
                                LLed_Id = lnp.Item1.Led_Id,
                                LLed_Name = lnp.Item1.Led_Name,
                                LAmount = lnp.Item1.CB_Amt,
                                PLed_Id = lnp.Item2.Led_Id,
                                PLed_Name = lnp.Item2.Led_Name,
                                PAmount = lnp.Item2.CB_Amt
                            };
                            lossAndProfitList.Add(lossAndProfit);
                        }
                    }

                    foreach (var profit in profitList.Skip(lossList.Count))
                    {
                        rptFinLossAndProfit lossAndProfit = new rptFinLossAndProfit
                        {
                            LLed_Id = 0,
                            LLed_Name = "",
                            LAmount = 0,
                            PLed_Id = profit.Led_Id,
                            PLed_Name = profit.Led_Name,
                            PAmount = profit.CB_Amt
                        };
                        lossAndProfitList.Add(lossAndProfit);
                    }
                }

                if (profitList.Count <= lossList.Count)
                {
                    foreach (var lnp in lossList.Zip(profitList, Tuple.Create))
                    {
                        rptFinLossAndProfit lossAndProfit = new rptFinLossAndProfit
                        {
                            LLed_Id = lnp.Item1.Led_Id,
                            LLed_Name = lnp.Item1.Led_Name,
                            LAmount = lnp.Item1.CB_Amt,
                            PLed_Id = lnp.Item2.Led_Id,
                            PLed_Name = lnp.Item2.Led_Name,
                            PAmount = lnp.Item2.CB_Amt
                        };
                        lossAndProfitList.Add(lossAndProfit);
                        //}
                    }

                    foreach (var loss in lossList.Skip(profitList.Count))
                    {
                        rptFinLossAndProfit lossAndProfit = new rptFinLossAndProfit
                        {
                            LLed_Id = loss.Led_Id,
                            LLed_Name = loss.Led_Name,
                            LAmount = loss.CB_Amt,
                            PLed_Id = 0,
                            PLed_Name = "",
                            PAmount = 0
                        };
                        lossAndProfitList.Add(lossAndProfit);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return lossAndProfitList;
        }

    }
}
