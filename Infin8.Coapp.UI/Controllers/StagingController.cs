using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.UI.Client.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

using System.Security.Claims;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StagingController : ControllerBase
    {
        readonly IStagingMasterHandler _stagingMasterHandler;
        readonly IStagingDetailsHandler _stagingDetailsHandler;
        readonly ITransactionsHandler _transactionsHandler;
        public StagingController(IStagingMasterHandler stagingMasterHandler, IStagingDetailsHandler stagingDetailsHandler,
            ITransactionsHandler transactionsHandler)
        {
            //_stagingMasterHandler = stagingMasterHandler;
            _stagingDetailsHandler = stagingDetailsHandler;
            _transactionsHandler = transactionsHandler;
            _stagingMasterHandler = stagingMasterHandler;
        }

        [HttpPost]
        [Route("AddStaging")]
        public async Task<ActionResult> AddStagingDetail([FromBody] Staging_Details stagingDetails)
        {
            var result = await _stagingDetailsHandler.AddStagingDetails(stagingDetails);
            if (result) return Ok();
            else return BadRequest();
        }
        [HttpPost]
        [Route("AddStagingForAccounting")]
        public async Task<ActionResult> AddStagingDetailForAccounting([FromBody] List<Staging_Details> stagingDetails)
        {
            var result = await _stagingDetailsHandler.AddStagingForAccountTransaction(stagingDetails);
            if (result) return Ok();
            else return BadRequest();
        }

        [HttpGet]
        [Route("GetInitiatedAccounts")]
        //[Route("GetInitiatedAccounts/{createdBy:decimal}/{createdDate:datetime}/{memId:decimal}/{stagingStatus}/{brCode}")]
        public async Task<ActionResult<List<AccountTransactionVM>>> GetInitiatedAccounts([FromQuery] string createdDate, [FromQuery] decimal memId, [FromQuery] string stagingStatus, [FromQuery] string brCode)
        {
            List<AccountTransactionVM> accList = new();
            if (!DateTime.TryParse(createdDate, out var transactionDate))
            {
                return BadRequest("Invalid date format");
            }
            var result = await _stagingDetailsHandler.GetAccountTransactions(transactionDate, memId, stagingStatus, brCode);
            if (result != null && result.Count > 0)
            {
                accList = result.ToList();
                return Ok(accList);
            }
            else return NotFound();
        }

        [HttpGet]
        [Route("GetCheckerDashboard")]
        public async Task<ActionResult<List<DtoCheckerDashboard>>> GetCheckerDashboard(
            [FromQuery] string createdDate,
            [FromQuery] string stagingStatus,
            [FromQuery] string brCode)
        {
            //List<DtoCheckerDashboard> checkerDashboardList = new();
            //var query = await _stagingDetailsHandler.GetCheckerDashboard(createdDate, stagingStatus, brCode);
            //if(query != null && query.Count >0) 
            //{ 
            //    checkerDashboardList = query.ToList();
            //    return Ok(checkerDashboardList);
            //}
            //else return NotFound();
            List<DtoCheckerDashboard> checkerDashboardList = new();
            if (!DateTime.TryParse(createdDate, out var transactionDate))
            {
                return BadRequest("Invalid date format");
            }

            var query = await _stagingDetailsHandler.GetCheckerDashboard(transactionDate, stagingStatus, brCode);
            //return query?.Count > 0 ? Ok(query) : NotFound();
            if (query != null && query.Any())
            {
                checkerDashboardList = query.ToList();
                return Ok(checkerDashboardList);
            }
            else
            {
                return NotFound();
            }
            
        }

        [HttpGet]
        [Route("GetChekerDashboardById/{stagingId:decimal}")]
        public async Task<ActionResult<List<AccountTransactionVM>>> GetChekerDashboardById(decimal stagingId)
        {
            List<AccountTransactionVM> accList = new();
            var result = await _stagingDetailsHandler.GetChekerDashboardById(stagingId);
            if (result != null && result.Count > 0)
            {
                accList = result.ToList();
                return Ok(accList);
            }
            else return NotFound();
        }

        [HttpGet]
        [Route("GetStagingDetailsById/{stagingId:decimal}/{relatedAccountId:int}")]
        public async Task<ActionResult<Staging_Details>> GetStagingDetailsById(decimal stagingId, int relatedAccountId)
        {
            Staging_Details details = new Staging_Details();
            var result = await _stagingDetailsHandler.GetStagingDetailsById(stagingId, relatedAccountId);
            if (result != null)
            {
                details = result;
                return Ok(details);
            }
            else return NotFound();
        }

        [HttpGet]
        [Route("GetStagingDetailsListById/{stagingId:decimal}")]
        public async Task<ActionResult<List<DtoAccountTransactionRelatedData>>> GetStagingDetailsListById(decimal stagingId)
        {
            List<DtoAccountTransactionRelatedData> details = new();
            var result = await _stagingDetailsHandler.GetStagingDetailsListById(stagingId);
            if (result != null)
            {
                details = result.ToList();
                return Ok(details);
            }
            else return NotFound();
        }

        [HttpDelete]
        [Route("DeleteStaging/{stagingId:decimal}/{relatedAccountId:int}")]
        public async Task<ActionResult<bool>> DeleteStaging(decimal stagingId, int relatedAccountId)
        {
            var result = await _stagingDetailsHandler.DeleteStagingDetailsByStagingId(stagingId, relatedAccountId);
            return Ok(result);
        }
        [HttpGet]
        [Route("VerifyTransactionExistsInStagingDetails/{memId:decimal}/{transactedDate}/{relatedAccountId:int}")]
        public async Task<ActionResult<int>> VerifyTransactionExistsInStagingDetails(decimal memId, string transactedDate, int relatedAccountId)
        {
            var result = await _stagingDetailsHandler.IsAlreadyTransactedButNotVerifiedOrRejected(memId, transactedDate, relatedAccountId);
            return Ok(result);
        }

        [HttpGet]
        [Route("VerifyForFixedDepositLoanRecovery/{accountId:int}/{memId:decimal}/{createdDate}")]
        public async Task<ActionResult<bool>> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId, string createdDate)
        {
            DateTime.TryParse(createdDate, out DateTime trnDate);
            var result = await _stagingDetailsHandler.VerifyForFixedDepositLoanRecovery(accountId, memId,trnDate );
            return Ok(result);
        }

        //[HttpPost]
        //[Route("UpdateStagingStatus")]

        [HttpPost]
        [Route("MakeStaging")]
        public async Task<ActionResult<bool>> MakeStagingDetails([FromBody] decimal stagingId)
        {
            var result = await _stagingDetailsHandler.MakeStagingDetails(stagingId);
            return Ok(result);
        }

        [HttpPost]
        [Route("ApproveStaging")]
        public async Task<ActionResult<DtoVoucher>> ApproveStaging([FromBody] decimal stagingId)
        {
            DtoVoucher voucherData = new();
            bool result = false;
            decimal vocId = 0;
            Staging_Master master = new();
            /// Step 1: Check Transaction Type (Member Transaction, Accounting Transaction or Staff Transaction)
            /// public class ApproveStagingResponse
            /// {
            /// public decimal VoucherId { get; set; }
            /// public bool Success { get; set; }
            /// public string? Message { get; set; }
            /// }
            /// result = await response.Content.ReadFromJsonAsync<ApproveStagingResponse>();
            //await JSRuntime.InvokeVoidAsync("alert", $"{result?.Message} Voucher Id: {result?.VoucherId}");

            var stagingMaster = await _stagingMasterHandler.GetStagingMasterById(stagingId);
            voucherData.brCode = stagingMaster.BrCode;
            switch (stagingMaster.Type!.Trim())
            {
                case "Member Transaction":
                case "Staff Transaction":
                    var response = await _transactionsHandler.SaveTransaction(stagingId, stagingMaster.Type!.Trim() == "Member Transaction" ? "MTRN" : "STRN", 110010000002, 110010000024);
                    if (response.Voc_Id >0) result = true; else result = false;
                    voucherData.Voc_Id = response.Voc_Id;
                    voucherData.brCode = response.brCode;
                    voucherData.Transactions = response.Transactions;
                    break;
                case "Account Transaction":
                    var accResponse = await _transactionsHandler.SaveAccountTransaction(stagingId, "ACTR", 110010000002, 110010000024);
                    if (accResponse.Voc_Id >0) result = true; else result = false;
                    voucherData.Voc_Id = accResponse.Voc_Id;
                    voucherData.brCode = accResponse.brCode;
                    voucherData.Transactions = accResponse.Transactions;
                    break;
            }

            if (result)
            {
                return Ok(voucherData);
            }
            else
            {
                return BadRequest("Failed to approve staging");
            }
        }

        [HttpPost]
        [Route("RejectStaging")]
        public async Task<ActionResult<bool>> RejectStaging([FromBody] decimal stagingId)
        {
            var result = await _transactionsHandler.RejectTransaction(stagingId, 110010000002);
            if (result) return Ok(result);
            else return BadRequest("Failed to reject staging");
        }


        [HttpGet]
        [Route("GetStagingMasterById/{stagingId:decimal}")]
        public async Task<ActionResult<Staging_Master>> GetStagingMasterById(decimal stagingId)
        {
            var result = await _stagingMasterHandler.GetStagingMasterById(stagingId);
            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }

        }
    }
}
