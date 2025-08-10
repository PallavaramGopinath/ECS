using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using System.Data;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsAccountController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGeneralHandler _generalHandler;
        private readonly IReportsAccountHandler _reportsAccountHandler;
        string societyName = "";
        
        public ReportsAccountController(IWebHostEnvironment webHostEnvironment,
            IReportsAccountHandler reportsAccountHandler , 
            IGeneralHandler generalHandler,
            IReportsHandler reportsHandler)    
        {
            _reportHandler = reportsHandler;
            _webHostEnvironment = webHostEnvironment;
            _reportsAccountHandler = reportsAccountHandler;
            _generalHandler = generalHandler;
        }

        [HttpPost]
        [Route("print-chitta")]
        public async Task<FileContentResult> Print_Chtta(rptReportAccountObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptDayBook2> ChittaList = new();
                List<rptDayBook2> _chittaRptList = new();
                List<rptDayBook2> _chittaPmtList = new();

                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var chittaData = await _reportsAccountHandler.GetChittaBook(rptObject.FromDateString!, rptObject.ToDateString!, rptObject.YrId, rptObject.BrCode!);
                if (chittaData != null && chittaData.Any())
                {
                    ChittaList = chittaData.ToList();
                }
                _chittaRptList = ChittaList.Where(x => x.voc_rpt > 0).OrderBy(x => x.Voc_Date).ThenByDescending(x => x.voc_rpt_No!.Substring(1, 2)).ThenBy(x => x.voc_Rpt_slNo).ToList();
                _chittaPmtList = ChittaList.Where(x => x.voc_pmt > 0).OrderBy(x => x.Voc_Date).ThenBy(x => x.voc_pmt_No).ToList();
                _chittaRptList.AddRange(_chittaPmtList);

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                };
                //localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_ChittaBook", _chittaRptList));
                localReport.SetParameters(parameters);
                //localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-daybook")]
        public async Task<FileContentResult> Print_DayBook(rptReportAccountObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptDayBook2> dayBookList = new();


                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }

                var daybookData = await _reportsAccountHandler.GetDayBook(rptObject.FromDateString!, rptObject.ToDateString!, rptObject.YrId, rptObject.BrCode!);
                if (daybookData != null && daybookData.Any())
                {
                    dayBookList = daybookData.ToList();
                }

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_DayBook", dayBookList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-generalLedger")]
        public async Task<FileContentResult> Print_GeneralLedger(rptReportAccountObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptFinGeneralLedger> glList = new();


                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                reportHeader = "General Ledger";
                var glLedgerData = await _reportsAccountHandler.GetGeneralLedger(rptObject.FromDate, rptObject.ToDate,rptObject.LedgerList!, rptObject.YrId, rptObject.BrCode!);
                if (glLedgerData != null && glLedgerData.Any())
                {
                    glList = glLedgerData.ToList();
                }

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader ) ,
                    new ReportParameter("ParamFirstSignature", report.FirstSignature ) ,
                    new ReportParameter("ParamSecondSignature", report.SecondSignature ),
                    new ReportParameter("ParamThirdSignature", report.ThirdSignature )
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_GeneralLedger", glList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
                //pdfAsBytes = localReport.Render("Excel");
                //pdfAsBytes = localReport.Render("Word");
            }
            catch (Exception)
            {

                throw;
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-receiptAndCharges")]
        public async Task<FileContentResult> Print_ReceiptAndCharges(rptReportAccountObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptFinReceiptAndCharges> rncList = new();


                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                reportHeader = "Receipt And Charges from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy");
                var rncData = await _reportsAccountHandler.GetReceiptAndCharges(rptObject.FromDate, rptObject.ToDate,rptObject.YrId, rptObject.BrCode!);
                if (rncData != null && rncData.Any())
                {
                    rncList = rncData.ToList();
                }

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader ) ,
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_ReceiptAndCharges", rncList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
                //pdfAsBytes = localReport.Render("Excel");
                //pdfAsBytes = localReport.Render("Word");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CreatePDFAsBytes(pdfAsBytes);
            //return CreateExcelAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-balanceSheet")]
        public async Task<FileContentResult> Print_BalanceSheet(rptReportAccountObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptFinBalanceSheet> balanceSheetList = new();


                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                var blsData = await _reportsAccountHandler.GetBalanceSheet(rptObject.FromDate, rptObject.ToDate, rptObject.YrId, rptObject.BrCode!);
                if (blsData != null && blsData.Any())
                {
                    balanceSheetList = blsData.ToList();
                }

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramFromDate", rptObject.FromDateString ) ,
                    new ReportParameter("paramToDate", rptObject.ToDateString ) 
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_BalanceSheet", balanceSheetList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
                //pdfAsBytes = localReport.Render("Excel");
                //pdfAsBytes = localReport.Render("Word");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CreatePDFAsBytes(pdfAsBytes);
            //return CreateExcelAsBytes(pdfAsBytes);
        }

        [HttpPost]
        [Route("print-lossAndProfit")]
        public async Task<FileContentResult> Print_LossAndProfit(rptReportAccountObject rptObject)
        {
            Reports_Master report = new Reports_Master();
            byte[] pdfAsBytes = Array.Empty<byte>();
            string reportHeader = "";
            try
            {
                societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                List<rptFinLossAndProfit> lossAndProfitList = new();


                report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                LocalReport localReport = new LocalReport
                {
                    EnableExternalImages = true,
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    localReport.LoadReportDefinition(stream);
                }
                reportHeader = "Loss and Profit statement from " + rptObject.FromDate.ToString("dd-MM-yyyy")  + " to " + rptObject.ToDate.ToString("dd-MM-yyyy") ;
                var lnpData = await _reportsAccountHandler.GetLossAndProfit(rptObject.FromDate, rptObject.ToDate, rptObject.YrId, rptObject.BrCode!);
                if (lnpData != null && lnpData.Any())
                {
                    lossAndProfitList = lnpData.ToList();
                }

                var parameters = new[]
                {
                    new ReportParameter("paramSocietyName", societyName ) ,
                    new ReportParameter("paramReportHeader", reportHeader ) 
                };
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("Ds_LossAndProfit", lossAndProfitList));
                localReport.SetParameters(parameters);
                localReport.Refresh();
                pdfAsBytes = localReport.Render("PDF");
                //pdfAsBytes = localReport.Render("Excel");
                //pdfAsBytes = localReport.Render("Word");
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            return CreatePDFAsBytes(pdfAsBytes);
            //return CreateExcelAsBytes(pdfAsBytes);
        }

        #region Create PDF AS Bytes
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

        private FileContentResult CreateExcelAsBytes(byte[] pdfAsBytes)
        {
            try
            {
                //var PDFDoc = File(pdfAsBytes, "application/pdf", "report.pdf");
                return File(pdfAsBytes, "application/Excel");
            }
            catch (Exception)
            {
                return File(Array.Empty<byte>(), "application/Excel");
            }
        }
        #endregion
    }
}
