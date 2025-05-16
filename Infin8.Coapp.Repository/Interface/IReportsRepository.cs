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
        Task<Reports_Master> GetReportNameWithSignature(int reportId);
        Task<Reports_Master> GetReportNameWithSignature(string reportName);
        Task<int> GetReportId(string reportName);
        Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount(decimal vocId);
        Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount(decimal vocId,string brCode);
        Task<(List<rptReceiptMemberList> receiptData, DateTime? intCalcDate)> GetReceiptData(decimal vocId,string brCode);

        Task<(List<rptReceiptMemberList> receiptData, string chequeDetails)> GetReceiptGeneralData(decimal vocId, string brCode);
        Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId);
        Task<string> GetChequeDetailsForReceipt(decimal vocId);

    }
}
