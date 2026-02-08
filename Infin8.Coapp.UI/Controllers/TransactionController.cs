using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;
using System.IdentityModel.Claims;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles ="Admin,Maker,Checker")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        readonly ITransactionsHandler _transactionHandler;
        private readonly IWebHostEnvironment _environment;
        private readonly IStagingDetailsHandler _stagingDetailsHandler;
        public TransactionController(ITransactionsHandler transactionsHandler, IWebHostEnvironment environment, 
                IStagingDetailsHandler stagingDetailsHandler)
        {
            _transactionHandler = transactionsHandler;
            _environment = environment;
            _stagingDetailsHandler = stagingDetailsHandler;
        }
        [HttpGet]
        [Route("GetAccountNames/{accountStatus}/{accountBelongTo}")]
        public async Task<ActionResult<List<DropdownItem>>> GetTransactionAccounts(string accountStatus, string accountBelongTo)
        {
            //var brCode = User.FindFirst("BrCode")?.Value.ToString();
            List<DropdownItem> accounts = new List<DropdownItem>();
            accounts = await _transactionHandler.GetTransactionAccounts(accountStatus, accountBelongTo);
            if (accounts == null)
            {
                return NotFound();
            }
            return Ok(accounts);
        }

        [HttpGet]
        [Route("GetAllAccountNames")]
        public async Task<ActionResult<List<DtoAccount_Transactions>>> GetAllTransactionAccounts()
        {
            List<DtoAccount_Transactions> dtoAccounts = new List<DtoAccount_Transactions>();
            dtoAccounts = await _transactionHandler.GetAllAccountsTransactions();
            if (dtoAccounts == null)
            {
                return NotFound();
            }
            return Ok(dtoAccounts);
        }

        [HttpGet]
        [Route("GetAllLedgers/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetAllLedgers(string brcode)
        {
            List<DropdownItem> ledgers = new List<DropdownItem>();
            ledgers = await _transactionHandler.GetAllLedgerItems(brcode);
            if (ledgers == null)
            {
                return NotFound();
            }
            return Ok(ledgers);
        }

        [HttpGet]
        [Route("GetSuspenseAccounts/{suspenseType:int}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetSuspenseLedgerItems(int suspenseType, string brCode)
        {
            List<DropdownItem> ledgers = new List<DropdownItem>();
            ledgers = await _transactionHandler.GetSuspenseLedgerItems(suspenseType, brCode);
            if (ledgers == null)
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
        public async Task<ActionResult<string>> GetComponentName(int accountId)
        {
            string componentName = await _transactionHandler.GetComponentName(accountId);
            if (string.IsNullOrWhiteSpace(componentName)) return NotFound();
            else return Ok(componentName);
        }

        [HttpGet]
        [Route("GetViewComponentName/{accountId:int}")]
        public async Task<ActionResult<string>> GetViewComponentName(int accountId)
        {
            string componentName = await _transactionHandler.GetViewComponentName(accountId);
            if (string.IsNullOrWhiteSpace(componentName)) return NotFound();
            else return Ok(componentName);
        }

        [HttpPost("upload")]
        [RequestSizeLimit(5_000_000)] // 5MB limit
        public async Task<IActionResult> UploadImage([FromBody] ImageUploadModel model)
        {
            try
            {
                // In ImageController
                if (!model.ImageData.StartsWith("data:image/jpeg;base64,") &&
                    !model.ImageData.StartsWith("data:image/png;base64,"))
                {
                    return BadRequest("Invalid image format");
                }
                // Extract base64 data
                var base64Data = model.ImageData.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);

                // Create unique filename
                var fileName = $"jewel_{Guid.NewGuid()}.jpg";
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "jewels");

                // Ensure directory exists
                Directory.CreateDirectory(uploadsPath);

                // Save file
                var filePath = Path.Combine(uploadsPath, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes);

                // Return relative path
                return Ok(new { path = $"/uploads/jewels/{fileName}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex}");
            }
        }

        [HttpPut]
        [Route("MakeStagingDetails/{stagingId:decimal}")]
        public async Task<IActionResult> MakeStagingDetails(decimal stagingId)
        {
            try
            {
                bool result = await _stagingDetailsHandler.MakeStagingDetails(stagingId);
                if (result)
                {
                    return Ok(new { message = "Staging details made successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to make staging details." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
