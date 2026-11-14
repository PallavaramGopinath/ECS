using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS Recovery Appropriation (spec 13, 13a). BrCode / user / year / business day come from the caller's claims;
    /// every endpoint is gated server-side to ECS societies and to its own roles (spec 13a §6): the Maker uploads and
    /// appropriates, the Checker saves, rejects and deletes saved recoveries; Admin may do either.
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
                YrEndDate = u.YrEndDate,
                Roles = u.Roles.ToList()
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

        private const string AnyRole = "Admin,Maker,Checker";
        private const string MakerRoles = "Admin,Maker";
        private const string CheckerRoles = "Admin,Checker";
        /// <summary>Largest sheet accepted (a pay office's list is a few hundred rows).</summary>
        private const long MaxSheetBytes = 5 * 1024 * 1024;

        [Authorize(Roles = AnyRole)]
        [HttpGet("State")]
        public Task<ActionResult> State() => Run(ctx => _recoveryHandler.GetStateAsync(ctx));

        [Authorize(Roles = AnyRole)]
        [HttpGet("Office/{demandId}/{officeId}")]
        public Task<ActionResult> Office(decimal demandId, decimal officeId) =>
            Run(ctx => _recoveryHandler.GetOfficeAsync(demandId, officeId, ctx));

        /// <summary>Spec 13a §3: the office's members as a specimen sheet to send to the pay office (?format=xlsx|csv).</summary>
        [Authorize(Roles = MakerRoles)]
        [HttpGet("Specimen/{demandId}/{officeId}")]
        public async Task<ActionResult> Specimen(decimal demandId, decimal officeId, [FromQuery] string? format)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode))
                return StatusCode(StatusCodes.Status403Forbidden,
                    new EcsRecoveryResultVM { Success = false, Message = "Recovery Appropriation is available only to ECS societies." });
            try
            {
                var file = await _recoveryHandler.GetSpecimenAsync(demandId, officeId, format ?? "xlsx", ctx);
                if (file == null) return NotFound();
                return File(file.Content, file.ContentType, file.FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new EcsRecoveryResultVM { Success = false, Message = ex.Message });
            }
        }

        [Authorize(Roles = MakerRoles)]
        [HttpPost("Upload/{demandId}/{officeId}")]
        [RequestSizeLimit(MaxSheetBytes + 64 * 1024)]
        public async Task<ActionResult> Upload(decimal demandId, decimal officeId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new EcsRecoveryResultVM { Success = false, Message = "Choose the pay office's Excel file." });
            if (file.Length > MaxSheetBytes)
                return BadRequest(new EcsRecoveryResultVM { Success = false, Message = "The file is larger than 5 MB." });
            // The reader needs a seekable stream (.xlsx is a zip).
            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer);
            buffer.Position = 0;
            return await Run(ctx => _recoveryHandler.UploadAsync(demandId, officeId, buffer, file.FileName, ctx));
        }

        [Authorize(Roles = MakerRoles)]
        [HttpPost("Appropriate")]
        public Task<ActionResult> Appropriate([FromBody] EcsAppropriateRequestVM request) =>
            Run(ctx => _recoveryHandler.AppropriateAsync(request, ctx));

        [Authorize(Roles = MakerRoles)]
        [HttpPost("DeleteAppropriation")]
        public Task<ActionResult> DeleteAppropriation([FromBody] EcsRecoveryOfficeRequestVM request) =>
            Run(ctx => _recoveryHandler.DeleteAppropriationAsync(request.DemandId, request.OfficeId, ctx));

        [Authorize(Roles = CheckerRoles)]
        [HttpPost("Reject")]
        public Task<ActionResult> Reject([FromBody] EcsRejectRequestVM request) =>
            Run(ctx => _recoveryHandler.RejectAsync(request, ctx));

        [Authorize(Roles = CheckerRoles)]
        [HttpPost("Save")]
        public Task<ActionResult> Save([FromBody] EcsSaveRecoveryRequestVM request) =>
            Run(ctx => _recoveryHandler.SaveAsync(request, ctx));

        [Authorize(Roles = CheckerRoles)]
        [HttpPost("DeleteSaved")]
        public Task<ActionResult> DeleteSaved([FromBody] EcsRecoveryOfficeRequestVM request) =>
            Run(ctx => _recoveryHandler.DeleteSavedAsync(request.DemandId, request.OfficeId, ctx));

        // ----- recovery refund (spec 13 §16) -----

        [Authorize(Roles = CheckerRoles)]
        [HttpGet("Refund/{demandId}")]
        public Task<ActionResult> Refund(decimal demandId, [FromQuery] string memberNo) =>
            Run(ctx => _recoveryHandler.GetRefundAsync(demandId, memberNo ?? "", ctx));

        [Authorize(Roles = CheckerRoles)]
        [HttpPost("Refund")]
        public Task<ActionResult> SaveRefund([FromBody] EcsRefundRequestVM request) =>
            Run(ctx => _recoveryHandler.RefundAsync(request, ctx));
    }
}
