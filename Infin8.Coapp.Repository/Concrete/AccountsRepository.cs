using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infin8.Coapp.Repository
{
    public class AccountsRepository : Repository<Account_Transactions>, IAccountsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public AccountsRepository(CSISContext context) : base(context)
        {
        }
        class LedgerBalanceModel
        {
            public double LedgerBalance { get; set; }
            public int FnlId { get; set; }
        }

        class LedgerReceiptAndPaymentsModel
        {
            public double ReceiptAmount { get; set; }
            public double PaymentAmount { get; set; }
        }

        public enum FinalAccountLedger
        {
            Assets = 1,
            Liabilities = 2,
            Income = 3,
            Expenditure = 4
        }

        public async Task<bool> AddAccountsTransaction(Account_Transactions accountTransaction)
        {
            bool result = false;
            try
            {
                int maxId = await CSISContext.Account_Transactions.MaxAsync(x => x.Acc_Id);
                maxId++;
                accountTransaction.Acc_Id = maxId;
                await AddAsync(accountTransaction);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Account Transaction data not saved");
            }
            return result;
        }

        public async Task<bool> EditAccountsTransaction(Account_Transactions accountTransaction)
        {
            bool result = false;
            try
            {
                await EditAsync(accountTransaction);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Accounts Transaction table not deleted");
            }
            return result;
        }
        public async Task<string> GetTransactedAccountNameFromAccount_Transactions(int accId)
        {
            return await CSISContext.Account_Transactions.Where(x => x.Acc_Id == accId).Select(x => x.Acc_Name).FirstAsync();
        }


        #region general
        public async Task<decimal> GetCashLedgerId(string brCode)
        {
            return await CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefaultAsync();
        }
        #endregion 
        public async Task<(double OBAmount, double CBAmount)> GetLedgerOBAndCBAmount(decimal ledId, decimal yrId, DateTime toDate, string brCode)
        {
            double OBAmount = 0;
            double CBAmount = 0;
            double rptAmt = 0;
            double pmtAmt = 0;
            int fnlId = 0;
            decimal cashLedId = 0;

            LedgerBalanceModel ledObj = new LedgerBalanceModel();
            LedgerReceiptAndPaymentsModel ledRptAndPmt = new LedgerReceiptAndPaymentsModel();
            try
            {
                cashLedId = await GetCashLedgerId(brCode);

                /// Get OB and final Ledger Id
                #region query
                //ledObj = await  CSISContext.Database.SqlQueryRaw<LedgerBalanceModel>(
                //    @"SELECT Fin_Ledger_Trn.OB_Amt AS LedgerBalance, Fin_Ledger_Grp.Fnl_Id AS FnlId
                //    FROM Fin_Ledger_Trn INNER JOIN (Fin_Ledger INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id) 
                //    ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id WHERE Fin_Ledger_Trn.Yr_Id = @YrId  AND Fin_Ledger_Trn.LedgerTrn_Delete = 0 
                //    AND Fin_Ledger.Led_Delete = 0  AND Fin_Ledger_Trn.Led_Id = @LedId"
                //    , new NpgsqlParameter("@YrId", yrId)
                //    , new NpgsqlParameter("@LedId", ledId)).FirstOrDefaultAsync();
                #endregion

                #region linq
                ledObj = await (from trn in CSISContext.Fin_Ledger_Trn
                                join ledger in CSISContext.Fin_Ledger
                                    on trn.Led_Id equals ledger.Led_Id
                                join grp in CSISContext.Fin_Ledger_Grp
                                    on ledger.Grp_Id equals grp.Grp_Id
                                where trn.Yr_Id == yrId
                                   && trn.LedgerTrn_Delete == false
                                   && ledger.Led_Delete == false
                                   && trn.Led_Id == ledId
                                select new LedgerBalanceModel
                                {
                                    LedgerBalance = trn.OB_Amt,
                                    FnlId = grp.Fnl_Id
                                }).FirstAsync();
                #endregion 

                if (ledObj != null)
                {
                    OBAmount = Convert.ToDouble(ledObj.LedgerBalance);
                    CBAmount = Convert.ToDouble(ledObj.LedgerBalance);
                    fnlId = Convert.ToInt16(ledObj.FnlId);
                }
                else
                {
                    OBAmount = 0;
                    CBAmount = 0;
                    fnlId = 0;
                }
                /// Get receipt and payment amoount for ob
                #region query
                //ledRptAndPmt = CSISContext.Database.SqlQueryRaw<LedgerReceiptAndPaymentsModel>(
                //        @"SELECT ISNULL(Sum(Fin_voucher_tr.voc_rpt), 0) AS ReceiptAmount, ISNULL(Sum(Fin_voucher_tr.voc_pmt),0) AS PaymentAmount
                //    FROM Fin_Voucher INNER JOIN Fin_voucher_tr ON Fin_Voucher.Voc_Id = Fin_voucher_tr.voc_Id WHERE Fin_Voucher.Yr_Id = @YrId  
                //    AND Fin_voucher_tr.led_id = @LedId AND Fin_voucher_tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Delete = 0 AND CAST(Fin_Voucher.Voc_Date as date) <@toDate"
                //        , new NpgsqlParameter("@LedId", ledId)
                //        , new NpgsqlParameter("@YrId", yrId)
                //        , new NpgsqlParameter("@toDate", toDate)).First();
                #endregion

                #region linq
                ledRptAndPmt = await (from voucher in CSISContext.Fin_Voucher
                                      join voucherTr in CSISContext.Fin_Voucher_Trn
                                          on voucher.Voc_Id equals voucherTr.Voc_Id
                                      where voucher.Yr_Id == yrId
                                         && voucherTr.Led_Id == ledId
                                         && voucherTr.FinVocTr_Delete == false
                                         && voucher.Voc_Delete == false
                                         && voucher.Voc_Date.Date < toDate
                                      select new { voucherTr.Voc_Rpt, voucherTr.Voc_Pmt })
               .GroupBy(x => 1) // Group by a constant since we want a single result
               .Select(g => new LedgerReceiptAndPaymentsModel
               {
                   ReceiptAmount = g.Sum(x => x.Voc_Rpt),
                   PaymentAmount = g.Sum(x => x.Voc_Pmt)
               })
               .FirstAsync();
                #endregion 

                if (ledRptAndPmt != null)
                {
                    rptAmt = Convert.ToDouble(ledRptAndPmt.ReceiptAmount);
                    pmtAmt = Convert.ToDouble(ledRptAndPmt.PaymentAmount);
                }
                /// calculate OBAmount
                switch (fnlId)
                {
                    case 1:
                    case 4:
                        if (ledId == cashLedId)
                        {
                            OBAmount += rptAmt - pmtAmt;
                        }
                        else
                        {
                            OBAmount += pmtAmt - rptAmt;
                        }
                        break;
                    case 2:
                    case 3:
                        OBAmount += rptAmt - pmtAmt;
                        break;
                }
                /// Get receipt and payment amount for cb

                #region query
                //ledRptAndPmt = CSISContext.Database.SqlQueryRaw<LedgerReceiptAndPaymentsModel>(
                //    @"SELECT ISNULL(Sum(Fin_voucher_tr.voc_rpt), 0) AS ReceiptAmount, ISNULL(Sum(Fin_voucher_tr.voc_pmt),0) AS PaymentAmount
                //    FROM Fin_Voucher INNER JOIN Fin_voucher_tr ON Fin_Voucher.Voc_Id = Fin_voucher_tr.voc_Id WHERE Fin_Voucher.Yr_Id = @YrId  
                //    AND Fin_voucher_tr.led_id = @LedId AND Fin_voucher_tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Delete = 0 AND CAST(Fin_Voucher.Voc_Date as date) <=@toDate"
                //        , new NpgsqlParameter("@LedId", ledId)
                //        , new NpgsqlParameter("@YrId", yrId)
                //        , new NpgsqlParameter("@toDate", toDate)).First();
                #endregion

                #region linq
                ledRptAndPmt = (from voucher in CSISContext.Fin_Voucher
                                join voucherTr in CSISContext.Fin_Voucher_Trn
                                    on voucher.Voc_Id equals voucherTr.Voc_Id
                                where voucher.Yr_Id == yrId
                                   && voucherTr.Led_Id == ledId
                                   && voucherTr.FinVocTr_Delete == false
                                   && voucher.Voc_Delete == false
                                   && voucher.Voc_Date.Date <= toDate
                                select new { voucherTr.Voc_Rpt, voucherTr.Voc_Pmt })
           .GroupBy(x => 1) // Group by a constant for single-row aggregation
           .Select(g => new LedgerReceiptAndPaymentsModel
           {
               ReceiptAmount = g.Sum(x => x.Voc_Rpt),
               PaymentAmount = g.Sum(x => x.Voc_Pmt)
           })
           .First();
                #endregion

                if (ledRptAndPmt != null)
                {
                    rptAmt = Convert.ToDouble(ledRptAndPmt.ReceiptAmount);
                    pmtAmt = Convert.ToDouble(ledRptAndPmt.PaymentAmount);
                }

                /// calculate OBAmount
                switch (fnlId)
                {
                    case 1:
                    case 4:
                        if (ledId == cashLedId)
                        {
                            CBAmount += rptAmt - pmtAmt;
                        }
                        else
                        {
                            CBAmount += pmtAmt - rptAmt;
                        }
                        break;
                    case 2:
                    case 3:
                        CBAmount += rptAmt - pmtAmt;
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return (OBAmount, CBAmount);
        }

        public async Task<double> GetPreviousReceipt(decimal ledId, decimal yrId, DateTime dateUpto)
        {
            double? balance = 0;
            try
            {
                #region query
                //balance = await (CSISContext.Database.SqlQueryRaw<double?>(
                //    @"SELECT Sum(fin_voucher_tr.voc_rpt) FROM Fin_Voucher INNER JOIN 
                //    fin_voucher_tr ON Fin_Voucher.Voc_Id = fin_voucher_tr.voc_Id 
                //    WHERE Fin_Voucher.Voc_Delete = 0 AND Fin_Voucher_Tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Date < @dateUpto
                //    AND Fin_Voucher.Yr_Id = @yrId AND Fin_voucher_tr.Led_Id = @ledId"
                //    , new NpgsqlParameter("@dateUpto", dateUpto)
                //    , new NpgsqlParameter("@yrId", yrId)
                //    , new NpgsqlParameter("@ledId", ledId))).FirstOrDefaultAsync();
                #endregion

                #region linq
                balance = await (from voucher in CSISContext.Fin_Voucher
                                 join voucherTr in CSISContext.Fin_Voucher_Trn
                                     on voucher.Voc_Id equals voucherTr.Voc_Id
                                 where voucher.Voc_Delete == false
                                    && voucherTr.FinVocTr_Delete == false
                                    && voucher.Voc_Date < dateUpto
                                    && voucher.Yr_Id == yrId
                                    && voucherTr.Led_Id == ledId
                                 select voucherTr.Voc_Rpt)
                .SumAsync();
                #endregion 
            }
            catch (Exception)
            {
                throw;
            }
            double.TryParse(balance.ToString(), out double result);
            return result;
        }

        public async Task<double> GetPreviousPayment(decimal ledId, decimal yrId, DateTime dateUpto)
        {
            double? balance = 0;
            try
            {
                #region query
                //balance = await (CSISContext.Database.SqlQueryRaw<double?>(
                //    @"SELECT Sum(fin_voucher_tr.voc_pmt) FROM Fin_Voucher INNER JOIN 
                //    fin_voucher_tr ON Fin_Voucher.Voc_Id = fin_voucher_tr.voc_Id 
                //    WHERE Fin_Voucher.Voc_Delete = 0 AND Fin_Voucher_Tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Date < @dateUpto
                //    AND Fin_Voucher.Yr_Id = @yrId AND Fin_voucher_tr.Led_Id = @ledId"
                //    , new NpgsqlParameter("@dateUpto", dateUpto)
                //    , new NpgsqlParameter("@yrId", yrId)
                //    , new NpgsqlParameter("@ledId", ledId))).FirstOrDefaultAsync();
                #endregion

                #region linq
                balance = await (from voucher in CSISContext.Fin_Voucher
                                 join voucherTr in CSISContext.Fin_Voucher_Trn
                                     on voucher.Voc_Id equals voucherTr.Voc_Id
                                 where voucher.Voc_Delete == false
                                    && voucherTr.FinVocTr_Delete == false
                                    && voucher.Voc_Date < dateUpto
                                    && voucher.Yr_Id == yrId
                                    && voucherTr.Led_Id == ledId
                                 select voucherTr.Voc_Pmt)
                .SumAsync();
                #endregion 
            }
            catch (Exception)
            {
                throw;
            }
            double.TryParse(balance.ToString(), out double result);
            return result;
        }

        public async Task<double> GetLedgerBalance(decimal ledId, decimal yrId, DateTime upToDate,string brCode)
        {
            double ledgerOB = 0;
            double rptAmt = 0;
            double pmtAmt = 0;
            int fnlId = 0;
            decimal cashLedId = 0;
            LedgerBalanceModel ledObj = new LedgerBalanceModel();
            LedgerReceiptAndPaymentsModel ledRptAndPmt = new LedgerReceiptAndPaymentsModel();
            try
            {
                /// Get Cash Ledger Id
                cashLedId = await CSISContext.Map_General.Select(x => x.Cash_Led_Id).FirstAsync();
                /// Get OB and final Ledger Id
                #region query
                //var ledObjTmp = CSISContext.Database.SqlQueryRaw<LedgerBalanceModel>(
                //    @"SELECT Fin_Ledger_Trn.OB_Amt AS LedgerBalance, Fin_Ledger_Grp.Fnl_Id AS FnlId
                //    FROM Fin_Ledger_Trn INNER JOIN (Fin_Ledger INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id) 
                //    ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id WHERE Fin_Ledger_Trn.Yr_Id = @YrId  AND Fin_Ledger_Trn.LedgerTrn_Delete = 0 
                //    AND Fin_Ledger.Led_Delete = 0  AND Fin_Ledger_Trn.Led_Id = @LedId"
                //    , new NpgsqlParameter("@YrId", yrId)
                //    , new NpgsqlParameter("@LedId", ledId)).FirstOrDefault();
                #endregion

                #region linq
                var ledObjTmp = await (from trn in CSISContext.Fin_Ledger_Trn
                                       join ledger in CSISContext.Fin_Ledger
                                           on trn.Led_Id equals ledger.Led_Id
                                       join grp in CSISContext.Fin_Ledger_Grp
                                           on ledger.Grp_Id equals grp.Grp_Id
                                       where trn.Yr_Id == yrId
                                          && trn.LedgerTrn_Delete == false
                                          && ledger.Led_Delete == false
                                          && trn.Led_Id == ledId
                                          && trn.BrCode == brCode 
                                          && ledger.BrCode == brCode 
                                          && trn.BrCode == brCode
                                       select new LedgerBalanceModel
                                       {
                                           LedgerBalance = trn.OB_Amt,
                                           FnlId = grp.Fnl_Id
                                       }).FirstOrDefaultAsync();
                if (ledObjTmp != null) ledObj = ledObjTmp;
                #endregion 

                if (ledObj != null)
                {
                    ledgerOB = Convert.ToDouble(ledObj.LedgerBalance);
                    fnlId = Convert.ToInt16(ledObj.FnlId);
                }
                else
                {
                    ledgerOB = 0;
                    fnlId = 0;
                }
                /// Get receipt and payment amount

                #region query
                //var ledRptAndPmtTmp = CSISContext.Database.SqlQueryRaw<LedgerReceiptAndPaymentsModel>(
                //        @"SELECT ISNULL(Sum(Fin_voucher_tr.voc_rpt), 0) AS ReceiptAmount, ISNULL(Sum(Fin_voucher_tr.voc_pmt),0) AS PaymentAmount
                //    FROM Fin_Voucher INNER JOIN Fin_voucher_tr ON Fin_Voucher.Voc_Id = Fin_voucher_tr.voc_Id WHERE Fin_Voucher.Yr_Id = @YrId  
                //    AND Fin_voucher_tr.led_id = @LedId AND Fin_voucher_tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Delete = 0 AND CAST(Fin_Voucher.Voc_Date as date) <@uptoDate"
                //        , new NpgsqlParameter("@LedId", ledId)
                //        , new NpgsqlParameter("@YrId", yrId)
                //        , new NpgsqlParameter("@uptoDate", upToDate.ToString("yyyy-MM-dd"))).FirstOrDefault();
                #endregion

                #region linq
                var ledRptAndPmtTmp = await (from voucher in CSISContext.Fin_Voucher
                                             join voucherTr in CSISContext.Fin_Voucher_Trn
                                                 on voucher.Voc_Id equals voucherTr.Voc_Id
                                             where voucher.Yr_Id == yrId
                                                && voucherTr.Led_Id == ledId
                                                && voucherTr.FinVocTr_Delete == false
                                                && voucher.Voc_Delete == false
                                                && voucher.Voc_Date.Date < upToDate
                                                && voucher.BrCode == brCode 
                                                && voucherTr.BrCode == brCode 
                                             select new { voucherTr.Voc_Rpt, voucherTr.Voc_Pmt })
                  .GroupBy(x => 1) // Group by a constant for single-row aggregation
                  .Select(g => new LedgerReceiptAndPaymentsModel
                  {
                      ReceiptAmount = g.Sum(x => x.Voc_Rpt),
                      PaymentAmount = g.Sum(x => x.Voc_Pmt)
                  })
                  .FirstOrDefaultAsync();
                if (ledRptAndPmtTmp != null) ledRptAndPmt = ledRptAndPmtTmp;
                #endregion 

                if (ledRptAndPmt != null)
                {
                    rptAmt = Convert.ToDouble(ledRptAndPmt.ReceiptAmount);
                    pmtAmt = Convert.ToDouble(ledRptAndPmt.PaymentAmount);
                }

                /// calculate ledger balance
                #region do it in handler
                switch (fnlId)
                {
                    case 1:
                    case 4:
                        if (ledId == cashLedId)
                        {
                            ledgerOB += rptAmt - pmtAmt;
                        }
                        else
                        {
                            ledgerOB += pmtAmt - rptAmt;
                        }
                        break;
                    case 2:
                    case 3:
                        ledgerOB += rptAmt - pmtAmt;
                        break;
                }
                #endregion 
            }

            catch (Exception)
            {
                throw;
            }
            return ledgerOB;
        }

        public async Task<DtoLedgerBalance> GetLedgerBalanceWithFnlId(decimal ledId, decimal yrId, DateTime upToDate, string brCode)
        {
            double ledgerOB = 0;
            double rptAmt = 0;
            double pmtAmt = 0;
            int fnlId = 0;
            decimal cashLedId = 0;
            
            LedgerBalanceModel ledObj = new LedgerBalanceModel();
            DtoLedgerBalance dtoLedgerBalance = new();
            LedgerReceiptAndPaymentsModel ledRptAndPmt = new LedgerReceiptAndPaymentsModel();
            try
            {
                /// Get Cash Ledger Id
                cashLedId = await CSISContext.Map_General.Select(x => x.Cash_Led_Id).FirstAsync();
                /// Get OB and final Ledger Id
                #region linq
                var ledObjTmp = await(from trn in CSISContext.Fin_Ledger_Trn
                                      join ledger in CSISContext.Fin_Ledger
                                          on trn.Led_Id equals ledger.Led_Id
                                      join grp in CSISContext.Fin_Ledger_Grp
                                          on ledger.Grp_Id equals grp.Grp_Id
                                      where trn.Yr_Id == yrId
                                         && trn.LedgerTrn_Delete == false
                                         && ledger.Led_Delete == false
                                         && trn.Led_Id == ledId
                                         && trn.BrCode == brCode
                                         && ledger.BrCode == brCode
                                         && trn.BrCode == brCode
                                      select new LedgerBalanceModel
                                      {
                                          LedgerBalance = trn.OB_Amt,
                                          FnlId = grp.Fnl_Id
                                      }).FirstOrDefaultAsync();
                if (ledObjTmp != null) ledObj = ledObjTmp;
                #endregion 

                if (ledObj != null)
                {
                    ledgerOB = Convert.ToDouble(ledObj.LedgerBalance);
                    fnlId = Convert.ToInt16(ledObj.FnlId);
                }
                else
                {
                    ledgerOB = 0;
                    fnlId = 0;
                }
                /// Get receipt and payment amount

                #region linq
                var ledRptAndPmtTmp = await(from voucher in CSISContext.Fin_Voucher
                                            join voucherTr in CSISContext.Fin_Voucher_Trn
                                                on voucher.Voc_Id equals voucherTr.Voc_Id
                                            where voucher.Yr_Id == yrId
                                               && voucherTr.Led_Id == ledId
                                               && voucherTr.FinVocTr_Delete == false
                                               && voucher.Voc_Delete == false
                                               && voucher.Voc_Date.Date < upToDate
                                               && voucher.BrCode == brCode
                                               && voucherTr.BrCode == brCode
                                            select new { voucherTr.Voc_Rpt, voucherTr.Voc_Pmt })
                  .GroupBy(x => 1) // Group by a constant for single-row aggregation
                  .Select(g => new LedgerReceiptAndPaymentsModel
                  {
                      ReceiptAmount = g.Sum(x => x.Voc_Rpt),
                      PaymentAmount = g.Sum(x => x.Voc_Pmt)
                  })
                  .FirstOrDefaultAsync();
                if (ledRptAndPmtTmp != null) ledRptAndPmt = ledRptAndPmtTmp;
                #endregion 

                if (ledRptAndPmt != null)
                {
                    rptAmt = Convert.ToDouble(ledRptAndPmt.ReceiptAmount);
                    pmtAmt = Convert.ToDouble(ledRptAndPmt.PaymentAmount);
                }

                /// calculate ledger balance
                #region do it in handler
                switch (fnlId)
                {
                    case 1:
                    case 4:
                        if (ledId == cashLedId)
                        {
                            ledgerOB += rptAmt - pmtAmt;
                        }
                        else
                        {
                            ledgerOB += pmtAmt - rptAmt;
                        }
                        break;
                    case 2:
                    case 3:
                        ledgerOB += rptAmt - pmtAmt;
                        break;
                }
                #endregion 
                dtoLedgerBalance.Ledger_Balance = ledgerOB;
                dtoLedgerBalance.Fin_Id = fnlId;
                dtoLedgerBalance.Cash_Led_Id = cashLedId;
            }
            catch (Exception)
            {
                throw;
            }
            return dtoLedgerBalance;
        }

        public string GetLedgerNameByLedId(decimal ledId)
        {
            string ledgerName = "";
            var ledName = CSISContext.Fin_Ledger.Where(x => x.Led_Id == ledId).Select(x => x.Led_Name).FirstOrDefault();
            if (ledName != null) ledgerName = ledName;
            return ledgerName;
        }

        public async Task<bool> UpdateLedgerBalance(decimal yrId, DateTime fromDate, DateTime toDate, string brCode)
        {
            bool result = false;
            decimal cashLedId = 0;
            double ledgerbalance = 0, totalReceipts = 0, totalPayments = 0;
            List<FinBal> obList = new List<FinBal>();
            FinBal? trn = new ();
            try
            {
                cashLedId = await GetCashLedgerId(brCode);
                CSISContext.Fin_Ledger_Trn
                .Where(t => t.Yr_Id == yrId)
                .ExecuteUpdate(setters => setters
                    .SetProperty(t => t.Tot_Rpt_Amt, 0)
                    .SetProperty(t => t.Tot_Pmt_Amt, 0)
                    .SetProperty(t => t.CB_Amt, 0));

                #region query
                //var obListTmp = CSISContext.Database.SqlQueryRaw<FinBal>(
                //        @"SELECT Fin_Ledger_Trn.Led_Id, 
                //        Fin_Ledger_Trn.OB_Amt, 
                //        Fin_Ledger_Trn.Tot_Rpt_Amt AS TotalReceipts, 
                //        Fin_Ledger_Trn.Tot_Pmt_Amt AS TotalPayments, 
                //        Fin_Ledger_Trn.CB_Amt, 
                //        Fin_Ledger_Grp.Fnl_Id
                //        FROM Fin_Ledger_Trn INNER JOIN Fin_Ledger ON Fin_Ledger_Trn.Led_Id = Fin_Ledger.Led_Id
                //        INNER JOIN Fin_Ledger_Grp ON Fin_Ledger.Grp_Id = Fin_Ledger_Grp.Grp_Id 
                //        WHERE Fin_Ledger_Trn.Yr_Id = @yrId AND Fin_Ledger_Trn.LedgerTrn_Delete = false AND Fin_Ledger.Led_Delete = false"
                //        , new NpgsqlParameter("@yrId", yrId)).ToList();
                //if(obListTmp !=null && obListTmp.Any())
                //{
                //    obList = obListTmp.ToList();
                //}
                #endregion

                #region linq
                var obListTmp = await (from ledTrn in CSISContext.Fin_Ledger_Trn
                                       join led in CSISContext.Fin_Ledger on ledTrn.Led_Id equals led.Led_Id
                                       join grp in CSISContext.Fin_Ledger_Grp on led.Grp_Id equals grp.Grp_Id
                                       where ledTrn.Yr_Id == yrId
                                          && !ledTrn.LedgerTrn_Delete
                                          && !led.Led_Delete
                                       select new FinBal
                                       {
                                           Led_Id = ledTrn.Led_Id,
                                           OB_Amt = ledTrn.OB_Amt,
                                           TotalReceipts = ledTrn.Tot_Rpt_Amt,
                                           TotalPayments = ledTrn.Tot_Pmt_Amt,
                                           CB_Amt = ledTrn.CB_Amt,
                                           Fnl_Id = grp.Fnl_Id
                                       }).ToListAsync();
                #endregion 
                if (obListTmp != null && obListTmp.Any())
                {
                    obList = obListTmp.ToList();
                }
                foreach (var ob in obList)
                {
                    ledgerbalance = 0;
                    #region sql
                    //trn = CSISContext.Database.SqlQueryRaw<FinBal>(
                    //    @"SELECT Fin_voucher_tr.led_id,
                    //        Sum(Fin_voucher_tr.voc_rpt) AS  TotalReceipts, 
                    //        Sum(Fin_voucher_tr.voc_pmt) AS TotalPayments
                    //        FROM Fin_voucher_tr INNER JOIN Fin_Voucher ON Fin_voucher_tr.voc_Id = Fin_Voucher.Voc_Id
                    //        Where CAST(Fin_Voucher.Voc_Date AS date) BETWEEN @fromDate AND @toDate AND Fin_voucher_tr.yr_id = @yrId  AND Fin_Voucher.Yr_Id = @yrId  
                    //        AND Fin_voucher_tr.FinVocTr_Delete = 0 AND Fin_Voucher.Voc_Delete = 0
                    //        GROUP BY Fin_voucher_tr.led_id
                    //        HAVING Fin_voucher_tr.led_id = @ledId"
                    //    , new NpgsqlParameter("@fromDate", fromDate.ToString("yyyy-MM-dd"))
                    //    , new NpgsqlParameter("@toDate", toDate.ToString("yyyy-MM-dd"))
                    //    , new NpgsqlParameter("@yrId", yrId)
                    //    , new NpgsqlParameter("@ledId", ob.Led_Id)).First();
                    #endregion

                    #region linq
                    var trnTmp = (from vtr in CSISContext.Fin_Voucher_Trn
                           join v in CSISContext.Fin_Voucher on vtr.Voc_Id equals v.Voc_Id
                           where v.Voc_Date.Date >= fromDate.Date
                              && v.Voc_Date.Date <= toDate.Date
                              && vtr.Yr_Id == yrId
                              && v.Yr_Id == yrId
                              && vtr.FinVocTr_Delete == false
                              && v.Voc_Delete == false
                           group vtr by vtr.Led_Id into g
                           where g.Key == ob.Led_Id
                           select new FinBal
                           {
                               Led_Id = g.Key,
                               TotalReceipts = g.Sum(x => (double?)x.Voc_Rpt) ?? 0,
                               TotalPayments = g.Sum(x => (double?)x.Voc_Pmt) ?? 0
                           }).FirstOrDefault();
                    #endregion 
                    if(trnTmp != null) trn = trnTmp; else trn = null;
                    if (trn != null)
                    {

                        totalReceipts = trn.TotalReceipts;
                        totalPayments = trn.TotalPayments;
                        switch (ob.Fnl_Id)
                        {
                            case 1:
                            case 4:
                                if (ob.Led_Id == cashLedId)
                                    ledgerbalance = ob.OB_Amt + trn.TotalReceipts - trn.TotalPayments;
                                else
                                    ledgerbalance = ob.OB_Amt + trn.TotalPayments - trn.TotalReceipts;
                                break;
                            case 2:
                            case 3:
                                ledgerbalance = ob.OB_Amt + trn.TotalReceipts - trn.TotalPayments;
                                break;
                        }
                    }
                    else
                    {
                        totalReceipts = 0; totalPayments = 0;
                        ledgerbalance = ob.OB_Amt;
                    }
                    #region sql
                    //CSISContext.Database.ExecuteSqlRaw(@"Update Fin_Ledger_Trn set CB_Amt = @CB,Tot_Rpt_Amt = @totRpt,Tot_Pmt_Amt = @totPmt where Led_Id = @ledId And Yr_ID = @yrId"
                    //, new NpgsqlParameter("@CB", Math.Round(ledgerbalance, 2))
                    //, new NpgsqlParameter("@totRpt", Math.Round(totalReceipts, 2))
                    //, new NpgsqlParameter("@totPmt", Math.Round(totalPayments, 2))
                    //, new NpgsqlParameter("@ledId", ob.Led_Id)
                    //, new NpgsqlParameter("@yrId", yrId));
                    #endregion

                    #region linq
                    var recordsToUpdate = CSISContext.Fin_Ledger_Trn
                    .Where(t => t.Led_Id == ob.Led_Id && t.Yr_Id == yrId);

                    if (recordsToUpdate != null)
                    {
                        foreach (var record in recordsToUpdate)
                        {
                            record.CB_Amt = Math.Round(ledgerbalance, 2);
                            record.Tot_Rpt_Amt = Math.Round(totalReceipts, 2);
                            record.Tot_Pmt_Amt = Math.Round(totalPayments, 2);
                        }
                        CSISContext.SaveChanges();
                    }
                    #endregion 
                }
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result = false;
            }
            return result;
        }
    }
}
