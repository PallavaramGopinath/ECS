using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        [Route("IsBankLedger/{ledgerId:decimal}/{brCode}")]
        public async Task<ActionResult<bool>> IsBankLedger(decimal ledgerId,string brCode)
        {
            bool result = false;
            try
            {
                result = await _accountsHandler.IsBankLedger(ledgerId, brCode);
            }
            catch (Exception ex)
            {
                result = false;
                Console.Write(ex.Message);
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLedgerBalance")]
        public async Task<ActionResult<double>> GetLedgerBalance([FromQuery] decimal ledId, [FromQuery] decimal yrId, [FromQuery] DateTime uptoDate, [FromQuery] string brCode)
        {
            double ledBalance = 0;
            try
            {
                ledBalance = await _accountsHandler.GetLedgerBalance(ledId, yrId, uptoDate, brCode);
            }
            catch (Exception)
            {
                return NotFound();
            }
            
            return Ok(ledBalance);
        }

        [HttpGet]
        [Route("GetLedgerBalanceWithFnlId")]
        public async Task<ActionResult<DtoLedgerBalance>> GetLedgerBalanceWithFnlId([FromQuery] decimal ledId, [FromQuery] decimal yrId, [FromQuery] DateTime uptoDate, [FromQuery] string brCode)
        {
            //double ledBalance = 0;
            DtoLedgerBalance ledBalance = new();
            try
            {
                var result = await _accountsHandler.GetLedgerBalanceWithFnlId(ledId, yrId, uptoDate, brCode);
                ledBalance.Ledger_Balance = result.Ledger_Balance;
                ledBalance.Fin_Id = result.Fin_Id;
                ledBalance.Cash_Led_Id = result.Cash_Led_Id;
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Ok(ledBalance);
        }


        [HttpGet]
        [Route("GetAccountName/{id:int}")]
        public async Task<ActionResult<string>> GetAccountName(int id)
        {
            string accountName = "";
            try
            {
                accountName = await _accountsHandler.GetTransactedAccountNameFromAccount_Transactions(id);
            }
            catch (Exception)
            {
                return NotFound();
            }

            return Ok(accountName);
        }
    }
}
