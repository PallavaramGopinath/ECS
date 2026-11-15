using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS Recurring Deposit Loan Receipt, acc 25 (spec 18). BrCode and business day come from the caller's claims; every
    /// endpoint is gated server-side to ECS societies. Posting happens at approval in TransactionsHandler.SaveTransaction.
    /// </summary>
    [Route("api/rdloanreceipt")]
    [ApiController]
    [Authorize]
    public class RdLoanReceiptController : ControllerBase
    {
        readonly IEcsRdLoanReceiptHandler _rdLoanReceiptHandler;
        readonly IGeneralHandler _generalHandler;

        public RdLoanReceiptController(IEcsRdLoanReceiptHandler rdLoanReceiptHandler, IGeneralHandler generalHandler)
        {
            _rdLoanReceiptHandler = rdLoanReceiptHandler;
            _generalHandler = generalHandler;
        }

        /// <summary>The member's open loans on RD with principal outstanding and interest due as on the business day (§4.1).</summary>
        [HttpGet]
        [Route("Loans/{memId}")]
        public async Task<ActionResult<EcsRdLoanReceiptSetupVM>> Loans(decimal memId)
        {
            var u = this.GetUserInfoDto();
            var brCode = u.BrCode ?? "";
            var businessDay = u.CurrentDate == DateTime.MinValue ? DateTime.Today : u.CurrentDate;
            if (!await IsEcsAsync(brCode))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new EcsRdLoanReceiptSetupVM { ApplicationDate = businessDay, SetupError = "RD loan receipt is available only to ECS societies." });
            try
            {
                return Ok(await _rdLoanReceiptHandler.GetLoansAsync(memId, brCode, businessDay));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new EcsRdLoanReceiptSetupVM { ApplicationDate = businessDay, SetupError = ex.Message });
            }
        }

        /// <summary>Server-side ECS gate: resolves Gen_Bank_Name.Bank_Type the same way api/Auth/me does.</summary>
        private async Task<bool> IsEcsAsync(string brCode)
        {
            if (string.IsNullOrEmpty(brCode)) return false;
            try
            {
                var society = await _generalHandler.GetSocietyData(brCode);
                return (society?.Bank_Type ?? 0).ToInstitutionType() == InstitutionType.ECS;
            }
            catch
            {
                return false;
            }
        }
    }
}
