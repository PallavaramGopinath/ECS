using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReportsHandler
    {
        Task<List<DropdownItem>> GetReportNameList(int grpId,string brCode);

        #region Report name and id
        Task<int> GetReportId(string reportName);
        Task<Reports_Master> GetReportNameWithSignature(int reportId);
        Task<Reports_Master> GetReportNameWithSignature(string reportName);
        #endregion

        #region status
        Task<List<string>> GetStatusForMemberTransaction(decimal vocId,string brCode);
        #endregion

        #region Receipt
        Task<List<rptReceiptAndPaymentAmount>> GetReceiptAndPaymentAmount(decimal vocId,string brCode);
        Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount2(decimal vocId, string brCode);
        Task<(List<rptReceiptMemberList> receiptData, DateTime? intCalcDate)> GetReceiptData(decimal vocId, string brCode);
        Task<(List<rptReceiptMemberList> receiptData, string chequeDetails)> GetReceiptGeneralData(decimal vocId, string brCode);
        Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId, string brCode);
        Task<string> GetChequeDetailsForReceipt(decimal vocId,string brCode);
        #endregion 

        #region Fixed Deposit
        Task<(List<rptFDPaymentList> fdPaymentList, string chequeDetails)> GetFDPaymentList(decimal vocId);
        Task<rptFDBond> GetFDBondPreprinted(decimal vocId);
        Task<rptFDBond> GetFDBondData(decimal vocId);
        #endregion

        #region  Jewel Loan
        Task<List<rptJewelLoanLedger>> GetJewelLoanLedger(decimal vocId);
        #endregion

        #region Create report data as byte[]
        //byte[] CreateLocalReport(string dsName, Stream path, IEnumerable result, Dictionary<string, string> parameterDictionary);
        #endregion
    }
}
