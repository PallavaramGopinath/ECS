using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsMemberController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IReportsMemberHandler _reportsMemberHandler;
        private readonly IReportsHandler _reportsHandler;
        private readonly IGeneralHandler _generalHandler;
        public ReportsMemberController(IWebHostEnvironment webHostEnvironment, IReportsMemberHandler reportsMemberHandler,
            IReportsHandler reportsHandler, IGeneralHandler generalHandler)
        {
            _webHostEnvironment = webHostEnvironment;
            _reportsMemberHandler = reportsMemberHandler;
            _reportsHandler = reportsHandler;
            _generalHandler = generalHandler;
        }

        [HttpGet]
        [Route("GetShareCapitalMemberWise/{fromDateStr}/{toDateStr}/{trnType:int}/{reportId:int}/{brCode}")]
        public async Task<byte[]> GetMemberShareCapitalMemberWise(string fromDateStr,string toDateStr, int trnType,int reportId, string brCode)
        {
            string reportHeader = "";
            switch(trnType)
            {
                case 1: /// due to
                    reportHeader = "Member Suspense Debtor Member-wise from " + fromDateStr + " to " + toDateStr;
                    break;
                case 2: /// due by 
                    reportHeader = "Member Suspense Creditor Member-wise from " + fromDateStr + " to " + toDateStr;
                    break;
                case 3: /// share capital
                    reportHeader = "Member Share capital Member-wise from " + fromDateStr + " to " + toDateStr;
                    break;
                case 5: /// staff due to
                    reportHeader = "Member Staff Suspense Debtor Staff-wise from " + fromDateStr + " to " + toDateStr;
                    break;
                case 6: /// staff due by
                    reportHeader = "Member Staff Suspense Creditor Staff-wise as on " + toDateStr;
                    break;
            }
            DateTime.TryParse(fromDateStr, out DateTime fromDate);
            DateTime.TryParse(toDateStr, out DateTime toDate);
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName },
                { "paramReportHeader",reportHeader  }
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsMemberHandler.GetShareCapitalToPrintAsBytes ("Ds_MemberTrn",fromDate, toDate, trnType, brCode, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }

        [HttpGet]
        [Route("GetMemberTrnOS/{toDateStr}/{trnType:int}/{reportId:int}/{brCode}")]
        public async Task<byte[]> GetMemberTrnOS(string toDateStr, int trnType, int reportId, string brCode)
        {
            string reportHeader = "";
            switch (trnType)
            {
                case 3: /// Shae capital os member-wise
                    reportHeader = "Member Share Capital Member-wise as on " + toDateStr;
                    break;
                case 2: /// member suspense creditor ledger wise
                    reportHeader = "Member Suspense Creditor Ledger-wise and Member-wise as on " + toDateStr;
                    break;
                case 1: /// member suspense debtor ledger wise
                    reportHeader = "Member Suspense Debtor Ledger-wise and Member-wiseas on " + toDateStr;
                    break;
                
            }
            DateTime.TryParse(toDateStr, out DateTime toDate);
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName },
                { "paramReportHeader",reportHeader  }
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsMemberHandler.GetMemberTrnOSToPrintAsBytes("Ds_MemberTrn",  toDate, trnType, brCode, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }

        [HttpGet]
        [Route("GetMemberList")]
        public async Task<byte[]> GetMemberList([FromQuery] DateTime asOnDate,[FromQuery] List<int> memberTypeList, [FromQuery] int memberStatus, [FromQuery] int reportId, [FromQuery] string brCode)
        {
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName },
                { "paramReportHeader","Member List as on " + asOnDate.ToString("dd-MM-yyyy")}
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsMemberHandler.GetMemberListToPrintAsBytes("Ds_MemberList",asOnDate , memberTypeList,memberStatus, brCode, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }

        [HttpGet]
        [Route("GetVoutersList")]
        public async Task<byte[]> GetMemberVoutersList([FromQuery] DateTime asOnDate, [FromQuery] List<int> memberTypeList, [FromQuery] int memberStatus,[FromQuery] int minimumSCBalance, [FromQuery] int reportId, [FromQuery] string brCode)
        {
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName },
                { "paramReportHeader","Member List as on " + asOnDate.ToString("dd-MM-yyyy")}
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsMemberHandler.GetMemberVoutersListToPrintAsBytes("Ds_MemberList", asOnDate, memberTypeList, memberStatus, minimumSCBalance , brCode, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }

        [HttpGet]
        [Route("GetMemberRegister/{memId:decimal}/{reportId:int}/{brCode}")]
        public async Task<byte[]> GetMemberRegister(decimal memId, int reportId,string brCode)
        {
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            //var kyc = await _reportsMemberHandler.GetMemberKYC(memId);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName }
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsMemberHandler.GetMemberRegisterToPrintAsBytes("Ds_MemberRegister", memId, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }

        [HttpGet]
        [Route("GetNewMembers/{fromDateStr}/{toDateStr}/{reportId:int}/{brCode}")]
        public async Task<byte[]> GetNewMembers(string fromDateStr, string toDateStr, int trnType, int reportId, string brCode)
        {
            string reportHeader = "Member Suspense Debtor Member-wise from " + fromDateStr + " to " + toDateStr;
            DateTime.TryParse(fromDateStr, out DateTime fromDate);
            DateTime.TryParse(toDateStr, out DateTime toDate);
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName },
                { "paramReportHeader",reportHeader  }
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                pdfAsBytes = await _reportsMemberHandler.GetNewMembersToPrintAsBytes("Ds_MemberTrn", fromDate, toDate, brCode, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }

        [HttpGet]
        [Route("GetMemberKYC/{memId:decimal}/{reportId:int}/{brCode}")]
        public async Task<byte[]> GetMemberKYC(decimal memId, int reportId, string brCode)
        {
            var report = await _reportsHandler.GetReportNameWithSignature(reportId);
            var path = $"{this._webHostEnvironment.ContentRootPath}\\Reports\\" + report.ReportFileName;
            var societyName = await _generalHandler.GetSocietyName(brCode);
            //var kyc = await _reportsMemberHandler.GetMemberKYC(memId);
            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", societyName }
            };
            byte[] pdfAsBytes = Array.Empty<byte>();
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                //pdfAsBytes = _reportsHandler.CreateLocalReport("Ds_MemberKYC", stream, kyc, parameters);
                pdfAsBytes = await _reportsMemberHandler.GetMemberKYCToPrintAsBytes("Ds_MemberKYC", memId,brCode, stream, parameters);
            }
            var pdfReport = CreatePDFAsBytes(pdfAsBytes);
            return pdfReport;
        }
        private IActionResult CreatePDF(byte[] pdfAsBytes)
        {
            try
            {
                return File(pdfAsBytes, "application/pdf");
            }
            catch (Exception)
            {
                return File(Array.Empty<byte>(), "application/pdf");
            }
        }

        private byte[] CreatePDFAsBytes(byte[] pdfAsBytes)
        {
            try
            {
                return  File(pdfAsBytes, "application/pdf").FileContents;
            }
            catch (Exception)
            {
                return File(Array.Empty<byte>(), "application/pdf").FileContents;
            }
        }
    }
}
