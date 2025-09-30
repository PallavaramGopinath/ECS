using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;
//using Microsoft.JSInterop;
//using System.Net.Http.Json;
namespace Infin8.Coapp.Repository
{
    public class ReportsRepository : Repository<Reports_Master>, IReportsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        //private readonly IJSRuntime _jsRuntime;
        //private readonly HttpClient _httpClient;
        
        public ReportsRepository(CSISContext context ) : base(context)
        {
            //_jsRuntime = jsRuntime;
            //_httpClient = httpClient;
        }

        public async Task<List<DropdownItem>> GetReportNameList(int grpId,string brCode)
        {
            List<DropdownItem> reportList = new();
            try
            {
                var result = await (from r in CSISContext.Reports_Master
                                    where r.ReportGrp_Id == grpId && r.BrCode == brCode && r.ReportDelete == false
                                    select new DropdownItem
                                    {
                                        Value = r.Report_Id.ToString(),
                                        Text = r.ReportName
                                    }).ToListAsync();
                if (result.Count > 0) reportList = result.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                reportList = new();
            }
            return reportList;
        }
        #region Report name and id
        public async Task<int> GetReportId(string reportName)
        {
            return await  CSISContext.Reports_Master.Where(x => x.ReportName == reportName).Select(x => x.Report_Id).FirstOrDefaultAsync();
        }
        public async Task<Reports_Master> GetReportNameWithSignature(int reportId)
        {
            Reports_Master report = new Reports_Master();
            report =   await CSISContext.Reports_Master.Where(x => x.Report_Id == reportId).FirstAsync();
            return report;
        }
        public async  Task<Reports_Master> GetReportNameWithSignature(string reportName)
        {
            Reports_Master report = new Reports_Master();
            report = await CSISContext.Reports_Master.Where(x => x.ReportName == reportName).FirstAsync();
            return report;
        }
        #endregion

        #region Status
        public async Task<List<string>> GetStatusForMemberTransaction(decimal vocId,string brCode)
        {
            List<string> status = new();
            try
            {
                var result = await  (from fvt in CSISContext.Fin_Voucher_Trn
                              where fvt.Voc_Id == vocId && fvt.BrCode == brCode && fvt.FinVocTr_Delete == false && fvt.Status!.Length > 0
                              select fvt.Status).Distinct().ToListAsync();
                if (result.Count > 0) status = result.ToList();
            }
            catch (Exception ex)
            {
                status = new();
                Console.WriteLine(ex.Message);
            }
            return status;
        }

        #endregion

