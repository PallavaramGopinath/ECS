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
        Task<List<DropdownItem>> GetReportNameList(int grpId);

        #region Report name and id
        Task<Reports_Master> GetReportNameWithSignature(int reportId);
        Task<Reports_Master> GetReportNameWithSignature(string reportName);
        Task<int> GetReportId(string reportName);
        #endregion

        #region Status List
        Task<List<string>> GetStatusForMemberTransaction(decimal vocId);
        #endregion

        #region Receipt
        Task<List<rptReceiptAndPaymentAmount>> GetReceiptAndPaymentAmount(decimal vocId,string brCode);
        Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount2(decimal vocId,string brCode);
        Task<(List<rptReceiptMemberList> receiptData, DateTime? intCalcDate)> GetReceiptData(decimal vocId,string brCode);
        Task<(List<rptReceiptMemberList> receiptData, string chequeDetails)> GetReceiptGeneralData(decimal vocId, string brCode);
        Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId);
        Task<string> GetChequeDetailsForReceipt(decimal vocId);
        #endregion 

        #region Fixed Deposit
        Task<(List<rptFDPaymentList> fdPaymentList,string chequeDetails)> GetFDPaymentList(decimal vocId);
        Task<rptFDBond> GetFDBondPreprinted(decimal vocId);
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
