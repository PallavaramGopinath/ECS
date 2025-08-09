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

        public async Task<List<DropdownItem>> GetReportNameList(int grpId)
        {
            return await _unitOfWork.ReportsMaster.GetReportNameList(grpId);
        }

        #region Report name and id
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
        #endregion

        #region status
        public async Task<List<string>> GetStatusForMemberTransaction(decimal vocId)
        {
            return  await _unitOfWork.ReportsMaster.GetStatusForMemberTransaction(vocId);
        }
        #endregion 

        #region Receipt and Payment
        public async Task<List<rptReceiptAndPaymentAmount>> GetReceiptAndPaymentAmount(decimal vocId, string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetReceiptAndPaymentAmount(vocId,brCode);
        }
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmount2(decimal vocId, string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetReceiptAndPaymentAmount2(vocId,brCode);
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
        #endregion 


        #region Fixed Deposit
        public async Task<(List<rptFDPaymentList> fdPaymentList, string chequeDetails)> GetFDPaymentList(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetFDPaymentList(vocId);
        }

        public async Task<rptFDBond> GetFDBondPreprinted(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetFDBondPreprinted(vocId);
        }

        public async Task<rptFDBond> GetFDBondData(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetFDBondData(vocId);
        }
        #endregion

        #region Jewel Loan
        public async Task<List<rptJewelLoanLedger>> GetJewelLoanLedger(decimal vocId)
        {
            return await _unitOfWork.ReportsMaster.GetJewelLoanLedger(vocId);
        }
        #endregion

    }
}
