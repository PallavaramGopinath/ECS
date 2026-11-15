using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS Loan on Recurring Deposit, acc 14 (spec 17). BrCode and business day come from the caller's claims; every
    /// endpoint is gated server-side to ECS societies. Posting happens at approval in TransactionsHandler.SaveTransaction.
    /// </summary>
    [Route("api/rdloan")]
    [ApiController]
    [Authorize]
    public class RdLoanController : ControllerBase
    {
        readonly IEcsRdLoanHandler _rdLoanHandler;
        readonly IGeneralHandler _generalHandler;

        public RdLoanController(IEcsRdLoanHandler rdLoanHandler, IGeneralHandler generalHandler)
        {
            _rdLoanHandler = rdLoanHandler;
            _generalHandler = generalHandler;
        }

        /// <summary>Scheme, eligibility %, ROI margin and the member's pledgeable RDs as on the business day (§4.1).</summary>
        [HttpGet]
        [Route("Setup/{memId}")]
        public async Task<ActionResult<EcsRdLoanSetupVM>> Setup(decimal memId)
        {
            var u = this.GetUserInfoDto();
            var brCode = u.BrCode ?? "";
            var businessDay = u.CurrentDate == DateTime.MinValue ? DateTime.Today : u.CurrentDate;
            if (!await IsEcsAsync(brCode))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new EcsRdLoanSetupVM { ApplicationDate = businessDay, SetupError = "Loan on RD is available only to ECS societies." });
            try
            {
                return Ok(await _rdLoanHandler.GetSetupAsync(memId, brCode, businessDay));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new EcsRdLoanSetupVM { ApplicationDate = businessDay, SetupError = ex.Message });
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
