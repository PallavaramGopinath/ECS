using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface IReportsRepository
    {
        Task<List<DropdownItem>> GetReportNameList(int grpId,string brCode);

        #region Report name and id
        Task<Reports_Master> GetReportNameWithSignature(int reportId);
        Task<Reports_Master> GetReportNameWithSignature(string reportName);
        Task<int> GetReportId(string reportName);
        Task<List<Reports_Master>> GetReportsList();
        #endregion

        #region Status List
        Task<List<string>>  GetStatusForMemberTransaction(decimal vocId,string brCode);
        #endregion

        #region Receipt
        Task<List<rptReceiptAndPaymentAmount>> GetReceiptAndPaymentAmount(decimal vocId,string brCode);
        Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount2(decimal vocId,string brCode);
        Task<(List<rptReceiptMemberList> receiptData, DateTime? intCalcDate)> GetReceiptData(decimal vocId,string brCode);
        Task<(List<rptReceiptMemberList> receiptData, string chequeDetails)> GetReceiptGeneralData(decimal vocId, string brCode);
        Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId, string brCode);
        Task<string> GetChequeDetailsForReceipt(decimal vocId,string brCode);
        #endregion 

        #region Fixed Deposit
        Task<(List<rptFDPaymentList> fdPaymentList,string chequeDetails)> GetFDPaymentList(decimal vocId);
        Task<rptFDBond> GetFDBondPreprinted(decimal vocId);
        /// <summary>Members' dividend paid in one voucher (report 146).</summary>
        Task<List<rptDividendPaymentList>> GetDividendPaymentListAsync(decimal vocId, string brCode);
        /// <summary>Member ledger lines of one member suspense bulk voucher (report 91).</summary>
        Task<List<rptMemberSuspenseBulk>> GetMemberSuspenseBulkAsync(decimal vocId, string brCode);
        /// <summary>Fixed deposits opened between two dates, as voucher id / FD number, for the bond reprint picker.</summary>
        Task<List<DropdownItem>> GetFDBondVouchersAsync(DateTime fromDate, DateTime toDate, string brCode);
        Task<rptFDBond> GetFDBondData(decimal vocId);
        #endregion

        #region Jewel Loan ledger
        Task<List<rptJewelLoanLedger>> GetJewelLoanLedger(decimal vocId);

        #endregion 

        #region Print
        //Task Print_Member_Receipt(rptReceiptObject rptObject);
        #endregion
    }
}
