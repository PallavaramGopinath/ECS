using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.BusinessLogic.Interface;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsGBController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IGeneralHandler _generalHandler;
        private readonly IReportsGBHandler _reportsGBHandler;
        private readonly IMemPayableMasterHandler _memPayableMasterHandler;
        string societyName = "";

        public ReportsGBController(IReportsHandler reportsHandler, IWebHostEnvironment webHostEnvironment,
               IReportsGBHandler reportsGBHandler, IGeneralHandler generalHandler, IMemPayableMasterHandler memPayableMasterHandler)
        {
            _reportHandler = reportsHandler;
            _webHostEnvironment = webHostEnvironment;
            _generalHandler = generalHandler;
            _reportsGBHandler = reportsGBHandler;
            _memPayableMasterHandler = memPayableMasterHandler;
        }

        [HttpGet]
        [Route("print-dividend-workingsheet/{reportId:int}/{pbleMasterId:decimal}/{brCode}")]
        public async Task<byte[]> Print_Dividend_WorkingSheet(int reportId, decimal pbleMasterId , string brCode)
        {
            Reports_Master report = new Reports_Master();
            string reportHeader = "";
            byte[] pdfAsBytes = Array.Empty<byte>();
            byte[] pdfReport = Array.Empty<byte>();
            Mem_Payable_Master master = new();
            try
            {
                List<rptLoanOutstanding> loanList = new();
                master = await _memPayableMasterHandler.GetMemPayableMasterByPbleMasterId(pbleMasterId, brCode);
                societyName = await _generalHandler.GetSocietyName(brCode);
                report = await _reportHandler.GetReportNameWithSignature(reportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;

                if (reportId == 56)
                    reportHeader = "Dividend working sheet  from " + master.FromDate .ToString("dd-MM-yyyy") + " to " + master.ToDate .ToString("dd-MM-yyyy");
                if (reportId == 57)
                    reportHeader = "Thrift deposit working sheet  from " + master.FromDate.ToString("dd-MM-yyyy") + " to " + master.ToDate.ToString("dd-MM-yyyy");
                if (reportId == 58)
                    reportHeader = "FWD working sheet  from " + master.FromDate.ToString("dd-MM-yyyy") + " to " + master.ToDate.ToString("dd-MM-yyyy");

                var parameters = new Dictionary<string, string>
                {
                    { "paramSocietyName", societyName },
                    { "paramReportHeader",reportHeader}
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    pdfAsBytes = await _reportsGBHandler.GetDividendWorkingSheet("Ds_DividendTDFWDWork", pbleMasterId, brCode, stream, parameters);
                }
                pdfReport = CreatePDFAsBytes(pdfAsBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return pdfReport;
        }

        [HttpGet]
        [Route("print-dividnd-pendinglist/{reportId:int}/{asOnDate}/{brCode}")]
        public async Task<byte[]> Print_Dividend_PendingList(int reportId, string asOnDate, string brCode)
        {
            Reports_Master report = new Reports_Master();
            string reportHeader = "";
            byte[] pdfAsBytes = Array.Empty<byte>();
            byte[] pdfReport = Array.Empty<byte>();
            Mem_Payable_Master master = new();
            try
            {
                DateTime.TryParse(asOnDate, out DateTime asOnDateFormatted);
                List<rptLoanOutstanding> loanList = new();
                societyName = await _generalHandler.GetSocietyName(brCode);
                report = await _reportHandler.GetReportNameWithSignature(reportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;

                if (reportId == 97)
                    reportHeader = "Dividend Pending List Year-Wise as on " + asOnDate;
                if (reportId == 128)
                    reportHeader = "Dividend Pending List Member-Wise as on " + asOnDate;
                
                var parameters = new Dictionary<string, string>
                {
                    { "paramSocietyName", societyName },
                    { "paramReportHeader",reportHeader}
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    pdfAsBytes = await _reportsGBHandler.GetDividendPendingList ("Ds_DividendIntOnTDPending", asOnDateFormatted, brCode, stream, parameters);
                }
                pdfReport = CreatePDFAsBytes(pdfAsBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return pdfReport;
        }

        [HttpGet]
        [Route("print-dividnd-paidlist/{reportId:int}/{fromDate}/{toDate}/{brCode}")]
        public async Task<byte[]> Print_Dividend_PendingList(int reportId, string fromDate,string toDate, string brCode)
        {
            Reports_Master report = new Reports_Master();
            string reportHeader = "";
            byte[] pdfAsBytes = Array.Empty<byte>();
            byte[] pdfReport = Array.Empty<byte>();
            Mem_Payable_Master master = new();
            try
            {
                DateTime.TryParse(fromDate, out DateTime fromDateFormatted);
                DateTime.TryParse(toDate, out DateTime toDateFormatted);
                List<rptLoanOutstanding> loanList = new();
                societyName = await _generalHandler.GetSocietyName(brCode);
                report = await _reportHandler.GetReportNameWithSignature(reportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
              
                reportHeader = "Dividend Paid for the period from " + fromDate + "to " + toDate ;

                var parameters = new Dictionary<string, string>
                {
                    { "paramSocietyName", societyName },
                    { "paramReportHeader",reportHeader}
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    pdfAsBytes = await _reportsGBHandler.GetDividendPaidList("Ds_DividendPaid", fromDateFormatted , toDateFormatted , brCode, stream, parameters);
                }
                pdfReport = CreatePDFAsBytes(pdfAsBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return pdfReport;
        }

        private byte[] CreatePDFAsBytes(byte[] pdfAsBytes)
        {
            try
            {
                return File(pdfAsBytes, "application/pdf").FileContents;
            }
            catch (Exception)
            {
                return File(Array.Empty<byte>(), "application/pdf").FileContents;
            }
        }
    }
}
