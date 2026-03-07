using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
//using Microsoft.Reporting.WebForms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class ReportsHandler : IReportsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ICreateReportsHandler _createReportsHandler;
        public ReportsHandler(IUnitOfWork unitOfWork, ICreateReportsHandler createReportsHandler )
        {
            _unitOfWork = unitOfWork;
            _createReportsHandler = createReportsHandler;
        }

        public async Task<List<DropdownItem>> GetReportNameList(int grpId,string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetReportNameList(grpId, brCode);
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
        public async Task<List<Reports_Master>> GetReportsList()
        {
            return await _unitOfWork.ReportsMaster.GetReportsList();
        }
        #endregion

        #region status
        public async Task<List<string>> GetStatusForMemberTransaction(decimal vocId,string brCode)
        {
            return  await _unitOfWork.ReportsMaster.GetStatusForMemberTransaction(vocId,brCode);
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
        public  async Task<(List<rptPaymentVoucher> paymentData, string chequeDetails)> GetPaymentData(decimal vocId,string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetPaymentData(vocId,brCode);
        }
        public async Task<string> GetChequeDetailsForReceipt(decimal vocId, string brCode)
        {
            return await _unitOfWork.ReportsMaster.GetChequeDetailsForReceipt(vocId,brCode );
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


        //#region Create report data as byte[]
        //public byte[] CreateLocalReport(string dsName, Stream path, IEnumerable result, Dictionary<string, string> parameterDictionary)
        //{
        //    byte[] pdfAsBytes = Array.Empty<byte>();

        //    pdfAsBytes = CreateReport(dsName, path, result, parameterDictionary);

        //    return pdfAsBytes;
        //}

        //public byte[] CreateReport(string dsName, Stream stream, IEnumerable result, Dictionary<string, string> parameterDictionary)
        //{
        //    byte[] pdfAsBytes = Array.Empty<byte>();

        //    if (result.Equals(0))
        //    {
        //        return pdfAsBytes;
        //    }

        //    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        //    Encoding.GetEncoding("windows-1252");

        //    LocalReport localReport = new LocalReport
        //    {
        //        EnableExternalImages = true
        //    };
        //    //using (FileStream stream = System.IO.File.OpenRead(path))
        //    //{
        //    localReport.LoadReportDefinition(stream);
        //    //}

        //    localReport.DataSources.Add(new ReportDataSource(dsName) { Value = result });

        //    List<ReportParameter> rParameters = new List<ReportParameter>();
        //    foreach (var param in parameterDictionary)
        //    {
        //        ReportParameter rParam = new ReportParameter(param.Key, param.Value);
        //        rParameters.Add(rParam);
        //    }

        //    localReport.SetParameters(rParameters);

        //    try
        //    {
        //        pdfAsBytes = localReport.Render("PDF");
        //        return pdfAsBytes;
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        return pdfAsBytes;
        //    }
        //}
        //#endregion

    }
}
