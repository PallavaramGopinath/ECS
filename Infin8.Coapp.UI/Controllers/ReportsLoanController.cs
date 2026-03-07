using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsLoanController : ControllerBase
    {
        private readonly IReportsHandler _reportHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReportsLoanHandler _reportsLoanHandler;
        private readonly IGeneralHandler _generalHandler;
        string societyName = "";
        public ReportsLoanController(IWebHostEnvironment webHostEnvironment,
            IReportsLoanHandler reportsLoanHandler, IReportsHandler reportsHandler,
            IGeneralHandler generalHandler)
        {
            _reportsLoanHandler = reportsLoanHandler;
            _webHostEnvironment = webHostEnvironment;
            _generalHandler = generalHandler;
            _reportHandler = reportsHandler;
            //this.societyName = societyName;
        }

        [HttpGet]
        [Route("print-FD-LoanOustanding/{reportId:int}/{asOnDate}/{brCode}")]
        public async Task<byte[]> Print_FD_LoanOustanding(int reportId, string asOnDate, string brCode)
        {
            Reports_Master report = new Reports_Master();
            string reportHeader = "";
            byte[] pdfAsBytes = Array.Empty<byte>();
            byte[] pdfReport = Array.Empty<byte>();
            try
            {
                DateTime.TryParse(asOnDate, out DateTime asOnDateFormatted);
                List<rptLoanOutstanding> loanList = new();
                //societyName = await _generalHandler.GetSocietyName(rptObject.BrCode!);
                societyName = await _generalHandler.GetSocietyName(brCode);
                report = await _reportHandler.GetReportNameWithSignature(reportId);
                var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
                reportHeader = "Loan outstanding as on " + asOnDate;
                var parameters = new Dictionary<string, string>
                {
                    { "paramSocietyName", societyName },
                    { "paramReportHeader",reportHeader}
                };
                
                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    pdfAsBytes = await _reportsLoanHandler.GetLoanOutstanding("Ds_LoanOutstanding", asOnDateFormatted, 3,brCode,stream , parameters);
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
