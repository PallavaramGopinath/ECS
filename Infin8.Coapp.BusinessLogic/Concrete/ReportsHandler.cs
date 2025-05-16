using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsHandler : IReportsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public ReportsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<int> GetReportId(string reportName)
        {
            return await _unitOfWork.ReportsMaster.GetReportId(reportName);
        }
        public async Task<Reports_Master> GetReportNameWithSignature(int reportId)
        {
            return await _unitOfWork.ReportsMaster.GetReportNameWithSignature(reportId);
        }
        public async Task<Reports_Master> GetReportNameWithSignature(string reportName)
        {
            return await _unitOfWork.ReportsMaster.GetReportNameWithSignature(reportName);
        }
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetReceiptAndPaymentAmount(vocId);
        }
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount(decimal vocId, string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetReceiptAndPaymentAmount(vocId,brCode);
        }
        public async Task<(List<rptReceiptMemberList> receiptData, DateTime? intCalcDate)> GetReceiptData(decimal vocId, string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetReceiptData(vocId,brCode);
        }
        public async Task<(List<rptReceiptMemberList> receiptData, string chequeDetails)> GetReceiptGeneralData(decimal vocId, string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetReceiptGeneralData(vocId,brCode);
        }
        public  async Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetPaymentData(vocId);
        }
        public async Task<string> GetChequeDetailsForReceipt(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetChequeDetailsForReceipt(vocId);
        }

        
    }
}
