using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.Map.WebForms.BingMaps;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StagingController : ControllerBase
    {
        //readonly IStagingMasterHandler _stagingMasterHandler;
        readonly IStagingDetailsHandler _stagingDetailsHandler;
        readonly ITransactionsHandler _transactionsHandler;
        public StagingController(IStagingMasterHandler stagingMasterHandler, IStagingDetailsHandler stagingDetailsHandler, ITransactionsHandler transactionsHandler)
        {
            //_stagingMasterHandler = stagingMasterHandler;
            _stagingDetailsHandler = stagingDetailsHandler;
            _transactionsHandler = transactionsHandler;
        }

        [HttpPost]
        [Route("AddStaging")]
        public async Task<ActionResult> AddStagingDetail([FromBody] Staging_Details stagingDetails)
        {
            var result = await _stagingDetailsHandler.AddStagingDetails(stagingDetails);
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
            if (!DateTime.TryParse(createdDate, out var transactionDate))
            {
                return BadRequest("Invalid date format");
            }

            var query = await _stagingDetailsHandler.GetCheckerDashboard(transactionDate, stagingStatus, brCode);
            return query?.Count > 0 ? Ok(query) : NotFound();
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
        [Route("VerifyForFixedDepositLoanRecovery/{accountId:int}/{memId:decimal}")]
        public async Task<ActionResult<bool>> VerifyForFixedDepositLoanRecovery(int accountId, decimal memId)
        {
            var result = await _stagingDetailsHandler.VerifyForFixedDepositLoanRecovery(accountId, memId);
            return Ok(result);
        }

        [HttpPost]
        [Route("UpdateStagingStatus")]

        [HttpPost]
        [Route("MakeStaging")]
        public async Task<ActionResult<bool>> MakeStagingDetails([FromBody] decimal stagingId)
        {
            var result = await _stagingDetailsHandler.MakeStagingDetails(stagingId);
            return Ok(result);
        }
        [HttpPost]
        [Route("ApproveStaging")]
        public async Task<ActionResult<bool>> ApproveStaging([FromBody] decimal stagingId)
        {
            //bool isApproved = true; // Replace with actual approval logic
            var result = await _transactionsHandler.SaveTransaction(stagingId, "MTRN", 110010000002, 110010000023); // Assuming "member" is the vocMode and 0 is yrId for now
            if (result)
            {
                return Ok(true);
            }
            else
            {
                return BadRequest("Failed to approve staging");
            }
        }

    }
}
