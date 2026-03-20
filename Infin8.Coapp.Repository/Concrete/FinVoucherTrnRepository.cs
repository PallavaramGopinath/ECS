using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class FinVoucherTrnRepository : Repository<Fin_Voucher_Trn>, IFinVoucherTrnRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public FinVoucherTrnRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddFinVoucherTrnAsync(Fin_Voucher_Trn finVoucherTrn)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Fin_Voucher_Trn.MaxAsync(x => x.Voc_Trn_Id);
                maxId++;
                finVoucherTrn.Voc_Trn_Id = maxId;
                await AddAsync(finVoucherTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Voucher transaction not saved");
            }
            return result;
        }
        public async Task<bool> AddFinVoucherTrnList(List<Fin_Voucher_Trn> finVoucherTrnList)
        {
            bool result = false;
            try
            {
                decimal maxId = CSISContext.Fin_Voucher_Trn.Max(x => x.Voc_Trn_Id);
                foreach (var finVoucherTrn in finVoucherTrnList)
                {
                    maxId++;
                    finVoucherTrn.Voc_Trn_Id = maxId;
                }
                await CSISContext.Fin_Voucher_Trn.AddRangeAsync(finVoucherTrnList);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Voucher transaction not saved");
            }
            return result;
        }

        public async Task<bool> EditFinVoucherTrnAsync(Fin_Voucher_Trn finVoucherTrn)
        {
            bool result = false;
            try
            {
                finVoucherTrn.FinVocTr_Delete = true;
                await EditAsync(finVoucherTrn);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying Voucher transaction (trn)");
            }
            return result;
        }

        public async Task<double> GetLedgerBalanceByLedIdAsync(decimal ledId, decimal cashLedId, decimal yearId)
        {
            double ledgerbalance = 0;
            //double ledgerOB = 0;
            double receiptAmount = 0;
            double paymentAmount = 0;
            int fnlId = 0;
            try
            {
                /// get ob
                var ledObj = await (from trn in CSISContext.Fin_Ledger_Trn
                                    join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                    join grp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals grp.Grp_Id
                                    where trn.Yr_Id == yearId
                                          && trn.LedgerTrn_Delete == false
                                          && ledger.Led_Delete == false
                                          && trn.Led_Id == ledId
                                    select new
                                    {
                                        LedgerBalance = trn.OB_Amt,
                                        FnlId = grp.Fnl_Id
                                    }).FirstOrDefaultAsync();
                if (ledObj != null)
                {
                    ledgerbalance = ledObj.LedgerBalance;
                    fnlId = ledObj.FnlId;
                }
                /// get between dates
                var ledRptAndPmt = await (from voucher in CSISContext.Fin_Voucher
                                          join voucherTr in CSISContext.Fin_Voucher_Trn
                                          on voucher.Voc_Id equals voucherTr.Voc_Id
                                          where voucher.Yr_Id == yearId
                                                && voucherTr.Led_Id == ledId
                                                && voucherTr.FinVocTr_Delete == false
                                                && voucher.Voc_Delete == false
                                          group new { voucherTr } by 1 into g
                                          select new
                                          {
                                              ReceiptAmount = g.Sum(x => x.voucherTr.Voc_Rpt), // ISNULL equivalent
                                              PaymentAmount = g.Sum(x => x.voucherTr.Voc_Pmt) // ISNULL equivalent
                                          }).FirstOrDefaultAsync();
                if (ledRptAndPmt != null)
                {
                    receiptAmount = ledRptAndPmt.ReceiptAmount;
                    paymentAmount = ledRptAndPmt.PaymentAmount;
                }
                /// calculate ledger balance
                switch (fnlId)
                {
                    case 1:
                    case 4:
                        if (ledId == cashLedId)
                        {
                            ledgerbalance += receiptAmount - paymentAmount;
                        }
                        else
                        {
                            ledgerbalance += paymentAmount - receiptAmount;
                        }
                        break;
                    case 2:
                    case 3:
                        ledgerbalance += receiptAmount - paymentAmount;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger balance by led id");
            }
            return ledgerbalance;
        }

        public async Task<double> GetLedgerBalance(decimal ledId, decimal yrId, DateTime upToDate, string brCode)
        {
            double ledgerbalance = 0;
            //double ledgerOB = 0;
            double receiptAmount = 0;
            double paymentAmount = 0;
            int fnlId = 0;
            decimal cashLedId = 0;
            try
            {
                /// get cash ledger id
                cashLedId = await GetCashLedgerId(brCode);
                /// get ob
                var ledObj = await (from trn in CSISContext.Fin_Ledger_Trn
                                    join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                    join grp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals grp.Grp_Id
                                    where trn.Yr_Id == yrId
                                          && trn.LedgerTrn_Delete == false
                                          && ledger.Led_Delete == false
                                          && trn.Led_Id == ledId
                                          && trn.BrCode == brCode
                                          && ledger.BrCode == brCode
                                          && grp.BrCode == brCode
                                    select new
                                    {
                                        LedgerBalance = trn.OB_Amt,
                                        FnlId = grp.Fnl_Id
                                    }).FirstOrDefaultAsync();
                if (ledObj != null)
                {
                    ledgerbalance = ledObj.LedgerBalance;
                    fnlId = ledObj.FnlId;
                }
                /// get between dates
                var ledRptAndPmt = await (from voucher in CSISContext.Fin_Voucher
                                          join voucherTr in CSISContext.Fin_Voucher_Trn
                                          on voucher.Voc_Id equals voucherTr.Voc_Id
                                          where voucher.Yr_Id == yrId
                                                && voucherTr.Led_Id == ledId
                                                && voucherTr.FinVocTr_Delete == false
                                                && voucher.Voc_Delete == false
                                                && voucher.BrCode == brCode
                                                && voucherTr.BrCode == brCode
                                                && voucher.Voc_Date <= upToDate
                                          group new { voucherTr } by 1 into g
                                          select new
                                          {
                                              ReceiptAmount = g.Sum(x => x.voucherTr.Voc_Rpt), // ISNULL equivalent
                                              PaymentAmount = g.Sum(x => x.voucherTr.Voc_Pmt) // ISNULL equivalent
                                          }).FirstOrDefaultAsync();
                if (ledRptAndPmt != null)
                {
                    receiptAmount = ledRptAndPmt.ReceiptAmount;
                    paymentAmount = ledRptAndPmt.PaymentAmount;
                }
                /// calculate ledger balance
                switch (fnlId)
                {
                    case 1:
                    case 4:
                        if (ledId == cashLedId)
                        {
                            ledgerbalance += receiptAmount - paymentAmount;
                        }
                        else
                        {
                            ledgerbalance += paymentAmount - receiptAmount;
                        }
                        break;
                    case 2:
                    case 3:
                        ledgerbalance += receiptAmount - paymentAmount;
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while fetching ledger balance by led id");
            }
            return ledgerbalance;
        }

        public async Task<decimal> GetCashLedgerId(string brCode)
        {
            return await CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefaultAsync();
        }

        public async Task<DtoVoucher> GetTransactionById(decimal vocId, string brCode)
        {
            DtoVoucher dtoVoucher = new();
            try
            {
                decimal cashLedId = CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
                // This query populates the master-detail ViewModel directly
                var voucherViewModel = await CSISContext.Fin_Voucher
                    .Where(v => v.Voc_Id == vocId && !v.Voc_Delete && v.BrCode == brCode )
                    .Select(v => new DtoVoucher // Project into your ViewModel
                    {
                        // Master Details
                        Voc_Id = v.Voc_Id,
                        Voc_Rpt_No = v.Voc_Rpt_No,
                        Voc_Pmt_No = v.Voc_Pmt_No,
                        Voc_Date = v.Voc_Date,
                        Voc_Type = v.Voc_Type,
                        Voc_Narration = v.Voc_Narration
                    }).FirstOrDefaultAsync();
                if (voucherViewModel != null) dtoVoucher = voucherViewModel;

                var trans = await (from trn in CSISContext.Fin_Voucher_Trn
                                   join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                   where trn.Led_Id != cashLedId && trn.Voc_Id == vocId && trn.FinVocTr_Delete == false
                                   && trn.BrCode == brCode
                                   orderby trn.Voc_Trn_Id
                                   select new DtoVoucherTrn
                                   {
                                       Led_Id = trn.Led_Id,
                                       Led_Name = ledger.Led_Name,
                                       Voc_Rpt = trn.Voc_Rpt,
                                       Voc_Pmt = trn.Voc_Pmt,
                                       Voc_Narr = trn.Voc_Narr,
                                       Voc_Trn_Type = trn.Voc_Trn_Type,
                                       BrCode = trn.BrCode,
                                   }).ToListAsync();
                if (trans != null)
                {
                    dtoVoucher.Transactions = trans;
                    dtoVoucher.brCode = brCode;
                }
            }
            catch (Exception)
            {
                dtoVoucher = new();
            }
            return dtoVoucher;
        }

        public async Task<DtoVoucher> GetTransactionByIds(decimal vocId, decimal yrId, string brCode)
        {
            DtoVoucher dtoVoucher = new();
            try
            {
                decimal cashLedId = CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
                // This query populates the master-detail ViewModel directly
                var voucherViewModel = await CSISContext.Fin_Voucher
                    .Where(v => v.Voc_Id == vocId && !v.Voc_Delete && v.Yr_Id == yrId && v.BrCode == brCode)
                    .Select(v => new DtoVoucher // Project into your ViewModel
                    {
                        // Master Details
                        Voc_Id = v.Voc_Id,
                        Voc_Rpt_No = v.Voc_Rpt_No,
                        Voc_Pmt_No = v.Voc_Pmt_No,
                        Voc_Date = v.Voc_Date,
                        Voc_Type = v.Voc_Type,
                        Voc_Narration = v.Voc_Narration
                    }).FirstOrDefaultAsync();
                if (voucherViewModel != null) dtoVoucher = voucherViewModel;

                var trans = await (from trn in CSISContext.Fin_Voucher_Trn
                                   join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                   where trn.Led_Id != cashLedId && trn.Voc_Id == vocId && trn.FinVocTr_Delete == false
                                   && trn.Yr_Id == yrId && trn.BrCode == brCode
                                   orderby trn.Voc_Trn_Id
                                   select new DtoVoucherTrn
                                   {
                                       Led_Id = trn.Led_Id,
                                       Led_Name = ledger.Led_Name,
                                       Voc_Rpt = trn.Voc_Rpt,
                                       Voc_Pmt = trn.Voc_Pmt,
                                       Voc_Narr = trn.Voc_Narr,
                                       Voc_Trn_Type = trn.Voc_Trn_Type,
                                   }).ToListAsync();
                if (trans != null)
                {
                    dtoVoucher.Transactions = trans;
                }
            }
            catch (Exception)
            {
                dtoVoucher = new();
            }
            return dtoVoucher;
        }

        public async Task<DtoVoucher> GetTransactionByNo(string rptNo, string pmtNo, decimal yrId, string brCode)
        {
            DtoVoucher dtoVoucher = new();
            decimal vocId = 0;
            try
            {
                if (rptNo.Length > 0)
                {
                    var vocModal = await CSISContext.Fin_Voucher
                    .Where(v => v.Voc_Rpt_No == rptNo && v.Yr_Id == yrId && v.BrCode == brCode && !v.Voc_Delete)
                    .Select(v => new DtoVoucher // Project into your ViewModel
                    {
                        // Master Details
                        Voc_Id = v.Voc_Id,
                        Voc_Rpt_No = v.Voc_Rpt_No,
                        Voc_Pmt_No = v.Voc_Pmt_No,
                        Voc_Date = v.Voc_Date,
                        Voc_Type = v.Voc_Type,
                        Voc_Narration = v.Voc_Narration,
                        brCode = v.BrCode
                    }).FirstOrDefaultAsync();
                    if (vocModal != null) dtoVoucher = vocModal;
                }
                if (pmtNo.Length > 0)
                {
                    var vocModal = await CSISContext.Fin_Voucher
                    .Where(v => v.Voc_Pmt_No == pmtNo && v.Yr_Id == yrId && v.BrCode == brCode && !v.Voc_Delete)
                    .Select(v => new DtoVoucher // Project into your ViewModel
                    {
                        // Master Details
                        Voc_Id = v.Voc_Id,
                        Voc_Rpt_No = v.Voc_Rpt_No,
                        Voc_Pmt_No = v.Voc_Pmt_No,
                        Voc_Date = v.Voc_Date,
                        Voc_Type = v.Voc_Type,
                        Voc_Narration = v.Voc_Narration,
                        brCode = v.BrCode
                    }).FirstOrDefaultAsync();
                    if (vocModal != null) dtoVoucher = vocModal;
                }
                decimal cashLedId = CSISContext.Map_General.Where(x => x.BrCode == dtoVoucher.brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
                vocId = dtoVoucher.Voc_Id;

                var trans = await (from trn in CSISContext.Fin_Voucher_Trn
                                   join ledger in CSISContext.Fin_Ledger on trn.Led_Id equals ledger.Led_Id
                                   where trn.Led_Id != cashLedId && trn.Voc_Id == vocId
                                   && trn.BrCode == brCode
                                   && trn.FinVocTr_Delete == false
                                   && ledger.BrCode == brCode
                                   orderby trn.Voc_Trn_Id
                                   select new DtoVoucherTrn
                                   {
                                       Led_Id = trn.Led_Id,
                                       Led_Name = ledger.Led_Name,
                                       Voc_Rpt = trn.Voc_Rpt,
                                       Voc_Pmt = trn.Voc_Pmt,
                                       Voc_Narr = trn.Voc_Narr,
                                       Voc_Trn_Type = trn.Voc_Trn_Type,
                                   }).ToListAsync();
                if (trans != null)
                {
                    dtoVoucher.Transactions = trans;
                }
            }
            catch (Exception ex)
            {
                dtoVoucher = new();
                Console.Write(ex.Message);
            }
            return dtoVoucher;
        }

        public async Task<List<FinBal>> GetLedgerBalanceMonthWise(decimal ledId, decimal yrId, string brCode)
        {
            List<FinBal> balList = new();
            try
            {
                var query = await (from fv in CSISContext.Fin_Voucher
                                   join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id
                                   join fl in CSISContext.Fin_Ledger on fvt.Led_Id equals fl.Led_Id
                                   join flg in CSISContext.Fin_Ledger_Grp on fl.Grp_Id equals flg.Grp_Id
                                   join flf in CSISContext.Fin_Ledger_Fnl on flg.Fnl_Id equals flf.Fnl_Id
                                   join flt in CSISContext.Fin_Ledger_Trn on fvt.Led_Id equals flt.Led_Id
                                   where fv.Voc_Delete == false
                                         && fvt.FinVocTr_Delete == false
                                         && fv.Yr_Id == yrId
                                         && flt.Yr_Id == yrId
                                         && fvt.Led_Id == ledId
                                         && fv.BrCode == brCode
                                         && fvt.BrCode == brCode
                                         && fl.BrCode == brCode
                                         && flt.BrCode == brCode
                                   group new { fvt, flt, flg, fl, fv } by new
                                   {
                                       flt.OB_Amt,
                                       fvt.Led_Id,
                                       flg.Fnl_Id,
                                       fl.Led_Name,
                                       GLYear = fv.Voc_Date.Year,
                                       GLMonth = fv.Voc_Date.Month
                                   } into g
                                   orderby g.Key.GLYear, g.Key.GLMonth
                                   select new FinBal
                                   {
                                       OB_Amt = g.Key.OB_Amt,
                                       Led_Id = g.Key.Led_Id,
                                       Fnl_Id = g.Key.Fnl_Id,
                                       Led_Name = g.Key.Led_Name,
                                       TotalReceipts = g.Sum(x => x.fvt.Voc_Rpt),
                                       TotalPayments = g.Sum(x => x.fvt.Voc_Pmt),
                                       GLYear = g.Key.GLYear,
                                       GLMonth = g.Key.GLMonth
                                   }).ToListAsync();
                if (query != null && query.Count > 0) balList = query.ToList();
            }
            catch (Exception)
            {
                balList = new();
            }
            return balList;
        }

        public async Task<List<FinBalGL>> GetLedgerBalanceDateWise(decimal ledId, decimal yrId, int month, int year, string brCode)
        {
            DateTime fromDate = new DateTime(year, month, 1);
            DateTime toDate = fromDate.AddMonths(1);
            toDate = toDate.AddDays(-1);
            List<FinBalGL> balList = new();
            try
            {
                var query = await (from voc in CSISContext.Fin_Voucher
                                   join tr in CSISContext.Fin_Voucher_Trn on voc.Voc_Id equals tr.Voc_Id
                                   join ledTrn in CSISContext.Fin_Ledger_Trn on tr.Led_Id equals ledTrn.Led_Id
                                   join led in CSISContext.Fin_Ledger on tr.Led_Id equals led.Led_Id
                                   join grp in CSISContext.Fin_Ledger_Grp on led.Grp_Id equals grp.Grp_Id
                                   where (voc.Voc_Date >=fromDate.Date && voc.Voc_Date <= toDate.Date)
                                   && voc.Yr_Id == yrId
                                   && voc.BrCode == brCode
                                   && voc.Voc_Delete == false

                                   && tr.Led_Id == ledId
                                   && tr.Yr_Id == yrId
                                   && tr.BrCode == brCode
                                   && tr.FinVocTr_Delete == false

                                   && ledTrn.Yr_Id == yrId
                                   && ledTrn.BrCode == brCode
                                   && grp.BrCode == brCode

                                   group new { tr, voc, led, grp } by new { tr.Led_Id, voc.Voc_Date,led.Led_Name , grp.Fnl_Id } into g
                                   orderby g.Key.Voc_Date
                                   select new FinBalGL
                                   {
                                       GL_Date = g.Key.Voc_Date,
                                       Led_Id = g.Key.Led_Id ,
                                       Led_Name = g.Key.Led_Name ,
                                       GL_Receipts = g.Sum(x => x.tr.Voc_Rpt),
                                       GL_Payments = g.Sum(x => x.tr.Voc_Pmt),
                                       GL_CB = 0,
                                   }).ToListAsync();
                if (query != null && query.Count > 0) balList = query.ToList();
            }
            catch (Exception)
            {
                balList = new();
            }
            return balList;
        }

        public async Task<List<FinBalVoucherTrn>> GetLedgerBalanceSlipWise(decimal ledId, decimal yrId, DateTime vocDate, string brCode)
        {
            List<FinBalVoucherTrn> balList = new();
            try
            {
                var query = await (from voc in CSISContext.Fin_Voucher
                                   join vocTrn in CSISContext.Fin_Voucher_Trn on voc.Voc_Id equals vocTrn.Voc_Id
                                   join led in CSISContext.Fin_Ledger on vocTrn.Led_Id equals led.Led_Id
                                   where voc.Voc_Date == vocDate 
                                   && voc.Yr_Id == yrId
                                   && voc.BrCode == brCode
                                   && voc.Voc_Delete == false
                                   && vocTrn.Led_Id == ledId
                                   && vocTrn.Yr_Id == yrId
                                   && vocTrn.BrCode == brCode
                                   && vocTrn.FinVocTr_Delete == false
                                   && led.BrCode == brCode
                                   group  new {voc,vocTrn, led} by new {voc.Voc_Id,voc.Voc_No, voc.Voc_Date,voc.Voc_Type, vocTrn.Led_Id,led.Led_Name } into g
                                   select new FinBalVoucherTrn
                                   {
                                       Voc_Id = g.Key.Voc_Id,
                                       Voc_No  = g.Key.Voc_No ,
                                       Voc_Date = g.Key.Voc_Date,
                                       Voc_Type = g.Key.Voc_Type,
                                       Led_Id = g.Key.Led_Id,
                                       Led_Name = g.Key.Led_Name,
                                       Voc_Rpt = g.Sum(x=> x.vocTrn.Voc_Rpt),
                                       Voc_Pmt = g.Sum(x=> x.vocTrn.Voc_Pmt),
                                   }).ToListAsync();
                if (query != null && query.Count > 0) balList = query.ToList();
            }
            catch (Exception)
            {
                balList = new();
            }
            return balList;
        }
    }
}
