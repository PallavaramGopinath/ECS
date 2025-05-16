using Infin8.Coapp.BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessDayController : ControllerBase
    {
        readonly ICalendarHandler _calendarHandler;
        public BusinessDayController(ICalendarHandler calendarHandler)
        {
            _calendarHandler = calendarHandler;
        }
        [HttpGet]
        [Route("VerifyDate/{brCode}")]
        public async Task<ActionResult<bool>> VerifyDayBegin(string brCode)
        {
            var result =  await _calendarHandler.VerifyDayBegin(brCode);
            return Ok(result);
        }
        [HttpGet]
        [Route("GetCurrentDate/{brCode}")]
        public async Task<ActionResult<DateTime>> GetCurrentDate(string brCode)
        {
            DateTime currentDate = DateTime.Now;
            currentDate= await _calendarHandler.GetCurrentDate(brCode);
            return Ok(currentDate);
        }
        [HttpPut]
        [Route("UpdateDayBegin/{currentDate}/{brCode}")]
        public async Task<ActionResult <int>> UpdateDayBegin(DateTime  currentDate, string brCode)
        {
            int result = 0;
            result = await _calendarHandler.UpdateCalendarStatus(currentDate,brCode , "B");
            return Ok(result);
        }
    }
}
