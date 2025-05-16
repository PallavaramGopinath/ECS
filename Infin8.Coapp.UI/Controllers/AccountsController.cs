using Infin8.Coapp.BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        readonly IAccountsHandler _accountsHandler;
        public AccountsController(IAccountsHandler accountsHandler)
        {
            _accountsHandler = accountsHandler;
        }
        [HttpGet]
        [Route("GetLedgerBalance")]
        public async Task<ActionResult<double>> GetLedgerBalance([FromQuery] decimal ledId, [FromQuery] decimal yrId, [FromQuery] DateTime uptoDate)
        {
            double ledBalance = 0;
            try
            {
                ledBalance = await _accountsHandler.GetLedgerBalance(ledId, yrId, uptoDate);
            }
            catch (Exception)
            {
                return NotFound();
            }
            
            return Ok(ledBalance);
        }
    }
}
