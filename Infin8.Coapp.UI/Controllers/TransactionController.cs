using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        readonly ITransactionsHandler _transactionHandler;
        public TransactionController(ITransactionsHandler transactionsHandler)
        {
            _transactionHandler = transactionsHandler;
        }
        [HttpGet]
        [Route("GetAccountNames/{accountStatus}/{accountBelongTo}")]
        public async Task<ActionResult<List<DropdownItem>>> GetTransactionAccounts(string accountStatus,string accountBelongTo)
        {
            List<DropdownItem> accounts = new List<DropdownItem>();
            accounts =  await _transactionHandler.GetTransactionAccounts(accountStatus, accountBelongTo);
            if(accounts == null)
            {
                return NotFound();
            }
            return Ok(accounts);
        }

        [HttpGet]
        [Route("GetSuspenseAccounts/{suspenseType:int}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetSuspenseLedgerItems(int suspenseType,string brCode)
        {
            List<DropdownItem> ledgers = new List<DropdownItem>();
            ledgers = await _transactionHandler.GetSuspenseLedgerItems(suspenseType,brCode);
            if(ledgers == null)
            {
                return NotFound();
            }
            return Ok(ledgers);
        }

        [HttpGet]
        [Route("GetShareCapitalLedger/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetShareCapitalLedger(string brCode)
        {
            List<DropdownItem> ledgers = new List<DropdownItem>();
            ledgers = await _transactionHandler.GetShareCapitalLedgerItem(brCode);
            if (ledgers == null)
            {
                return NotFound();
            }
            return Ok(ledgers);
        }
        [HttpGet]
        [Route("GetBankLedgers/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetBankLedger(string brCode)
        {
            List<DropdownItem> ledgers = new List<DropdownItem>();
            ledgers = await _transactionHandler.GetBankLedgerItems(brCode);
            if (ledgers == null)
            {
                return NotFound();
            }
            return Ok(ledgers);
        }
        [HttpGet]
        [Route("GetLedgersExceptBankLedger/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetLedgersExceptBankLedger(string brCode)
        {
            List<DropdownItem> ledgers = new List<DropdownItem>();
            ledgers = await _transactionHandler.GetLedgersExpectBankLedgerItems(brCode);
            if (ledgers == null)
            {
                return NotFound();
            }
            return Ok(ledgers);
        }

        [HttpGet]
        [Route("GetComponentName/{accountId:int}")]
        public async Task<ActionResult< string>> GetComponentName(int accountId)
        {
            string componentName = await _transactionHandler.GetComponentName(accountId);
            if (string.IsNullOrWhiteSpace(componentName)) return NotFound();
            else return Ok(componentName);
        }
    }
}
