using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infin8.Coapp.Repository
{
    public class ReportsRepository : Repository<Reports_Master>, IReportsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public ReportsRepository(CSISContext context) : base(context)
        {
        }
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
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount(decimal vocId)
        {
            rptReceiptAndPaymentAmount? rptAmtPmt = new rptReceiptAndPaymentAmount();
            try
            {
                var result  = await (from vocTrn in CSISContext.Fin_Voucher_Trn
                              join voc in CSISContext.Fin_Voucher on vocTrn.Voc_Id equals voc.Voc_Id
                              where vocTrn.Voc_Id == vocId &&
                                    !CSISContext.Map_General.Any(m => m.Cash_Led_Id == vocTrn.Led_Id) &&
                                    vocTrn.FinVocTr_Delete == false
                              group new { voc, vocTrn } by new { voc.Voc_Type, vocTrn.Voc_Trn_Type } into g
                              select new rptReceiptAndPaymentAmount
                              {
                                  Voc_Type = g.Key.Voc_Type,
                                  Voc_Trn_Type = g.Key.Voc_Trn_Type,
                                  Voc_Rpt = g.Sum(x => x.vocTrn.Voc_Rpt),
                                  Voc_Pmt = g.Sum(x => x.vocTrn.Voc_Pmt)
                              }).FirstOrDefaultAsync();
                if (result == null) rptAmtPmt = result;
                
            }
            catch (Exception)
            {

                throw;
            }
            return rptAmtPmt!;
        }
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount(decimal vocId, string brCode)
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

        
    }
}
