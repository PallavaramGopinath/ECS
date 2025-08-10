using Infin8.Coapp.Dto;

using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class TransactionsRepository : Repository<Account_Transactions>, ITransactionsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TransactionsRepository(CSISContext context) : base(context)
        {

        }

        public async Task<List<DropdownItem>> GetTransactionAccounts(string accountStatus, string accountBelongTo)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            var accTypes = new List<string> { "C", accountStatus };
            var belongsToList = new List<string> { "B", accountBelongTo };
            try
            {
                trnList = await CSISContext.Account_Transactions
                .Where(a => !a.Acc_Delete &&
                            accTypes.Contains(a.Acc_Type) &&
                            belongsToList.Contains(a.Acc_BelongsTo))
                .Select(a => new DropdownItem
                {
                    Value = a.Acc_Id.ToString(),
                    Text = a.Acc_Name
                })
                .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DtoAccount_Transactions>> GetAllAccountsTransactions()
        {
            return await (from trn in CSISContext.Account_Transactions
                          where trn.Acc_Delete == false
                          select new DtoAccount_Transactions
                          {
                              Acc_Id = trn.Acc_Id,
                              Acc_Name = trn.Acc_Name,
                              Acc_Status = trn.Acc_Status,
                              Acc_Type = trn.Acc_Type,
                              Component_Name = trn.Component_Name,
                          }).ToListAsync();
        }

        public async Task<List<DropdownItem>> GetAllLedgerItems(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await CSISContext.Fin_Ledger
                .Where(fl => fl.Led_Delete == false &&
                        fl.BrCode == brCode )
                .Select(fl => new DropdownItem
                {
                    Value = fl.Led_Id.ToString(),
                    Text = fl.Led_Name
                })
                .ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }
        public async Task<List<DropdownItem>> GetSuspenseLedgerItems(int suspeneType, string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await (from map in CSISContext.Map_SuspenseAccounts
                                    join led in CSISContext.Fin_Ledger
                                      on map.Led_Id equals led.Led_Id
                                    where map.Sus_Type == suspeneType
                                    && map.BrCode == brCode
                                    select new DropdownItem
                                    {
                                        Value = map.Led_Id.ToString(),
                                        Text = led.Led_Name
                                    }).ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DropdownItem>> GetShareCapitalLedgerItem(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await (from map in CSISContext.Map_General
                                    join led in CSISContext.Fin_Ledger
                                    on map.ShareCapital_Led_Id equals led.Led_Id
                                    where map.BrCode == brCode
                                    select new DropdownItem
                                    {
                                        Value = map.ShareCapital_Led_Id.ToString(),
                                        Text = led.Led_Name
                                    }).ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DropdownItem>> GetBankLedgerItems(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await (from map in CSISContext.Map_Banks
                                    join led in CSISContext.Fin_Ledger
                                    on map.Led_Id equals led.Led_Id
                                    where map.BrCode == brCode
                                    select new DropdownItem
                                    {
                                        Value = map.Led_Id.ToString(),
                                        Text = led.Led_Name
                                    }).ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<List<DropdownItem>> GetLedgersExpectBankLedgerItems(string brCode)
        {
            List<DropdownItem> trnList = new List<DropdownItem>();
            try
            {
                var result = await CSISContext.Fin_Ledger
                .Where(fl => fl.Led_Delete == false &&
                       !CSISContext.Map_General.Select(m => m.Cash_Led_Id)
                            .Union(CSISContext.Map_Banks.Select(m => m.Led_Id))
                            .Contains(fl.Led_Id))
                .Select(fl => new DropdownItem
                {
                    Value = fl.Led_Id.ToString(),
                    Text = fl.Led_Name
                })
                .ToListAsync();
                if (result != null) trnList = result;
            }
            catch (Exception)
            {
                throw;
            }
            return trnList;
        }

        public async Task<string> GetComponentName(int accountId)
        {
            string componentName = "";
            try
            {
                var result = await CSISContext.Account_Transactions.Where(x => x.Acc_Id == accountId).Select(x => x.Component_Name).FirstOrDefaultAsync();
                if (result != null) componentName = result;

            }
            catch (Exception)
            {
                throw;
            }
            return componentName;
        }

        public async Task<string> GetViewComponentName(int accountId)
        {
            string componentName = "";
            try
            {
                var result = await CSISContext.Account_Transactions.Where(x => x.Acc_Id == accountId).Select(x => x.View_Component_Name).FirstOrDefaultAsync();
                if (result != null) componentName = result;

            }
            catch (Exception)
            {
                throw;
            }
            return componentName;
        }

        private DtoTransactionNo GetTransactionNo(string vocMode, decimal yrId)
        {
            int? SlNo = 0;
            string TrnNo = "";
            DtoTransactionNo trn = new DtoTransactionNo();
            switch (vocMode)
            {
                case "CR":
                    SlNo = CSISContext.Fin_Voucher
                        .Where(v => (v.Voc_Rpt_Mode == "CR" || v.Voc_Rpt_Mode == "RC") && v.Yr_Id == yrId)
                        .Max(v => (int?)v.Voc_Rpt_SlNo);
                    break;
                case "AR":
                    SlNo = CSISContext.Fin_Voucher
                        .Where(v => (v.Voc_Rpt_Mode == "AR" || v.Voc_Rpt_Mode == "QR") && v.Yr_Id == yrId)
                        .Max(v => (int?)v.Voc_Rpt_SlNo);
                    break;
                case "VR":
                    SlNo = CSISContext.Fin_Voucher
                       .Where(v => (v.Voc_Pmt_Mode == "VR" || v.Voc_Rpt_Mode == "PT" || v.Voc_Rpt_Mode == "PC" || v.Voc_Rpt_Mode == "PQ" || v.Voc_Rpt_Mode == "PA") && v.Yr_Id == yrId)
                       .Max(v => (int?)v.Voc_Pmt_SlNo);
                    break;
                case "CH":
                    SlNo = CSISContext.Fin_Voucher
                        .Where(v => (v.Voc_Rpt_Mode == "CH" || v.Voc_Rpt_Mode == "BK") && v.Yr_Id == yrId)
                        .Max(v => (int?)v.Voc_Rpt_SlNo);
                    break;
                default:
                    SlNo = 0;
                    break;
            }
            int.TryParse(SlNo.ToString(), out int result);
            result++;
            TrnNo = vocMode + "/" + result;
            trn.TrnSlNo = result;
            trn.TrnMode = vocMode;
            trn.TrnNo = TrnNo;
            return trn;
        }

        public DtoTransactionRptPmtNos GetReceiptAndPaymentNo(double cashReceipt, double cashPayment, double adjReceipt, double adjPayment, bool IsChequeOnly, decimal yrId)
        {
            DtoTransactionRptPmtNos rptPmtNo = new();
            DtoTransactionNo rptNo = new();
            DtoTransactionNo pmtNo = new();
            bool IsModeForChequeRpt = CSISContext.Map_General.Select(x => x.IsNewModeForChequeRpt).First();
            List<DtoTransactionNo> trnNosList = new();
            if (cashReceipt > 0) rptNo = GetTransactionNo("CR", yrId);
            if (adjReceipt > 0)
            {
                if (IsChequeOnly && IsModeForChequeRpt) rptNo = GetTransactionNo("CH", yrId);
                else rptNo = GetTransactionNo("AR", yrId);
            }
            if (cashPayment > 0 || adjPayment > 0) pmtNo = GetTransactionNo("VR", yrId);

            rptPmtNo.Voc_Rpt_No = rptNo.TrnNo;
            rptPmtNo.Voc_Rpt_SlNo = rptNo.TrnSlNo;
            rptPmtNo.Voc_Rpt_Mode = rptNo.TrnMode;

            rptPmtNo.Voc_Pmt_No = pmtNo.TrnNo;
            rptPmtNo.Voc_Pmt_SlNo = pmtNo.TrnSlNo;
            rptPmtNo.Voc_Pmt_Mode = pmtNo.TrnMode;

            return rptPmtNo;
        }

        public async Task<List<DtoTransaction>> GetStagingDataByStagingId(decimal stagingId)
        {
            List<DtoTransaction> transaction = new();
            try
            {
                var query = await (from master in CSISContext.Staging_Master
                                   join details in CSISContext.Staging_Details
                                       on master.Staging_Id equals details.Staging_Id
                                   where master.Staging_Id == stagingId
                                         && details.Staging_Status == "M"
                                   select new DtoTransaction
                                   {
                                       Staging_Id = master.Staging_Id,
                                       Transacted_Member_Id = master.Member_Id,
                                       Transacted_Date = master.Created_Date,
                                       Checked_By = master.Checked_By,
                                       Cash_Or_Adjustment = details.Cash_Or_Adjustment,
                                       Account_Holder_Member_Id = details.Member_Id,
                                       Ledger_Id = details.Ledger_Id,
                                       Receipt_Amount = details.Receipt_Amount,
                                       Payment_Amount = details.Payment_Amount,
                                       Cheque_Date = details.Cheque_Date,
                                       Cheque_No = details.Cheque_No,
                                       Issue_Bank_Name = details.Issue_Bank_Name,
                                       Related_Account_Id = details.Related_Account_Id,
                                       Related_Account_Data = details.Related_Account_Data,
                                       BrCode = master.BrCode
                                   }).ToListAsync();
                if (query != null && query.Count >0)
                {
                    transaction = query.ToList();
                }
            }
            catch (Exception)
            {
                transaction = new();
            }
            return transaction;
        }

        DtoTransactionRptPmtNos ITransactionsRepository.GetReceiptAndPaymentNo(double cashReceipt, double cashPayment, double adjReceipt, double adjPayment, bool IsChequeOnly, decimal yrId)
        {
            return GetReceiptAndPaymentNo(cashReceipt, cashPayment, adjReceipt, adjPayment, IsChequeOnly, yrId);
        }

        public async Task<bool> IsChequeOnly(decimal stagingId)
        {
            bool result = true;
            try
            {
                var count = await (from sd in CSISContext.Staging_Details
                                   where sd.Staging_Id == stagingId &&
                                         sd.Related_Account_Id != 11 &&
                                         sd.Receipt_Amount > 0
                                   select sd).CountAsync();
                if (count > 0) { result = false; }
            }
            catch (Exception)
            {
            }
            return result;
        }

        public async Task<string> GetTransactionStatusByAccId(int accId)
        {
            string status = "";
            status = await CSISContext.Account_Transactions.Where(x => x.Acc_Id == accId).Select(x => x.Acc_Status).FirstAsync();
            return status;
        }

        
    }
}
