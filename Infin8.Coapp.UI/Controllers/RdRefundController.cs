using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS RD Refund, acc 10 (spec 16). BrCode and business day come from the caller's claims; every endpoint is
    /// gated server-side to ECS societies. Posting happens at approval in TransactionsHandler.SaveTransaction.
    /// </summary>
    [Route("api/rdrefund")]
    [ApiController]
    [Authorize]
    public class RdRefundController : ControllerBase
    {
        readonly IEcsRdRefundHandler _rdRefundHandler;
        readonly IGeneralHandler _generalHandler;

        public RdRefundController(IEcsRdRefundHandler rdRefundHandler, IGeneralHandler generalHandler)
        {
            _rdRefundHandler = rdRefundHandler;
            _generalHandler = generalHandler;
        }

        /// <summary>The member's open RDs with the payable figures as on the business day (§4–§6).</summary>
        [HttpGet]
        [Route("Rds/{memId}")]
        public async Task<ActionResult<EcsRdRefundLoadVM>> Rds(decimal memId)
        {
            var u = this.GetUserInfoDto();
            var brCode = u.BrCode ?? "";
            var businessDay = u.CurrentDate == DateTime.MinValue ? DateTime.Today : u.CurrentDate;
            if (!await IsEcsAsync(brCode))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new EcsRdRefundLoadVM { ApplicationDate = businessDay, Message = "RD Refund is available only to ECS societies." });
            try
            {
                return Ok(await _rdRefundHandler.GetRdsAsync(memId, brCode, businessDay));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new EcsRdRefundLoadVM { ApplicationDate = businessDay, Message = ex.Message });
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
