using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Reporting.NETCore;
using System.Diagnostics;
using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System.Data;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IUtilityHandler _utilityHandler;
        private readonly IGeneralHandler _generalHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        string societyName = "";
        public PrintController(IReportsHandler reportsHandler,IUtilityHandler utilityHandler, 
            IGeneralHandler generalHandler, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _reportHandler = reportsHandler;
            _utilityHandler = utilityHandler;
            _generalHandler = generalHandler;
        }

        
        //public async Task<FileContentResult> PrintMemberReceipt(decimal vocId,string brCode)
        //{
        //    rptReceiptAndPaymentAmount rptAndPmtAmt = new rptReceiptAndPaymentAmount();
        //    Reports_Master report = new Reports_Master();
        //    byte[] pdfAsBytes = Array.Empty<byte>();
        //    try
        //    {
        //        societyName = await _generalHandler.GetSocietyName(brCode);
        //        //var path = $"{this._webHostEnvironment.ContentRootPath}\\wwwroot\\Reports\\SKSMChecklist.rdlc";
        //        var path = $"{this._webHostEnvironment.ContentRootPath}\\wwwroot\\Reports\\"; // + report.ReportFileName;
                
        //        rptAndPmtAmt = await _reportHandler.GetReceiptAndPaymentAmount(vocId);
        //        LocalReport localReport = new LocalReport
        //        {
        //            EnableExternalImages = true,
        //        };
                
        //        using (FileStream stream = System.IO.File.OpenRead(path))
        //        {
        //            localReport.LoadReportDefinition(stream);
        //        }
        //        /// print member receipt
        //        if(rptAndPmtAmt.Voc_Rpt > 0)
        //        {
        //            string receiptHeader = rptAndPmtAmt.Voc_Trn_Type == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
        //            List<rptReceiptMemberList> rptList = new List<rptReceiptMemberList>();
        //            if (rptAndPmtAmt.Voc_Type == 5 || rptAndPmtAmt.Voc_Type == 11)
        //            {
        //                return await Print_MemberReceipt(rptAndPmtAmt, vocId, brCode);
        //            }
        //            else if(rptAndPmtAmt .Voc_Type == 14)
        //            {
                        
        //            }
                   
        //        }
        //        /// print voucher
        //        if (rptAndPmtAmt.Voc_Pmt > 0)
        //        {

        //        }
        //        //pdfAsBytes = localReport.Render("PDF");
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return CreatePDFAsBytes(pdfAsBytes);
        //}

        [HttpGet]
        [Route("GetReceiptAndPaymentData/{vocId:decimal}/{brCode}")]
        public async Task<rptReceiptAndPaymentAmount> GetReceiptAndPaymentAmountByVocId(decimal vocId, string brCode)
        {
            rptReceiptAndPaymentAmount rptAndPmtAmt = new rptReceiptAndPaymentAmount();
            try
            {
                rptAndPmtAmt = await _reportHandler.GetReceiptAndPaymentAmount(vocId,brCode);
            }
            catch (Exception)
            {
                //_logger.LogError(ex, "Error in GetReceiptAndPaymentAmountByVocId");
                //return null; // Or handle more gracefully
            }
            return rptAndPmtAmt;
        }

        [HttpPost]
        //[Route("print-receipt/{vocTrnType:int}/{vocId:decimal}/{brCode}")]
        [Route("print-receipt")]
        public async Task<FileContentResult> Print_MemberReceipt( [FromBody] rptReceiptObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string rsInWords = "";
            //double receiptAmt = 0;
            try
            {
                societyName = await  _generalHandler.GetSocietyName(rptObject.brCode!);
                string receiptHeader = rptObject.vocTrnType == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
                List<rptReceiptMemberList> rptList = new List<rptReceiptMemberList>();
                report = await _reportHandler.GetReportNameWithSignature("Receipt");
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                
                var receiptData = await _reportHandler.GetReceiptData(rptObject.vocId, rptObject.brCode!);
                rptList = receiptData.receiptData;
                //receiptAmt = rptList.Sum(x => x.Voc_Rpt);
                rsInWords  = _utilityHandler.RupeesInWords(rptObject.receiptAmt);
                DateTime? intCalcDate = null;
                intCalcDate = receiptData.intCalcDate;
                DataTable dth = new DataTable();
                dth.Columns.Add("RsInWords");
                dth.Columns.Add("SocietyName");
                dth.Columns.Add("ReportHeader");
                dth.Columns.Add("IntCalcDetails");
                dth.Columns.Add("AdjString");
                DataRow dr = dth.NewRow();
                dr["RsInWords"] = rsInWords;
                dr["SocietyName"] = societyName;
                dr["ReportHeader"] = receiptHeader;
                if (intCalcDate != null)
                    dr["IntCalcDetails"] = "Interest Calculated upto " + intCalcDate.Value.ToString("dd-MM-yyyy");
                else
                    dr["IntCalcDetails"] = "";
                string _adjString = await _reportHandler.GetChequeDetailsForReceipt(rptObject.vocId);

                if (_adjString != null)
                {
                    dr["AdjString"] = _adjString;
                }
                else
                {
                    dr["AdjString"] = "";
                    /// if current inters date is null then get loan id and obtain last interest calculated date here.
                }
                dth.Rows.Add(dr);
                var parameters = new[]
                {
                        new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                        new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                        new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                    };

                localReport.DataSources.Add(new ReportDataSource("Ds_Receipt", rptList));
                localReport.DataSources.Add(new ReportDataSource("Ds_Header", dth));
                localReport.SetParameters(parameters);
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return  CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-receipt-general")]
        public async Task<FileContentResult> Print_MemberReceiptGeneral([FromBody] rptReceiptObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string rsInWords = "";
            string chequeDetails = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.brCode!);
                string receiptHeader = rptObject.vocTrnType == 1 ? "CASH RECEIPT" : "ADJUSTMENT RECEIPT";
                List<rptReceiptMemberList> rptList = new List<rptReceiptMemberList>();
                report = await _reportHandler.GetReportNameWithSignature("Receipt General");
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var receiptData = await _reportHandler.GetReceiptGeneralData(rptObject.vocId, rptObject.brCode!);
                rptList = receiptData.receiptData;
                //receiptAmt = rptList.Sum(x => x.Voc_Rpt);
                rsInWords = _utilityHandler.RupeesInWords(rptObject.receiptAmt);
;
                chequeDetails = receiptData.chequeDetails;
               
                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature ),
                    new ReportParameter("paramReportHeader", receiptHeader ),
                    new ReportParameter("paramRsInWords", rsInWords ),
                    new ReportParameter("paramChequeDetails", chequeDetails)
                    };

                localReport.DataSources.Add(new ReportDataSource("Ds_ReceiptGeneral", rptList));
                localReport.SetParameters(parameters);
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        private FileContentResult CreatePDFAsBytes(byte[] pdfAsBytes)
        {
            try
            {
                //var PDFDoc = File(pdfAsBytes, "application/pdf", "report.pdf");
                return File(pdfAsBytes, "application/pdf");
            }
            catch (Exception)
            {
                return File(Array.Empty<byte>(), "application/pdf");
            }
        }
    }
}
