using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;

namespace Infin8.Coapp.UI.Controllers
{
    /// <summary>Income tax reports 109–112 (reports_master group 14).</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsITController : ControllerBase
    {
        private readonly IReportsITHandler _itHandler;
        private readonly IReportsHandler _reportHandler;
        private readonly IGeneralHandler _generalHandler;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ReportsITController(IReportsITHandler itHandler, IReportsHandler reportHandler,
            IGeneralHandler generalHandler, IWebHostEnvironment webHostEnvironment)
        {
            _itHandler = itHandler;
            _reportHandler = reportHandler;
            _generalHandler = generalHandler;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>Renders one IT report to PDF. BrCode comes from the caller's claims.</summary>
        [HttpPost]
        [Route("print")]
        public async Task<IActionResult> Print(rptReportITObject request)
        {
            request.BrCode = this.GetUserInfoDto().BrCode ?? "";
            var data = await _itHandler.BuildAsync(request);
            if (data.Message != null) return BadRequest(data.Message);

            var report = await _reportHandler.GetReportNameWithSignature(request.ReportId);
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "");
            var localReport = new LocalReport { EnableExternalImages = true };
            using (FileStream stream = System.IO.File.OpenRead(path))
            {
                localReport.LoadReportDefinition(stream);
            }
            localReport.DataSources.Add(new ReportDataSource(data.DataSetName, data.Rows));
            var parameters = new List<ReportParameter> { new("paramSocietyName", await _generalHandler.GetSocietyName(request.BrCode)) };
            parameters.AddRange(data.Parameters.Select(p => new ReportParameter(p.Key, p.Value)));
            localReport.SetParameters(parameters);
            return File(localReport.Render("PDF"), "application/pdf");
        }
    }
}
