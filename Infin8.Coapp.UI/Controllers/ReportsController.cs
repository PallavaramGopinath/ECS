using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportsHandler _reportsHandler;

        public ReportsController( IReportsHandler reportsHandler )
        {
            _reportsHandler = reportsHandler;
        }

        [HttpGet]
        [Route("GetReportsList")]
        public async Task<ActionResult<List<Reports_Master>>> GetReportsList()
        {
            List<Reports_Master> reportsList = new List<Reports_Master>();
            reportsList = await _reportsHandler.GetReportsList();
            if (reportsList == null && !reportsList!.Any())
            {
                return NotFound();
            }
            return Ok(reportsList);
        }
    }
}
