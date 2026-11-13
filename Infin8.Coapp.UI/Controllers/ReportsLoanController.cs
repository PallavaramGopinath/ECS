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
                var path = Path.Combine(this._webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
                reportHeader = "Fixed Deposit Loan outstanding as on " + asOnDate;
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

        [HttpGet]
        [Route("print-LoanOustanding-Others/{reportId:int}/{loanType:int}/{asOnDate}/{brCode}")]
        public async Task<byte[]> Print_LoanOustanding_Others(int reportId,int loanType, string asOnDate, string brCode)
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
                var path = Path.Combine(this._webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
                switch(loanType)
                {
                    case 3:
                        if(reportId == 68)
                            reportHeader = "Fixed Deposit Loan outstanding Loan No-wise as on " + asOnDate;
                        else if(reportId == 69)
                            reportHeader = "Fixed Deposit Loan outstanding Member-wise as on " + asOnDate;
                        break;
                    case 4:
                        reportHeader = "Recurring Deposit Loan outstanding as on " + asOnDate;
                        break;
                    case 5:
                        reportHeader = "Staff Loan outstanding Loan No-wise as on " + asOnDate;
                        break;
                }
                
                var parameters = new Dictionary<string, string>
                {
                    { "paramSocietyName", societyName },
                    { "paramReportHeader",reportHeader}
                };

                using (FileStream stream = System.IO.File.OpenRead(path))
                {
                    pdfAsBytes = await _reportsLoanHandler.GetLoanOutstanding("Ds_LoanOutstanding", asOnDateFormatted, loanType, brCode, stream, parameters);
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


        /// <summary>Loan disbursement statement (report 141, TSIS LoanDisbursementHSIS) for one loan type between two dates.</summary>
        [HttpGet]
        [Route("print-LoanDisbursement/{reportId:int}/{loanType:int}/{fromDate}/{toDate}/{brCode}")]
        public async Task<byte[]> Print_LoanDisbursement(int reportId, int loanType, string fromDate, string toDate, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime from);
            DateTime.TryParse(toDate, out DateTime to);
            var report = await _reportHandler.GetReportNameWithSignature(reportId);
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", await _generalHandler.GetSocietyName(brCode) },
                { "paramReportHeader", "Loan Disbursement Statement From " + from.ToString("dd-MM-yyyy") + " To " + to.ToString("dd-MM-yyyy") }
            };
            byte[] pdfAsBytes;
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsLoanHandler.GetLoanDisbursementToPrint("Ds_LoanDisbursementHSIS", from, to, loanType, brCode, stream, parameters);
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        /// <summary>Loan outstanding member-wise with age-wise principal overdue (report 19).</summary>
        [HttpGet]
        [Route("print-LoanOutstanding-Agewise/{reportId:int}/{loanType:int}/{asOnDate}/{brCode}")]
        public async Task<byte[]> Print_LoanOutstanding_Agewise(int reportId, int loanType, string asOnDate, string brCode)
        {
            DateTime.TryParse(asOnDate, out DateTime asOn);
            var report = await _reportHandler.GetReportNameWithSignature(reportId);
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", await _generalHandler.GetSocietyName(brCode) },
                { "paramReportHeader", "Loan outstanding as on " + asOn.ToString("dd-MM-yyyy") }
            };
            byte[] pdfAsBytes;
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsLoanHandler.GetLoanOutstandingWithAgewise("Ds_LoanOutstanding", asOn, loanType, brCode, stream, parameters);
            }
            return CreatePDFAsBytes(pdfAsBytes);
        }

        /// <summary>The member's loans of one type, for the loan ledger picker.</summary>
        [HttpGet]
        [Route("LedgerLoans/{memId:decimal}/{loanType:int}/{brCode}")]
        public async Task<List<DtoLoanNo>> LedgerLoans(decimal memId, int loanType, string brCode) =>
            await _reportsLoanHandler.GetMemberLoansAsync(memId, loanType, brCode);

        /// <summary>Loan ledger (report 16) for the chosen loans between two dates.</summary>
        [HttpPost]
        [Route("print-LoanLedger")]
        public async Task<IActionResult> Print_LoanLedger(rptReportLoanObject rptObject)
        {
            var loanIds = rptObject.LoanNoList ?? new List<decimal>();
            if (loanIds.Count == 0) return BadRequest("Select at least one loan.");
            var report = await _reportHandler.GetReportNameWithSignature(rptObject.ReportId);
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", await _generalHandler.GetSocietyName(rptObject.BrCode ?? "") },
                { "paramReportHeader", "Loan Ledger from " + rptObject.FromDate.ToString("dd-MM-yyyy") + " to " + rptObject.ToDate.ToString("dd-MM-yyyy") }
            };
            byte[] pdfAsBytes;
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsLoanHandler.GetLoanLedgerToPrint("Ds_LoanLedger", loanIds, rptObject.FromDate, rptObject.ToDate, rptObject.BrCode ?? "", stream, parameters);
            }
            return File(pdfAsBytes, "application/pdf");
        }
    }
}
