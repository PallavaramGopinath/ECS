using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS Recovery Appropriation (spec 13). BrCode / user / year / business day come from the caller's claims;
    /// every endpoint is gated server-side to ECS societies.
    /// </summary>
    [Route("api/ecsrecovery")]
    [ApiController]
    [Authorize]
    public class EcsRecoveryController : ControllerBase
    {
        readonly IEcsRecoveryHandler _recoveryHandler;
        readonly IGeneralHandler _generalHandler;

        public EcsRecoveryController(IEcsRecoveryHandler recoveryHandler, IGeneralHandler generalHandler)
        {
            _recoveryHandler = recoveryHandler;
            _generalHandler = generalHandler;
        }

        private EcsUserContextVM UserContext()
        {
            var u = this.GetUserInfoDto();
            return new EcsUserContextVM
            {
                UsrId = u.UserId,
                YrId = u.YrId,
                BrCode = u.BrCode ?? "",
                CurrentDate = u.CurrentDate == DateTime.MinValue ? DateTime.Today : u.CurrentDate,
                YrBeginningDate = u.YrBeginningDate,
                YrEndDate = u.YrEndDate
            };
        }

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

        private async Task<ActionResult> Run<T>(Func<EcsUserContextVM, Task<T>> action)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new EcsRecoveryResultVM { Success = false, Message = "Recovery Appropriation is available only to ECS societies." });
            try
            {
                var result = await action(ctx);
                if (result == null) return NotFound();
                if (result is EcsRecoveryResultVM { Success: false } failed) return BadRequest(failed);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new EcsRecoveryResultVM { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("State")]
        public Task<ActionResult> State() => Run(ctx => _recoveryHandler.GetStateAsync(ctx));

        [HttpGet("Office/{demandId}/{officeId}")]
        public Task<ActionResult> Office(decimal demandId, decimal officeId) =>
            Run(ctx => _recoveryHandler.GetOfficeAsync(demandId, officeId, ctx));

        [HttpPost("Appropriate")]
        public Task<ActionResult> Appropriate([FromBody] EcsAppropriateRequestVM request) =>
            Run(ctx => _recoveryHandler.AppropriateAsync(request, ctx));

        [HttpPost("DeleteAppropriation")]
        public Task<ActionResult> DeleteAppropriation([FromBody] EcsRecoveryOfficeRequestVM request) =>
            Run(ctx => _recoveryHandler.DeleteAppropriationAsync(request.DemandId, request.OfficeId, ctx));

        [HttpPost("Save")]
        public Task<ActionResult> Save([FromBody] EcsSaveRecoveryRequestVM request) =>
            Run(ctx => _recoveryHandler.SaveAsync(request, ctx));

        [HttpPost("DeleteSaved")]
        public Task<ActionResult> DeleteSaved([FromBody] EcsRecoveryOfficeRequestVM request) =>
            Run(ctx => _recoveryHandler.DeleteSavedAsync(request.DemandId, request.OfficeId, ctx));
    }
}
