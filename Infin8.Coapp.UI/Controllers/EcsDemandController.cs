using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS Demand Calculation (spec 12). BrCode / user / year / business day come from the caller's claims;
    /// every endpoint is gated server-side to ECS societies.
    /// </summary>
    [Route("api/ecsdemand")]
    [ApiController]
    [Authorize]
    public class EcsDemandController : ControllerBase
    {
        readonly IEcsDemandHandler _demandHandler;
        readonly IGeneralHandler _generalHandler;

        public EcsDemandController(IEcsDemandHandler demandHandler, IGeneralHandler generalHandler)
        {
            _demandHandler = demandHandler;
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

        private ObjectResult NotEcs() =>
            StatusCode(StatusCodes.Status403Forbidden,
                new EcsDemandResultVM { Success = false, Message = "ECS Demand Calculation is available only to ECS societies." });

        private ObjectResult ServerError(Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new EcsDemandResultVM { Success = false, Message = ex.Message });
        }

        private async Task<ActionResult> Run<T>(Func<EcsUserContextVM, Task<T>> action)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode)) return NotEcs();
            try
            {
                var result = await action(ctx);
                if (result is EcsDemandResultVM { Success: false } failed) return BadRequest(failed);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        [HttpGet("State")]
        public Task<ActionResult> State() => Run(ctx => _demandHandler.GetStateAsync(ctx));

        [HttpPost("Preview")]
        public Task<ActionResult> Preview([FromBody] EcsDemandRequestVM request) =>
            Run(ctx => _demandHandler.PreviewAsync(request.DueDate, ctx));

        [HttpGet("Lines/{demandId}")]
        public async Task<ActionResult> Lines(decimal demandId)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode)) return NotEcs();
            try
            {
                var summary = await _demandHandler.GetSavedSummaryAsync(demandId, ctx.BrCode);
                return summary == null ? NotFound() : Ok(summary);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        [HttpPost("Save")]
        public Task<ActionResult> Save([FromBody] EcsDemandRequestVM request) =>
            Run(ctx => _demandHandler.SaveCalculationAsync(request.DueDate, ctx));

        [HttpPost("Post/{demandId}")]
        public Task<ActionResult> Post(decimal demandId) => Run(ctx => _demandHandler.PostAsync(demandId, ctx));

        [HttpPost("DeleteCalculation/{demandId}")]
        public Task<ActionResult> DeleteCalculation(decimal demandId) =>
            Run(ctx => _demandHandler.DeleteCalculationAsync(demandId, ctx));

        // ----- stop demand -----

        [HttpGet("StopList")]
        public Task<ActionResult> StopList([FromQuery] DateTime dueDate) =>
            Run(ctx => _demandHandler.GetStopListAsync(dueDate, ctx.BrCode));

        [HttpPost("Stop")]
        public Task<ActionResult> Stop([FromBody] EcsStopDemandRequestVM request) =>
            Run(ctx => _demandHandler.AddStopAsync(request, ctx));

        [HttpPost("Unstop/{stopId}")]
        public Task<ActionResult> Unstop(decimal stopId) => Run(ctx => _demandHandler.RemoveStopAsync(stopId, ctx));

        [HttpPost("DeletePosted/{demandId}")]
        public Task<ActionResult> DeletePosted(decimal demandId) =>
            Run(ctx => _demandHandler.DeletePostedAsync(demandId, ctx));
    }
}
