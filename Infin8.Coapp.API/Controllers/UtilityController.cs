using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        public UtilityController()
        {
            
        }
        [HttpGet]
        [Route("GetAge")]
        public ActionResult<int> GetAge([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate) 
        {
            int age = Utility.Utilities.GetAge(fromDate, toDate);   
            return Ok(age);
        }
        [HttpGet]
        [Route("GetTDMaturityDate")]
        public ActionResult<DateTime> GetTDMaturityDate([FromQuery] DateTime TDDate, [FromQuery] int NoOfMonths, [FromQuery] int NoOfDays)
        {
            DateTime maturityDate = Utility.Utilities.GetTDMaturityDate(TDDate, NoOfMonths, NoOfDays);
            return Ok(maturityDate);
        }
        [HttpGet]
        [Route("GetFDMaturityAmount")]
        public ActionResult<double> GetFDMaturityAmount([FromQuery] double Principal, [FromQuery] DateTime FromDate, 
            [FromQuery] DateTime ToDate, [FromQuery] int PeriodInMonths, [FromQuery] int PeriodInDays, [FromQuery] double ROI,
            [FromQuery] int IntPayablePrd, [FromQuery] bool IsDiscountRate, [FromQuery] int CompoundFrequency)
        {
            double maturityAmt = Utility.Utilities.GetFDMaturityAmount(Principal, FromDate,ToDate,PeriodInMonths,PeriodInDays, 
                    ROI,IntPayablePrd ,IsDiscountRate,CompoundFrequency );
            return Ok(maturityAmt);
        }
    }
}
