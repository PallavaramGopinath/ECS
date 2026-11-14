using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS RD New Account, acc 17 (spec 15). BrCode and business day come from the caller's claims; every endpoint is
    /// gated server-side to ECS societies. Posting happens at approval in TransactionsHandler.SaveTransaction.
    /// </summary>
    [Route("api/rdcreate")]
    [ApiController]
    [Authorize]
    public class RdCreateController : ControllerBase
    {
        const string EcsOnly = "RD New Account is available only to ECS societies.";

        readonly IEcsRdCreateHandler _rdCreateHandler;
        readonly IGeneralHandler _generalHandler;

        public RdCreateController(IEcsRdCreateHandler rdCreateHandler, IGeneralHandler generalHandler)
        {
            _rdCreateHandler = rdCreateHandler;
            _generalHandler = generalHandler;
        }

        /// <summary>Schemes, ROI rows, provisional RD No, first holder and last nominee (§4).</summary>
        [HttpGet]
        [Route("Setup/{memId}")]
        public async Task<ActionResult<EcsRdCreateSetupVM>> Setup(decimal memId)
        {
            var (brCode, day) = Context();
            if (!await IsEcsAsync(brCode))
                return StatusCode(StatusCodes.Status403Forbidden, new EcsRdCreateSetupVM { ApplicationDate = day, Message = EcsOnly });
            try
            {
                return Ok(await _rdCreateHandler.GetSetupAsync(memId, brCode, day));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new EcsRdCreateSetupVM { ApplicationDate = day, Message = ex.Message });
            }
        }

        /// <summary>A joint holder by member number (§4.2).</summary>
        [HttpGet]
        [Route("Member/{memNo}")]
        public async Task<ActionResult<EcsRdHolderLookupVM>> Member(string memNo)
        {
            var (brCode, day) = Context();
            if (!await IsEcsAsync(brCode))
                return StatusCode(StatusCodes.Status403Forbidden, new EcsRdHolderLookupVM { Message = EcsOnly });
            try
            {
                return Ok(await _rdCreateHandler.GetHolderAsync(memNo, brCode, day));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new EcsRdHolderLookupVM { Message = ex.Message });
            }
        }

        /// <summary>Server recomputation and validation, run just before Initiate (§6, §7).</summary>
        [HttpPost]
        [Route("Calculate")]
        public async Task<ActionResult<EcsRdCreateCalcVM>> Calculate([FromBody] DtoEcsRdCreate dto)
        {
            var (brCode, day) = Context();
            if (!await IsEcsAsync(brCode))
                return StatusCode(StatusCodes.Status403Forbidden, new EcsRdCreateCalcVM { Dto = dto, Message = EcsOnly });
            try
            {
                return Ok(await _rdCreateHandler.CalculateAsync(dto, brCode, day));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new EcsRdCreateCalcVM { Dto = dto, Message = ex.Message });
            }
        }

        private (string brCode, DateTime day) Context()
        {
            var u = this.GetUserInfoDto();
            return (u.BrCode ?? "", u.CurrentDate == DateTime.MinValue ? DateTime.Today : u.CurrentDate.Date);
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
