using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.UI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        readonly IAccountsHandler _accountsHandler;
        readonly IFinLedgerHandler _finLedgerHandler;
        readonly IFinLedgerGroupHandler _finLedgerGroupHandler;
        readonly IFinLedgerFnlHandler _finLedgerFnlHandler;
        readonly IFinLedgerTrnHandler _finLedgerTrnHandler;
        public AccountsController(IAccountsHandler accountsHandler,IFinLedgerHandler finLedgerHandler,
            IFinLedgerGroupHandler finLedgerGroupHandler , IFinLedgerTrnHandler finLedgerTrnHandler,
            IFinLedgerFnlHandler finLedgerFnlHandler  )
        {
            _accountsHandler = accountsHandler;
            _finLedgerHandler = finLedgerHandler;
            _finLedgerGroupHandler = finLedgerGroupHandler;
            _finLedgerTrnHandler = finLedgerTrnHandler;
            _finLedgerFnlHandler = finLedgerFnlHandler;
        }

        [HttpPost]
        [Route("AddFinLedger")]
        public async Task<ActionResult<Fin_Ledger>> AddFinLedger([FromBody] Fin_Ledger finLedger)
        {
            var result = await _finLedgerHandler.AddFinLedgerAsync(finLedger);
            if (result) return Ok(result);
            else return NotFound();
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
        [Route("GetCashBalanceAsOnDate")]
        public async Task<ActionResult<double>> GetCashBalanceAsOnDate( [FromQuery] decimal yrId, [FromQuery] DateTime asOnDate, [FromQuery] string brCode)
        {
            double ledBalance = 0;
            try
            {
                ledBalance = await _accountsHandler.GetCashBalanceAsOnDate( yrId, asOnDate, brCode);
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

        [HttpPost]
        [Route("update-ledger-balance")]
        public async Task<ActionResult<bool>> UpdateLedgerBalane(decimal yrId, DateTime fromDate, DateTime toDate, string brCode)
        {
            bool requestResult = false;
            try
            {
                var result = await _accountsHandler.UpdateLedgerBalance(yrId, fromDate, toDate, brCode);
                requestResult = result;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Ok(requestResult);
        }

        [HttpGet]
        [Route("create-year/{yrId:decimal}/{fromDate}/{toDate}/{created_By:decimal}/{brCode}")]
        public async Task<ActionResult<bool>> CreateYear(decimal yrId, string fromDate, string toDate, decimal created_By, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormated);
            DateTime.TryParse(toDate,out DateTime toDateFormated);
            var result = await _accountsHandler.CreateNewFinancialYear(yrId , fromDateFormated,toDateFormated,created_By,brCode );
            if(result ==  true)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Failed to create new financial year");
            }
        }

        [HttpGet]
        [Route("GetLedgerList10Async/{brCode}")]
        public async Task<ActionResult<List<Fin_Ledger>>> GetLedgerList10Async(string brCode)
        {
            List<Fin_Ledger> result = new List<Fin_Ledger>();
            try
            {
                result = await _finLedgerHandler.GetLedgerList10Async(brCode);
            }
            catch (Exception)
            {
                //return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLedgerListAsync/{brCode}")]
        public async Task<ActionResult<List<Fin_Ledger>>> GetLedgerListAsync(string brCode)
        {
            List<Fin_Ledger> result = new List<Fin_Ledger>();
            try
            {
                result = await _finLedgerHandler.GetLedgerListAsync(brCode);
            }
            catch (Exception)
            {
                //return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLedgerVMListAsync/{brCode}")]
        public async Task<ActionResult<List<FinLedgerVM>>> GetLedgerVMListAsync(string brCode)
        {
            List<FinLedgerVM> result = new List<FinLedgerVM>();
            try
            {
                result = await _finLedgerHandler.GetLedgerVMListAsync(brCode);
            }
            catch (Exception)
            {
                //return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLedgerByIdAsync/{Id:decimal}/{brCode}")]
        public async Task<ActionResult<Fin_Ledger>> GetLedgerByIdAsync(decimal Id, string brCode)
        {
            Fin_Ledger result = new Fin_Ledger();
            try
            {
                result = await _finLedgerHandler.GetLedgerByIdAsync(Id, brCode);
            }
            catch (Exception)
            {
                //return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLedgerGroupListAsync/{brCode}")]
        public async Task<ActionResult<List<Fin_Ledger_Grp>>> GetLedgerGroupListAsync(string brCode)
        {
            List<Fin_Ledger_Grp> result = new List<Fin_Ledger_Grp>();
            try
            {
                result = await _finLedgerGroupHandler.GetFinLedgerGroupListAsync(brCode);
            }
            catch (Exception)
            {
                //return NotFound();
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("GetLedgerFnlListAsync")]
        public async Task<ActionResult<List<Fin_Ledger_Fnl>>> GetLedgerFnlListAsync()
        {
            List<Fin_Ledger_Fnl> result = new List<Fin_Ledger_Fnl>();
            try
            {
                result = await _finLedgerFnlHandler.GetFinLedgerFnlListAsync();
            }
            catch (Exception)
            {
                //return NotFound();
            }
            return Ok(result);
        }


    }
}
