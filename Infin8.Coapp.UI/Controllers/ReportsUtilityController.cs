using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;

namespace Infin8.Coapp.UI.Controllers
{
    /// <summary>Utility prints that need no transaction data: the loan repayment (EMI) schedule, report 126.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsUtilityController : ControllerBase
    {
        private const int EmiScheduleReportId = 126;

        private readonly IReportsHandler _reportHandler;
        private readonly IGeneralHandler _generalHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportsUtilityController(IReportsHandler reportHandler, IGeneralHandler generalHandler, IWebHostEnvironment webHostEnvironment)
        {
            _reportHandler = reportHandler;
            _generalHandler = generalHandler;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>Recomputes the schedule with <see cref="EmiScheduleCalculator"/> and renders it to PDF.</summary>
        [HttpPost]
        [Route("print-emi")]
        public async Task<IActionResult> PrintEmiSchedule(rptReportEmiObject request)
        {
            var invalid = EmiScheduleCalculator.Validate(request);
            if (invalid != null) return BadRequest(invalid);

            var report = await _reportHandler.GetReportNameWithSignature(EmiScheduleReportId);
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
            var localReport = new LocalReport { EnableExternalImages = true };
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                localReport.LoadReportDefinition(stream);
            }
            localReport.DataSources.Add(new ReportDataSource("Ds_UtilityEMI", EmiScheduleCalculator.Schedule(request)));
            localReport.SetParameters(new[]
            {
                new ReportParameter("paramSocietyName", await _generalHandler.GetSocietyName(this.GetUserInfoDto().BrCode ?? "")),
                new ReportParameter("paramReportHeader", EmiScheduleCalculator.Header(request))
            });
            return File(localReport.Render("PDF"), "application/pdf");
        }
    }
}
