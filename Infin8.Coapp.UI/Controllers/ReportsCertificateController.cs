using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;

namespace Infin8.Coapp.UI.Controllers
{
    /// <summary>Certificate reports 117 (loan), 118 (FD loan) and 119 (FD interest payment).</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsCertificateController : ControllerBase
    {
        private readonly IReportsCertificateHandler _certificateHandler;
        private readonly IReportsHandler _reportHandler;
        private readonly IGeneralHandler _generalHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportsCertificateController(IReportsCertificateHandler certificateHandler, IReportsHandler reportHandler,
            IGeneralHandler generalHandler, IWebHostEnvironment webHostEnvironment)
        {
            _certificateHandler = certificateHandler;
            _reportHandler = reportHandler;
            _generalHandler = generalHandler;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>The member's loans for the loan-certificate picker. BrCode comes from the caller's claims.</summary>
        [HttpGet]
        [Route("MemberLoans/{memId:decimal}")]
        public async Task<ActionResult<List<DropdownItem>>> MemberLoans(decimal memId)
        {
            return Ok(await _certificateHandler.GetMemberLoansAsync(memId, this.GetUserInfoDto().BrCode ?? ""));
        }

        /// <summary>Renders one certificate to PDF; 400 with a reason when there is nothing to certify.</summary>
        [HttpPost]
        [Route("print")]
        public async Task<IActionResult> Print(rptReportCertificateObject request)
        {
            request.BrCode = this.GetUserInfoDto().BrCode ?? "";
            var data = await _certificateHandler.BuildAsync(request);
            if (data.Message != null) return BadRequest(data.Message);

            var report = await _reportHandler.GetReportNameWithSignature(request.ReportId);
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
            var localReport = new LocalReport { EnableExternalImages = true };
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                localReport.LoadReportDefinition(stream);
            }
            if (data.DataSetName != null) localReport.DataSources.Add(new ReportDataSource(data.DataSetName, data.Rows));
            localReport.SetParameters(new[]
            {
                new ReportParameter("paramSocietyName", await _generalHandler.GetSocietyName(request.BrCode)),
                new ReportParameter("paramThirdSignature", report.ThirdSignature ?? ""),
                new ReportParameter("paramContent", data.Content),
                new ReportParameter("paramAsOnDate", request.AsOnDate.ToString("dd-MM-yyyy"))
            });
            return File(localReport.Render("PDF"), "application/pdf");
        }
    }
}