        #region Receipt and Payment
        public async Task<List<rptReceiptAndPaymentAmount>> GetReceiptAndPaymentAmount(decimal vocId,string brCode)
        {
            List<rptReceiptAndPaymentAmount> rptPmtList = new();
            //rptReceiptAndPaymentAmount? rptAmtPmt = new rptReceiptAndPaymentAmount();
            bool isChequeOnlyReceipt = false;
            try
            {
                var bankLedgerIds = CSISContext.Map_Banks.Select(b => b.Led_Id).ToList();
                var cashId = CSISContext.Map_General.Where(x => x.BrCode == brCode).Select(x => x.Cash_Led_Id).FirstOrDefault();
                #region Receipt items

                //var voc_rpt_sum = (from fv in CSISContext.Fin_Voucher
                //                   join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id
                //                   // The join to Fin_Ledger is not necessary as it's not used for filtering or selection.
                //                   // It has been removed for optimization. See note below.
                //                   where fv.Voc_Id == vocId
                //                         && fv.Voc_Delete == false
                //                         && fvt.FinVocTr_Delete == false
                //                         && fv.BrCode == brCode 
                //                         && fvt.BrCode == brCode 
                //                         && !CSISContext.Map_General.Any(m => m.Cash_Led_Id == fvt.Led_Id)
                //                         && !bankLedgerIds.Contains(fvt.Led_Id) // Handles the NOT IN clause
                //                   select fvt.Voc_Rpt)
                   //.Sum();

                var voc_rpt_sum = (from v in CSISContext.Fin_Voucher
                          join vt in CSISContext.Fin_Voucher_Trn on v.Voc_Id equals vt.Voc_Id
                          join l in CSISContext.Fin_Ledger on vt.Led_Id equals l.Led_Id
                          where v.Voc_Id == vocId
                                && v.Voc_Delete == false
                                && vt.FinVocTr_Delete == false
                                && vt.Led_Id != cashId
                                && !CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(vt.Led_Id)
                          select vt.Voc_Rpt).Sum();

                if (voc_rpt_sum > 0)
                {
                    isChequeOnlyReceipt = false;
                }
                if (voc_rpt_sum > 0)
                {
                    //var cheque_rpt_sum = (from fv in CSISContext.Fin_Voucher
                    //                      join fvt in CSISContext.Fin_Voucher_Trn on fv.Voc_Id equals fvt.Voc_Id
                    //                      // The join to Fin_Ledger is not necessary as it's not used for filtering or selection.
                    //                      // It has been removed for optimization. See note below.
                    //                      where fv.Voc_Id == vocId
                    //                            && fv.Voc_Delete == false
                    //                            && fvt.FinVocTr_Delete == false
                    //                            && fv.BrCode == brCode
                    //                            && fvt.BrCode == brCode
                    //                            && !CSISContext.Map_General.Any(m => m.Cash_Led_Id == fvt.Led_Id)
                    //                            && bankLedgerIds.Contains(fvt.Led_Id) // Handles the NOT IN clause
                    //                      select fvt.Voc_Rpt)
                    //   .Sum();

                    var cheque_rpt_sum = (from v in CSISContext.Fin_Voucher
                              join vt in CSISContext.Fin_Voucher_Trn on v.Voc_Id equals vt.Voc_Id
                              join l in CSISContext.Fin_Ledger on vt.Led_Id equals l.Led_Id
                              where v.Voc_Id == vocId
                                    && v.Voc_Delete == false
                                    && vt.FinVocTr_Delete == false
                                    && vt.Led_Id != cashId
                                    && CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(vt.Led_Id)
                              select vt.Voc_Rpt).Sum();

                    if (cheque_rpt_sum > 0)
                        isChequeOnlyReceipt = true;
                }

                if(isChequeOnlyReceipt == false)
                {
                    var result = await (from vocTrn in CSISContext.Fin_Voucher_Trn
                                        join voc in CSISContext.Fin_Voucher on vocTrn.Voc_Id equals voc.Voc_Id
                                        where vocTrn.Voc_Id == vocId &&
                                              voc.BrCode == brCode &&
                                              vocTrn.BrCode == brCode &&
                                              !CSISContext.Map_General.Any(m => m.Cash_Led_Id == vocTrn.Led_Id) &&
                                              vocTrn.FinVocTr_Delete == false
                                        group new { voc, vocTrn } by new { voc.Voc_Type, vocTrn.Voc_Trn_Type } into g
                                        select new rptReceiptAndPaymentAmount
                                        {
                                            Voc_Type = g.Key.Voc_Type,
                                            Voc_Trn_Type = g.Key.Voc_Trn_Type,
                                            Voc_Rpt = g.Sum(x => x.vocTrn.Voc_Rpt),
                                            Voc_Pmt = g.Sum(x => x.vocTrn.Voc_Pmt)
                                        }).ToListAsync();

                    if (result.Count > 0) rptPmtList.AddRange(result);
                }
                if(isChequeOnlyReceipt == true)
                {
                    // First, get the relevant voucher transactions
                    var voucherTransactions = await (from vt in CSISContext.Fin_Voucher_Trn
                                                     join v in CSISContext.Fin_Voucher on vt.Voc_Id equals v.Voc_Id
                                                     where vt.Voc_Id == vocId
                                                           && vt.Led_Id != cashId
                                                           && vt.FinVocTr_Delete == false
                                                     select new { vt, v.Voc_Type })
                                              .ToListAsync();

                    // Get bank ledger IDs
                    var bankLedIds = await CSISContext.Map_Banks
                        .Select(mb => mb.Led_Id)
                        .ToListAsync();

                    // Filter and group in memory
                    var result = voucherTransactions
                        .Where(x => bankLedIds.Contains(x.vt.Led_Id))
                        .GroupBy(x => new { x.Voc_Type, x.vt.Voc_Trn_Type })
                        .Select(g => new
                        {
                            Key = g.Key,
                            SumVocRpt = g.Sum(x => x.vt.Voc_Rpt)
                        })
                        .Where(x => x.SumVocRpt > 0)
                        .Select(x => new rptReceiptAndPaymentAmount
                        {
                            Voc_Type = x.Key.Voc_Type,
                            Voc_Trn_Type = x.Key.Voc_Trn_Type,
                            Voc_Rpt = x.SumVocRpt,
                            Voc_Pmt = 0.0
                        })
                        .ToList();
                    if (result.Count > 0) rptPmtList.AddRange(result);
                }
                #endregion 
            }
            catch (Exception ex)
            {
                rptPmtList = new();
                Console.WriteLine(ex.Message);
            }
            return rptPmtList;
        }
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount2(decimal vocId, string brCode)
        {
            rptReceiptAndPaymentAmount? rptAmtPmt = new rptReceiptAndPaymentAmount();
            try
            {
                var result = await(from vocTrn in CSISContext.Fin_Voucher_Trn
                                   join voc in CSISContext.Fin_Voucher on vocTrn.Voc_Id equals voc.Voc_Id
                                   where vocTrn.Voc_Id == vocId &&
                                         !CSISContext.Map_General.Any(m => m.Cash_Led_Id == vocTrn.Led_Id) &&
                                         vocTrn.FinVocTr_Delete == false && voc.BrCode == brCode 
                                   group new { voc, vocTrn } by new { voc.Voc_Type, vocTrn.Voc_Trn_Type } into g
                                   select new rptReceiptAndPaymentAmount
                                   {
                                       Voc_Type = g.Key.Voc_Type,
                                       Voc_Trn_Type = g.Key.Voc_Trn_Type,
                                       Voc_Rpt = g.Sum(x => x.vocTrn.Voc_Rpt),
                                       Voc_Pmt = g.Sum(x => x.vocTrn.Voc_Pmt)
                                   }).FirstOrDefaultAsync();
                if (result != null) rptAmtPmt = result;

            }
            catch (Exception)
            {

                throw;
            }
            return rptAmtPmt!;
        }
        public async Task<(List<rptReceiptMemberList> receiptData, DateTime? intCalcDate)> GetReceiptData(decimal vocId,string brCode)
        {
            List<rptReceiptMemberList> receiptData = new List<rptReceiptMemberList>();
            DateTime? intCalcDate = null;
            //bool isChequeOnlyReceipt = false;
            DateTime? trnDate = null;
            double receiptAmount = 0;
            decimal cashLedger = 0;
            try
            {
                /// get Cash ledger id
                cashLedger = CSISContext.Map_General.Where(x => x.ID == 110010000001).Select(x => x.Cash_Led_Id).FirstOrDefault();

                /// get cash receipt items
                var notBankReceipt =  (from voucher in CSISContext.Fin_Voucher
                                      join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                      join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                      where voucher.Voc_Id == vocId
                                      && voucher.BrCode == brCode
                                      && voucher.Voc_Delete == false
                                      && voucherTrn.FinVocTr_Delete == false
                                      && voucherTrn.BrCode == brCode    
                                      && voucherTrn.Led_Id != cashLedger
                                      && !CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(voucherTrn.Led_Id)
                                      select voucherTrn.Voc_Rpt)
                .Sum();

                double.TryParse(notBankReceipt.ToString(), out receiptAmount);
                //if (receiptAmount > 0)
                //{
                //    isChequeOnlyReceipt = false;
                //}

                /// get cheque receipt items
                if (receiptAmount == 0)
                {
                    var bankReceipt = (from voucher in CSISContext.Fin_Voucher
                                       join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                       join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                       where voucher.Voc_Id == vocId
                                       && voucher.Voc_Delete == false
                                       && voucherTrn.FinVocTr_Delete == false
                                       && voucher.BrCode == brCode 
                                       && voucherTrn.BrCode == brCode 
                                       && voucherTrn.Led_Id != cashLedger
                                       && CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(voucherTrn.Led_Id)
                                       select voucherTrn.Voc_Rpt)
                .Sum();
                    double.TryParse(bankReceipt.ToString(), out receiptAmount);
                    //if (receiptAmount > 0)
                    //    isChequeOnlyReceipt = true;
                }

                if (receiptAmount > 0)
                {
                    var result = await (from voucher in CSISContext.Fin_Voucher
                                  join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                  join member in CSISContext.mem_master on voucher.Mem_Id equals member.mem_id
                                  join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                  join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                  where voucher.Voc_Id == vocId
                                  && voucher.Voc_Delete == false
                                  && voucherTrn.FinVocTr_Delete == false
                                  && voucher.BrCode == brCode 
                                  && voucherTrn.BrCode == brCode    
                                  && voucherTrn.Led_Id != cashLedger
                                  && !CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(voucherTrn.Led_Id)
                                  group new { voucher, voucherTrn, member, ledger, ledgerGrp } by new
                                  {
                                      member.memberno,
                                      member.perno,
                                      member.membername,
                                      voucher.Voc_Type,
                                      voucher.Voc_Rpt_No,
                                      voucher.Voc_Date,
                                      ledger.Led_Name,
                                      voucherTrn.Description,
                                      ledgerGrp.Grp_SlNo,
                                      ledger.Led_SlNo,
                                      ledger.Led_Desc
                                  } into g
                                  where g.Sum(x => x.voucherTrn.Voc_Rpt) > 0
                                  orderby g.Key.Grp_SlNo, g.Key.Led_SlNo
                                  select new rptReceiptMemberList
                                  {
                                      MemberNo = g.Key.memberno,
                                      PerNo = g.Key.perno,
                                      MemberName = g.Key.membername,
                                      Voc_Type = g.Key.Voc_Type,
                                      Voc_Rpt_No = g.Key.Voc_Rpt_No,
                                      Voc_Date = g.Key.Voc_Date,
                                      Led_Name = g.Key.Led_Name,
                                      Voc_Rpt = g.Sum(x => x.voucherTrn.Voc_Rpt),
                                      Outstanding = g.Sum(x => x.voucherTrn.Outstanding),
                                      Balance = g.Sum(x => x.voucherTrn.Balance),
                                      Description = g.Key.Description,
                                      Grp_SlNo = g.Key.Grp_SlNo,
                                      Led_SlNo = g.Key.Led_SlNo,
                                      Led_Desc = g.Key.Led_Desc
                                  }).ToListAsync();
                    receiptData =  result.ToList();
                }
                else
                {
                    var result = await (from voucher in CSISContext.Fin_Voucher
                                  join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                  join member in CSISContext.mem_master on voucher.Mem_Id equals member.mem_id
                                  join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                  join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                  where voucher.Voc_Id == vocId
                                  && voucher.Voc_Delete == false
                                  && voucherTrn.FinVocTr_Delete == false
                                  && voucher.BrCode == brCode 
                                  && voucherTrn.BrCode == brCode 
                                  && voucherTrn.Led_Id != cashLedger
                                  && CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(voucherTrn.Led_Id)
                                  group new { voucher, voucherTrn, member, ledger, ledgerGrp } by new
                                  {
                                      member.memberno,
                                      member.perno,
                                      member.membername,
                                      voucher.Voc_Type,
                                      voucher.Voc_Rpt_No,
                                      voucher.Voc_Date,
                                      ledger.Led_Name,
                                      voucherTrn.Description,
                                      ledgerGrp.Grp_SlNo,
                                      ledger.Led_SlNo,
                                      ledger.Led_Desc
                                  } into g
                                  where g.Sum(x => x.voucherTrn.Voc_Rpt) > 0
                                  orderby g.Key.Grp_SlNo, g.Key.Led_SlNo
                                  select new rptReceiptMemberList
                                  {
                                      MemberNo = g.Key.memberno,
                                      PerNo = g.Key.perno,
                                      MemberName = g.Key.membername,
                                      Voc_Type = g.Key.Voc_Type,
                                      Voc_Rpt_No = g.Key.Voc_Rpt_No,
                                      Voc_Date = g.Key.Voc_Date,
                                      Led_Name = g.Key.Led_Name,
                                      Voc_Rpt = g.Sum(x => x.voucherTrn.Voc_Rpt),
                                      Outstanding = g.Sum(x => x.voucherTrn.Outstanding),
                                      Balance = g.Sum(x => x.voucherTrn.Balance),
                                      Description = g.Key.Description,
                                      Grp_SlNo = g.Key.Grp_SlNo,
                                      Led_SlNo = g.Key.Led_SlNo,
                                      Led_Desc = g.Key.Led_Desc
                                  }).ToListAsync();
                    receiptData = result.ToList();
                }

                trnDate = CSISContext.Loan_Trn.Where(x=> x.Voc_Id == vocId).Select(x=> x.Trn_Date).FirstOrDefault();
                if(trnDate != null)
                {
                    var result = (from loanTrn in CSISContext.Loan_Trn 
                                  where loanTrn.Voc_Id == vocId
                                  select loanTrn.IntCalc_Date).Max();
                    intCalcDate = result;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return (receiptData, intCalcDate);
        }
        public async Task<(List<rptReceiptMemberList> receiptData, string chequeDetails)> GetReceiptGeneralData(decimal vocId, string brCode)
        {
            List<rptReceiptMemberList> receiptData = new List<rptReceiptMemberList>();
            string chequeDetails = "";
            double receiptAmount = 0;
            decimal cashLedger = 0;
            try
            {
                /// get Cash ledger id
                cashLedger = CSISContext.Map_General.Where(x => x.ID == 110010000001).Select(x => x.Cash_Led_Id).FirstOrDefault();

                /// get cash receipt items
                var notBankReceipt = (from voucher in CSISContext.Fin_Voucher
                        join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                        join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                        where voucher.Voc_Id == vocId
                        && voucher.BrCode == brCode
                        && voucher.Voc_Delete == false
                        && voucherTrn.FinVocTr_Delete == false
                        && voucherTrn.BrCode == brCode
                        && voucherTrn.Led_Id != cashLedger
                        && !CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(voucherTrn.Led_Id)
                        select voucherTrn.Voc_Rpt)
                .Sum();

                double.TryParse(notBankReceipt.ToString(), out receiptAmount);

                /// get cheque receipt items
                if (receiptAmount == 0)
                {
                    var bankReceipt = (from voucher in CSISContext.Fin_Voucher
                                       join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                       join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                       where voucher.Voc_Id == vocId
                                       && voucher.Voc_Delete == false
                                       && voucherTrn.FinVocTr_Delete == false
                                       && voucherTrn.Led_Id != cashLedger
                                       && CSISContext.Map_Banks.Select(mb => mb.Led_Id).Contains(voucherTrn.Led_Id)
                                       select voucherTrn.Voc_Rpt)
                .Sum();
                    double.TryParse(bankReceipt.ToString(), out receiptAmount);
                }

                if (receiptAmount > 0)
                {
                    var result = await (from voucher in CSISContext.Fin_Voucher
                                 join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                 join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                 join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                 join memberMaster in CSISContext.mem_master on voucherTrn.Mem_Id equals memberMaster.mem_id into memberJoin
                                 from member in memberJoin.DefaultIfEmpty()
                                 where voucher.Voc_Id == vocId
                                    && voucherTrn.Voc_Rpt > 0
                                    && voucherTrn.FinVocTr_Delete == false
                                    && voucherTrn.Led_Id != cashLedger
                                    && voucherTrn.BrCode == brCode
                                    && !CSISContext.Map_Banks.Select(b => b.Led_Id).Contains(voucherTrn.Led_Id)
                                 group new { voucher, voucherTrn, ledger, ledgerGrp, member } by new
                                 {
                                     voucher.Voc_Rpt_No,
                                     voucher.Voc_Date,
                                     ReceipentInVoucher = voucher.Voc_Narration,
                                     ledger.Led_Name,
                                     voucherTrn.Description,
                                     member.memberno,
                                     member.membername,
                                     ledgerGrp.Grp_SlNo,
                                     ledger.Led_SlNo,
                                     ledger.Led_Desc
                                 } into g
                                 orderby g.Key.Grp_SlNo, g.Key.Led_SlNo
                                 select new  rptReceiptMemberList
                                 {
                                     Voc_Rpt_No = g.Key.Voc_Rpt_No,
                                     Voc_Date =     g.Key.Voc_Date,
                                     ReceipentInVoucher = g.Key.ReceipentInVoucher,
                                     Led_Name = g.Key.Led_Name,
                                     Voc_Rpt = g.Sum(x => x.voucherTrn.Voc_Rpt),
                                     Description = g.Key.Description,
                                     MemberNo =  g.Key.memberno,
                                     MemberName =  g.Key.membername,
                                     Grp_SlNo = g.Key.Grp_SlNo,
                                     Led_SlNo = g.Key.Led_SlNo,
                                     Led_Desc = g.Key.Led_Desc
                                 }).ToListAsync();
                    receiptData = result.ToList();
                }
                else
                {
                    var resultBank = await (from voucher in CSISContext.Fin_Voucher
                                join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                join memberMaster in CSISContext.mem_master on voucherTrn.Mem_Id equals memberMaster.mem_id into memberJoin
                                from member in memberJoin.DefaultIfEmpty()
                                where voucher.Voc_Id == vocId
                                    && voucherTrn.Voc_Rpt > 0
                                    && voucherTrn.FinVocTr_Delete == false
                                    && voucherTrn.Led_Id != cashLedger
                                    && voucherTrn.BrCode == brCode
                                    && CSISContext.Map_Banks.Select(b => b.Led_Id).Contains(voucherTrn.Led_Id)
                                group new { voucher, voucherTrn, ledger, ledgerGrp, member } by new
                                {
                                    voucher.Voc_Rpt_No,
                                    voucher.Voc_Date,
                                    ReceipentInVoucher = voucher.Voc_Narration,
                                    ledger.Led_Name,
                                    voucherTrn.Description,
                                    member.memberno,
                                    member.membername,
                                    ledgerGrp.Grp_SlNo,
                                    ledger.Led_SlNo,
                                    ledger.Led_Desc
                                } into g
                                orderby g.Key.Grp_SlNo, g.Key.Led_SlNo
                                select new rptReceiptMemberList
                                {
                                    Voc_Rpt_No = g.Key.Voc_Rpt_No,
                                    Voc_Date = g.Key.Voc_Date,
                                    ReceipentInVoucher = g.Key.ReceipentInVoucher,
                                    Led_Name = g.Key.Led_Name,
                                    Voc_Rpt = g.Sum(x => x.voucherTrn.Voc_Rpt),
                                    Description = g.Key.Description,
                                    MemberNo = g.Key.memberno,
                                    MemberName = g.Key.membername,
                                    Grp_SlNo = g.Key.Grp_SlNo,
                                    Led_SlNo = g.Key.Led_SlNo,
                                    Led_Desc = g.Key.Led_Desc
                                }).ToListAsync();
                    receiptData = resultBank.ToList();
                }

                //var resultCheque = CSISContext.Fin_Voucher_Bank
                //    .Where(vb => vb.Voc_Id == vocId)
                //    .Select(vb =>
                //        "Cheque No " +
                //        vb.Fvb_Cheque_No +
                //        " Dated " +
                //        EF.Functions.FormatDateTime(vb.Fvb_Cheque_Date, "dd/MM/yyyy") +
                //        " Amount " +
                //        vb.Fvb_Amount.ToString()
                //    );
                var query = CSISContext.Fin_Voucher_Bank.Where(vb => vb.Voc_Id == vocId);
                //var formattedResults = new List<string>();
                string formatted = "";
                // Format each record
                foreach (var vb in query)
                {
                    string dateStr = vb.Fvb_Cheque_Date.HasValue ?
                                    vb.Fvb_Cheque_Date.Value.ToString("dd-MM-yyyy") : "";

                    string amountStr = string.Format("{0:############.00}", vb.Fvb_Amount);

                    formatted = "Cheque No " + vb.Fvb_Cheque_No +
                                      " Dated " + dateStr +
                                      " Amount " + amountStr;

                    //formattedResults.Add(formatted);
                }
                if (formatted != null ) chequeDetails = formatted;

                
            }
            catch (Exception)
            {

                throw;
            }
            return (receiptData, chequeDetails);
        }
        public async Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId)
        {
            List<rptPaymentVoucher> paymentData = new List<rptPaymentVoucher>();
            string chequeDetails = "";
            List<rptChequeDetails> chequeList = new List<rptChequeDetails> ();
            try
            {
                decimal cashLedger = CSISContext.Map_General.Where(x => x.ID == 1).Select(x => x.Cash_Led_Id).FirstOrDefault();
                paymentData = await (from voucher in CSISContext.Fin_Voucher
                                 join member in CSISContext.mem_master on voucher.Mem_Id equals member.mem_id into memGroup
                                 from member in memGroup.DefaultIfEmpty() // LEFT JOIN
                                 join voucherTrn in CSISContext.Fin_Voucher_Trn on voucher.Voc_Id equals voucherTrn.Voc_Id
                                 join ledger in CSISContext.Fin_Ledger on voucherTrn.Led_Id equals ledger.Led_Id
                                 join ledgerGrp in CSISContext.Fin_Ledger_Grp on ledger.Grp_Id equals ledgerGrp.Grp_Id
                                 where voucherTrn.FinVocTr_Delete == false
                                 && voucherTrn.Led_Id != cashLedger
                                 && voucher.Voc_Id == vocId
                                 group new { voucher, member, voucherTrn, ledger, ledgerGrp } by new
                                 {
                                     voucher.Voc_Pmt_No,
                                     voucher.Voc_Date,
                                     voucherTrn.Voc_Narr,
                                     member.memberno,
                                     member.membername,
                                     ledger.Led_Name,
                                     voucherTrn.Description,
                                     ledgerGrp.Grp_SlNo,
                                     ledger.Led_SlNo,
                                     ledger.Led_Desc
                                 } into g
                                 where g.Sum(x => x.voucherTrn.Voc_Pmt) > 0
                                 orderby g.Key.Grp_SlNo, g.Key.Led_SlNo
                                 select new rptPaymentVoucher
                                 {
                                     Voc_Pmt_No = g.Key.Voc_Pmt_No,
                                     Voc_Date = g.Key.Voc_Date,
                                     PaymentAmt = g.Sum(x => x.voucherTrn.Voc_Pmt),
                                     Voc_Narr = g.Key.Voc_Narr,
                                     MemberNo = g.Key.memberno,
                                     MemberName = g.Key.membername,
                                     Led_Name = g.Key.Led_Name,
                                     Description = g.Key.Description,
                                     Grp_SlNo = g.Key.Grp_SlNo,
                                     Led_SlNo = g.Key.Led_SlNo,
                                     Led_Desc = g.Key.Led_Desc
                                 }).ToListAsync();

                var result = await (from voucherTrn in CSISContext.Fin_Voucher_Trn
                              join bank in CSISContext.Fin_Voucher_Bank on voucherTrn.Voc_Id equals bank.Voc_Id
                              join mapBank in CSISContext.Map_Banks on voucherTrn.Led_Id equals mapBank.Led_Id
                              where voucherTrn.Voc_Id == vocId
                              && voucherTrn.Voc_Rpt > 0
                              && voucherTrn.FinVocTr_Delete == false
                              && bank.Fvb_Delete == false
                              select new rptChequeDetails
                              {
                                  Fvb_Cheque_No = bank.Fvb_Cheque_No,
                                  Fvb_Cheque_Date = bank.Fvb_Cheque_Date.ToString(),
                                  Fvb_Bank_Name = bank.Fvb_Bank_Name
                              }).ToListAsync();
                foreach (var cheq in result)
                {
                    chequeDetails += "Cheque No " + cheq.Fvb_Cheque_No + " Dated " + cheq.Fvb_Cheque_Date + " " + cheq.Fvb_Bank_Name + " ";
                }
            }
            catch (Exception)
            {

                throw;
            }
            return (paymentData, chequeDetails);
        }
        public async Task<string> GetChequeDetailsForReceipt(decimal vocId)
        {
            string data = "";
            try
            {
                var result = await (from bank in CSISContext.Fin_Voucher_Bank
                              join ledger in CSISContext.Fin_Ledger on bank.Led_Id equals ledger.Led_Id
                              join voucher in CSISContext.Fin_Voucher on bank.Voc_Id equals voucher.Voc_Id
                              where voucher.Voc_Id == 110010121896
                              && (voucher.Voc_Rpt_Mode == "AR" || voucher.Voc_Rpt_Mode == "CH")
                              && voucher.Voc_Delete == false
                              && bank.Fvb_Delete == false  // Changed gvb_Delete to fvb_Delete assuming it was a typo
                              group bank by new
                              {
                                  bank.Fvb_Cheque_No,
                                  bank.Fvb_Cheque_Date,
                                  bank.Fvb_Bank_Name,
                                  ledger.Led_Desc
                              } into g
                              select new
                              {
                                  givenString = $"Cheque No : {g.Key.Fvb_Cheque_No} " +
                                              $"Dt : {g.Key.Fvb_Cheque_Date:dd-MM-yyyy} " +
                                              $"{g.Key.Fvb_Bank_Name} " +
                                              $"Rs. {g.Sum(x => x.Fvb_Amount):N2} " +
                                              "/- adjusted"
                              }).FirstAsync();
                return data;

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion 

        #region Fixed Deposit Reports
        public async Task<(List<rptFDPaymentList> fdPaymentList, string chequeDetails)> GetFDPaymentList(decimal vocId)
        {
            List<rptFDPaymentList> fdPmtList = new();
            string chequeDetails = "";
            List<rptChequeDetails> chequeList = new();
            try
            {
                var result = await (from tdm in CSISContext.TermDeposit_Master
                                    join tdt in CSISContext.TermDeposit_Trn on tdm.TD_Id equals tdt.TD_Id
                                    join fv in CSISContext.Fin_Voucher on tdt.Voc_Id equals fv.Voc_Id
                                    where tdt.Voc_Id == 110010122205 && (tdt.InterestPaidAmount > 0 || tdt.DepositPaidAmount > 0) && tdt.TD_Delete == false
                                    orderby tdm.TD_No
                                    select new rptFDPaymentList
                                    {
                                        Voc_Date = fv.Voc_Date,
                                        Voc_Pmt_No =  fv.Voc_Pmt_No,
                                        TD_No =  tdm.TD_No,
                                        TDH_Name = tdm.TDH_Name,
                                        ValueDate = tdm.ValueDate,
                                        DepositAmount = tdm.DepositAmount,
                                        MaturityDate = tdm.MaturityDate,
                                        MaturityAmount = tdm.MaturityAmount,
                                        RateOfInterest = tdm.RateOfInterest,
                                        InterestAppliedDate = tdt.InterestAppliedDate,
                                        DepositPaidAmount = tdt.DepositReceiptAmount,
                                        InterestPaidAmount = tdt.InterestPaidAmount
                                    }).ToListAsync();
                var result2 = await (from voucherTrn in CSISContext.Fin_Voucher_Trn
                                    join bank in CSISContext.Fin_Voucher_Bank on voucherTrn.Voc_Id equals bank.Voc_Id
                                    join mapBank in CSISContext.Map_Banks on voucherTrn.Led_Id equals mapBank.Led_Id
                                    where voucherTrn.Voc_Id == vocId
                                    && voucherTrn.Voc_Rpt > 0
                                    && voucherTrn.FinVocTr_Delete == false
                                    && bank.Fvb_Delete == false
                                    select new rptChequeDetails
                                    {
                                        Fvb_Cheque_No = bank.Fvb_Cheque_No,
                                        Fvb_Cheque_Date = bank.Fvb_Cheque_Date.ToString(),
                                        Fvb_Bank_Name = bank.Fvb_Bank_Name
                                    }).ToListAsync();
                foreach (var cheq in result2)
                {
                    chequeDetails += "Cheque No " + cheq.Fvb_Cheque_No + " Dated " + cheq.Fvb_Cheque_Date + " " + cheq.Fvb_Bank_Name + " ";
                }
                if (result.Count > 0) fdPmtList = result.ToList();
            }
            catch (Exception)
            {
                fdPmtList = new();
                chequeDetails = "";
            }
            return (fdPmtList,chequeDetails);
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

        public async Task<rptFDBond> GetFDBondData(decimal vocId)
        {
            rptFDBond _bond = new rptFDBond();
            try
            {
                #region sql query
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
                #endregion

                #region linq
                var query = await (from tdm in CSISContext.TermDeposit_Master
                                   join tds in CSISContext.TermDeposit_Schemes on tdm.TDScheme_Id equals tds.TDScheme_Id
                                   join mm in CSISContext.mem_master on tdm.Mem_Id equals mm.mem_id
                                   join fv in CSISContext.Fin_Voucher on tdm.Voc_Id equals fv.Voc_Id into fvGroup
                                   from fv in fvGroup.DefaultIfEmpty()
                                   where tdm.Voc_Id == vocId
                                         && tdm.TD_Delete == false
                                         && fv.Voc_Delete == false
                                         && tdm.Voc_Status == "V"
                                         && fv.Voc_Status == "V"
                                   select new rptFDBond
                                   {
                                       TD_No = tdm.TD_No,
                                       TDH_Name = tdm.TDH_Name,
                                       AccountOpenDate = tdm.AccountOpenDate,
                                       ValueDate = tdm.ValueDate,
                                       DepositAmount = tdm.DepositAmount,
                                       PeriodInMonths = tdm.PeriodInMonths,
                                       PeriodInDays = tdm.PeriodInDays,
                                       RateOfInterest = tdm.RateOfInterest,
                                       MaturityDate = tdm.MaturityDate,
                                       MaturityAmount = tdm.MaturityAmount,
                                       InterestPayableFrequency = tdm.InterestPayableFrequency,
                                       Nominee1Name = tdm.Nominee1Name,
                                       Nominee1Age = tdm.Nominee1Age,
                                       Nominee1Relationship = tdm.Nominee1Relationship,
                                       Nominee2Name = tdm.Nominee2Name,
                                       Nominee2Age = tdm.Nominee2Age,
                                       Nominee2Relationship = tdm.Nominee2Relationship,
                                       TDScheme_Name = tds.TDScheme_Name,
                                       RenewalTD_No = tdm.RenewalTD_No,
                                       Voc_Rpt_No = fv.Voc_Rpt_No,
                                       Voc_No = fv.Voc_No,
                                       MemberNo = mm.memberno,
                                       MemberName = mm.membername,
                                       MobileNo = mm.mobileno,
                                       Address = (mm.preadd1 ?? "") + "," +
                                         (mm.preadd2 != null ? mm.preadd2 + "," : "") +
                                         (mm.preadd3 != null ? mm.preadd3 + "," : "") +
                                         (mm.prepin ?? ""),
                                       ModeOfOperation = tdm.ModeOfOperation

                                   }).FirstOrDefaultAsync();
                #endregion 
                if (query !=null) { _bond = query; }
            }
            catch (Exception)
            {
                _bond = new();
            }
            return _bond;
        }
        #endregion

        #region Jewel Loan ledger
        public async Task<List<rptJewelLoanLedger>> GetJewelLoanLedger(decimal vocId)
        {
            List<rptJewelLoanLedger> loanList = new();
            try
            {
                var query = await  (from lm in CSISContext.Loan_Master
                join mm in CSISContext.mem_master on lm.Mem_Id equals mm.mem_id
                join jld in CSISContext.JL_Details on lm.Loan_Id equals jld.Loan_Id
                join jlo in CSISContext.JL_Ornments on lm.Loan_Id equals jlo.Loan_Id
                join fv in CSISContext.Fin_Voucher on lm.Voc_Id equals fv.Voc_Id
                where lm.Voc_Id == vocId
                      && lm.Loan_Delete == false
                      && jld.JL_Delete == false
                      && jlo.JLO_Delete == false
                      && fv.Voc_Delete == false
                select new rptJewelLoanLedger // Select raw data needed for the final projection
                {
                    Loan_Id = lm.Loan_Id,
                    Mem_Id =  lm.Mem_Id,
                    MemberNo =  mm.memberno,
                    PerNo =  mm.perno,
                    MemberName =  mm.membername,
                    MemberPhoto = mm.memberphoto,
                    Address = (mm.preadd1 ?? "") + "," +
                                             (mm.preadd2 != null ? mm.preadd2 + "," : "") +
                                             (mm.preadd3 != null ? mm.preadd3 + "," : "") +
                                             (mm.prepin ?? ""),
                    MobileNo =  mm.mobileno,
                    PANNo =  mm.panno,
                    AadharNo =  mm.aadharno,
                    SmartCardNo =  mm.smartcardno,
                    Loan_No =  lm.Loan_No,
                    San_Date =  lm.San_Date,
                    San_Amt = lm.San_Amt,
                    Roi = lm.Roi,
                    Pi = lm.Pi,
                    JL_DueDate =  jld.JL_DueDate,
                    GovtRatePerGram = jld.MarketRatePerGram,
                    RatePerGram = jld.RatePerGram,
                    GrossWeight = jld.GrossWeight,
                    Wastage = jld.Wastage,
                    NetWeight = jld.NetWeight,
                    JLO_Name = jlo.JLO_Name,
                    JLO_Nos = jlo.JLO_Nos,
                    JewelsImage = jld.JewelsImagePath
                }).ToListAsync();
                if (query.Count > 0)
                    loanList = query.ToList();
                
            }
            catch (Exception)
            {
                loanList = new();
            }
            return loanList;
        }
        #endregion

        #region Print methods
        //public async Task Print_Member_Receipt(rptReceiptObject rptObject)
        //{
        //    byte[] fileBytes;
        //    var response = await _httpClient.PostAsJsonAsync<rptReceiptObject>($"https://localhost:7073/api/Print/print-receipt", rptObject);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        fileBytes = await response.Content.ReadAsByteArrayAsync();
        //        // 1. Convert byte array to a Base64 string
        //        var base64String = Convert.ToBase64String(fileBytes);

        //        /// 2. Render in iframe commented
        //        // Set the data URL to a property bound to the iframe's src
        //        // pdfDataUrl = $"data:application/pdf;base64,{base64String}";

        //        // If fileBytes is null or empty, the API returned nothing.
        //        if (fileBytes == null || fileBytes.Length == 0)
        //        {
        //            Console.WriteLine("Error: API returned a success status code but with no content.");
        //            return; // Stop here
        //        }
        //        // 3. Call the new JavaScript function to open the PDF in a new tab
        //        await _jsRuntime.InvokeVoidAsync("openPdfInNewTab", base64String);

        //        // 4. Down load pdf commented
        //        // await JSRuntime.SaveAs("Receipt.pdf", fileBytes!);
        //    }
        //    else
        //    {
        //        // Log or show error
        //        Console.WriteLine($"Error: {response.StatusCode}");
        //    }
        //}
        #endregion
    }
}
