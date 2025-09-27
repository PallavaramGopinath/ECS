using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessDayController : ControllerBase
    {
        readonly ICalendarHandler _calendarHandler;
        readonly IStagingBalanceHandler _stagingBalanceHandler;
        public BusinessDayController(ICalendarHandler calendarHandler, IStagingBalanceHandler stagingBalanceHandler)
        {
            _calendarHandler = calendarHandler;
            _stagingBalanceHandler = stagingBalanceHandler;
        }
        [HttpGet]
        [Route("VerifyDate/{brCode}")]
        public async Task<ActionResult<bool>> VerifyDayBegin(string brCode)
        {
            var result = await _calendarHandler.VerifyDayBegin(brCode);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetCurrentDate/{brCode}")]
        public async Task<ActionResult<DateTime>> GetCurrentDate(string brCode)
        {
            DateTime currentDate = DateTime.Now;
            currentDate = await _calendarHandler.GetCurrentDate(brCode);
            return Ok(currentDate);
        }

        [HttpPost]
        [Route("day-end")]
        public async Task<ActionResult<bool>> DayEndProcess([FromBody] DtoDayProcess dayProcess)
        {
            var result = await _calendarHandler.DayEndProcess(dayProcess);
            return Ok(result);
        }

        [HttpPut]
        [Route("UpdateDayBegin/{currentDate}/{brCode}")]
        public async Task<ActionResult<int>> UpdateDayBegin(DateTime currentDate, string brCode)
        {
            int result = 0;
            result = await _calendarHandler.UpdateCalendarStatus(currentDate, brCode, "B");
            return Ok(result);
        }

        [HttpGet]
        [Route("Getledgerbalance/{yrId:decimal}/{accountingDate}/{fromDate}/{toDate}/{created_By:decimal}/{brCode}")]
        public async Task<ActionResult<List<DtoAccountsBalance>>> GetStagingBalance(decimal yrId, string accountingDate, string fromDate, string toDate, decimal created_By, string brCode)
        {
            List<DtoAccountsBalance> balanceList = new();

            DateTime.TryParse(accountingDate, out DateTime accountingDateFormated);
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate, out DateTime toDateFormated);
            var balance = await _stagingBalanceHandler.GetStagingBalance(yrId, accountingDateFormated, fromDateFormated, toDateFormated, created_By, brCode!);
            if (balance != null && balance.Any())
            {
                balanceList = balance.ToList();
                return Ok(balanceList);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("day-begin/{brCode}")]
        public async Task<ActionResult<DateTime>> GetDayBeginDate(string brCode)
        {
            DateTime currentDate = DateTime.Now;
            var result = await _calendarHandler.DayBeginProcess(brCode);
            currentDate= result.Date;
            return Ok(currentDate);
            //else
            //{
            //    return StatusCode(500, new { errorMessage = "error in fetcing current working date" });
            //}
        }
    }
}
