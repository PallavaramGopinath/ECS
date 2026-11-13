using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Infin8.Coapp.Utility.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    /// <summary>
    /// ECS Loan Sanction (spec 06 §7.3). Every write endpoint takes BrCode / user / year / business day
    /// from the caller's claims and is gated server-side to ECS societies.
    /// </summary>
    [Route("api/loansanction")]
    [ApiController]
    [Authorize]
    public class LoanSanctionController : ControllerBase
    {
        private const string OfficeNoteReportName = "Office Note";

        readonly IEcsLoanSanctionHandler _sanctionHandler;
        readonly IFinLedgerHandler _finLedgerHandler;
        readonly IGeneralHandler _generalHandler;
        readonly IReportsHandler _reportsHandler;
        readonly IWebHostEnvironment _webHostEnvironment;

        public LoanSanctionController(IEcsLoanSanctionHandler sanctionHandler, IFinLedgerHandler finLedgerHandler,
            IGeneralHandler generalHandler, IReportsHandler reportsHandler, IWebHostEnvironment webHostEnvironment)
        {
            _sanctionHandler = sanctionHandler;
            _finLedgerHandler = finLedgerHandler;
            _generalHandler = generalHandler;
            _reportsHandler = reportsHandler;
            _webHostEnvironment = webHostEnvironment;
        }

        // ----- context helpers -----

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

        /// <summary>The business day from the caller's claims.</summary>
        private DateTime BusinessDay() => UserContext().CurrentDate;

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

        private ObjectResult NotEcs() =>
            StatusCode(StatusCodes.Status403Forbidden,
                new SanctionSaveResultVM { Success = false, Message = "ECS Loan Sanction is available only to ECS societies." });

        /// <summary>
        /// Logs an unexpected write failure and returns its message as JSON, so the page shows the cause
        /// instead of a bare "Request failed (500)".
        /// </summary>
        private ObjectResult ServerError(Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new SanctionSaveResultVM { Success = false, Message = ex.Message });
        }

        // ----- lookups -----

        /// <summary>
        /// Schemes with Loan_Type 1 (Thrift &amp; Credit loans; surety, education, draught, etc. differ by
        /// Scheme_Id) that the ECS policy allows.
        /// </summary>
        [HttpGet]
        [Route("GetEcsLoanSchemes/{brCode}")]
        [Route("GetSuretyLoanSchemes/{brCode}")] // alias kept for the old page URL
        public async Task<ActionResult<List<DropdownItem>>> GetEcsLoanSchemes(string brCode)
        {
            if (!await IsEcsAsync(brCode)) return NotEcs();
            return Ok(await _sanctionHandler.GetEcsSchemesAsync(brCode));
        }

        [HttpGet]
        [Route("GetSuretyLoanMember/{memberNo}/{brCode}")]
        public async Task<ActionResult<SuretyLoanSanctionDataVM>> GetSuretyLoanMember(string memberNo, string brCode)
            => Ok(await _sanctionHandler.GetMemberAsync(memberNo, BusinessDay(), brCode));

        /// <summary>Records a missing date of birth in mem_master; returns the member with service recomputed.</summary>
        [HttpPost]
        [Route("UpdateMemberDob")]
        public async Task<ActionResult<SuretyLoanSanctionDataVM>> UpdateMemberDob([FromBody] EcsMemberDobVM vm)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode)) return NotEcs();
            return Ok(await _sanctionHandler.UpdateMemberDobAsync(vm, ctx.CurrentDate, ctx.BrCode));
        }

        /// <summary>Optional "Fetch from payroll" prefill of the pay details.</summary>
        [HttpGet]
        [Route("GetPayDetails/{memId:decimal}/{brCode}")]
        public async Task<ActionResult<EcsPayDetailsVM>> GetPayDetails(decimal memId, string brCode)
            => Ok(await _sanctionHandler.GetPayDetailsAsync(memId, brCode));

        [HttpGet]
        [Route("GetSchemeDefaults/{schemeId:int}/{memId:decimal}/{excludeSanctionId:decimal}/{brCode}")]
        public async Task<ActionResult<SuretyLoanSchemeDefaultsVM>> GetSchemeDefaults(int schemeId, decimal memId,
            decimal excludeSanctionId, string brCode)
            => Ok(await _sanctionHandler.GetSchemeDefaultsAsync(schemeId, memId, excludeSanctionId, BusinessDay(), brCode));

        [HttpGet]
        [Route("GetSuretyDetails/{schemeId:int}/{suretyMemId:decimal}/{brCode}")]
        public async Task<ActionResult<EcsSuretyDetailsVM>> GetSuretyDetails(int schemeId, decimal suretyMemId, string brCode)
            => Ok(await _sanctionHandler.GetSuretyDetailsAsync(schemeId, suretyMemId, brCode));

        /// <summary>Ledger dropdown for manual deductions — excludes cash and bank ledgers.</summary>
        [HttpGet]
        [Route("GetLedgers/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetLedgers(string brCode)
        {
            var items = await _finLedgerHandler.GetLedgerItemsExceptBankLedgersAsyn(brCode);
            return Ok(items ?? new List<DropdownItem>());
        }

        [HttpPost]
        [Route("GetRecoverables")]
        public async Task<ActionResult<EcsRecoverablesVM>> GetRecoverables([FromBody] EcsRecoverablesRequestVM req)
        {
            var ctx = UserContext();
            return Ok(await _sanctionHandler.GetRecoverablesAsync(req, ctx.CurrentDate, ctx.BrCode));
        }

        [HttpGet]
        [Route("ResolveDeductionStatus/{ledId:decimal}/{brCode}")]
        public async Task<ActionResult<DeductableLoanRowVM>> ResolveDeductionStatus(decimal ledId, string brCode)
            => Ok(await _sanctionHandler.ResolveDeductionAsync(ledId, brCode));

        [HttpPost]
        [Route("CalculateEligibility")]
        public async Task<ActionResult<LoanEligibilityResultVM>> CalculateEligibility([FromBody] LoanEligibilityInputVM input)
            => Ok(await _sanctionHandler.CalculateEligibilityAsync(input));

        // ----- writes (claims context, ECS gate) -----

        /// <summary>Create (LoanSanction_Id = 0) or modify a sanction.</summary>
        [HttpPost]
        [Route("SaveSanction")]
        public async Task<ActionResult<SanctionSaveResultVM>> SaveSanction([FromBody] SuretyLoanSanctionSaveVM vm)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode)) return NotEcs();
            try
            {
                var result = await _sanctionHandler.SaveAsync(vm, ctx);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        [HttpPost]
        [Route("DeleteSanction/{sanctionId:decimal}")]
        public async Task<ActionResult<SanctionSaveResultVM>> DeleteSanction(decimal sanctionId)
        {
            var ctx = UserContext();
            if (!await IsEcsAsync(ctx.BrCode)) return NotEcs();
            try
            {
                var result = await _sanctionHandler.DeleteAsync(sanctionId, ctx);
                return result.Success ? Ok(result) : BadRequest(result);
            }
            catch (Exception ex)
            {
                return ServerError(ex);
            }
        }

        // ----- list / load / print -----

        [HttpPost]
        [Route("ListSanctions")]
        public async Task<ActionResult<List<EcsSanctionListItemVM>>> ListSanctions([FromBody] EcsSanctionListFilterVM filter)
            => Ok(await _sanctionHandler.ListAsync(filter, UserContext().BrCode));

        [HttpGet]
        [Route("GetSanction/{sanctionId:decimal}/{brCode}")]
        public async Task<ActionResult<EcsSanctionDetailVM>> GetSanction(decimal sanctionId, string brCode)
        {
            var detail = await _sanctionHandler.GetAsync(sanctionId, brCode);
            return detail == null ? NotFound() : Ok(detail);
        }

        /// <summary>Office Note PDF (spec 06 §9), returned as bytes (base64 in JSON) like the other report endpoints.</summary>
        [HttpGet]
        [Route("OfficeNote/{sanctionId:decimal}")]
        public async Task<ActionResult<byte[]>> OfficeNote(decimal sanctionId)
        {
            var brCode = UserContext().BrCode;

            Infin8.Coapp.Models.Reports_Master report;
            try
            {
                report = await _reportsHandler.GetReportNameWithSignature(OfficeNoteReportName);
            }
            catch
            {
                return NotFound($"No '{OfficeNoteReportName}' row in reports_master. Add it to enable printing.");
            }

            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Reports", report.ReportFileName ?? "OfficeNote.rdlc");
            if (!System.IO.File.Exists(path)) return NotFound($"Report file {report.ReportFileName} not found.");

            var parameters = new Dictionary<string, string>
            {
                { "paramSocietyName", await _generalHandler.GetSocietyName(brCode) ?? "" },
                { "ParamFirstSignature", report.FirstSignature ?? "" },
                { "ParamSecondSignature", report.SecondSignature ?? "" },
                { "ParamThirdSignature", report.ThirdSignature ?? "" },
            };

            byte[]? pdf;
            using (var stream = System.IO.File.OpenRead(path))
                pdf = await _sanctionHandler.GetOfficeNotePdfAsync(sanctionId, brCode, stream, parameters);
            return pdf == null ? NotFound("Sanction not found.") : Ok(pdf);
        }
    }
}
