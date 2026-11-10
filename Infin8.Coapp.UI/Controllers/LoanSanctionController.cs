using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.UI.Controllers; // ControllerExtensions.GetUserInfoDto
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/loansanction")]
    [ApiController]
    //[Authorize]
    public class LoanSanctionController : ControllerBase
    {
        // Loan-purpose schemes for the Surety Loan screen are configured under Loan_Type = 1.
        private const int SuretyLoanSchemeType = 1;

        readonly ISuretyLoanSanctionHandler _suretyLoanSanctionHandler;
        readonly ILoanSchemeHandler _loanSchemeHandler;
        readonly IFinLedgerHandler _finLedgerHandler;
        readonly ILoanEligibilityHandler _loanEligibilityHandler;

        public LoanSanctionController(ISuretyLoanSanctionHandler suretyLoanSanctionHandler,
            ILoanSchemeHandler loanSchemeHandler, IFinLedgerHandler finLedgerHandler,
            ILoanEligibilityHandler loanEligibilityHandler)
        {
            _suretyLoanSanctionHandler = suretyLoanSanctionHandler;
            _loanSchemeHandler = loanSchemeHandler;
            _finLedgerHandler = finLedgerHandler;
            _loanEligibilityHandler = loanEligibilityHandler;
        }

        /// <summary>Loan-purpose dropdown for Surety Loans (Spec 03 Sec 5).</summary>
        [HttpGet]
        [Route("GetSuretyLoanSchemes/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetSuretyLoanSchemes(string brCode)
        {
            var items = await _loanSchemeHandler.GetLoanSchemeItemsAsync(SuretyLoanSchemeType, brCode);
            return Ok(items ?? new List<DropdownItem>());
        }

        /// <summary>Member + surety + salary + dates + assets assembled on member lookup (Spec 03 Sec 2-7).</summary>
        [HttpGet]
        [Route("GetSuretyLoanMember/{memberNo}/{brCode}")]
        public async Task<ActionResult<SuretyLoanSanctionDataVM>> GetSuretyLoanMember(string memberNo, string brCode)
        {
            var data = await _suretyLoanSanctionHandler.GetSuretyLoanMemberAsync(memberNo, brCode);
            return Ok(data);
        }

        /// <summary>Scheme-driven defaults when a loan purpose is selected (Spec 03 Sec 5).</summary>
        [HttpGet]
        [Route("GetSchemeDefaults/{schemeId:int}/{brCode}")]
        public async Task<ActionResult<SuretyLoanSchemeDefaultsVM>> GetSchemeDefaults(int schemeId, string brCode)
        {
            var defaults = await _suretyLoanSanctionHandler.GetSchemeDefaultsAsync(schemeId, brCode);
            return Ok(defaults);
        }

        /// <summary>Ledger dropdown for manual deductions — excludes cash + bank ledgers (Spec 03 Sec 9.1).</summary>
        [HttpGet]
        [Route("GetLedgers/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetLedgers(string brCode)
        {
            var items = await _finLedgerHandler.GetLedgerItemsExceptBankLedgersAsyn(brCode);
            return Ok(items ?? new List<DropdownItem>());
        }

        /// <summary>Deductable loans, share-capital + sundry rows, and verification data (Spec 03 Sec 9/10.1/11).</summary>
        [HttpGet]
        [Route("GetDeductions/{memId:decimal}/{suretyMemId:decimal}/{schemeId:int}/{loanAmount:double}/{brCode}")]
        public async Task<ActionResult<SuretyDeductionsDataVM>> GetDeductions(decimal memId, decimal suretyMemId,
            int schemeId, double loanAmount, string brCode)
        {
            var data = await _suretyLoanSanctionHandler.GetDeductionsDataAsync(memId, suretyMemId, schemeId, loanAmount, brCode);
            return Ok(data);
        }

        /// <summary>Loan eligibility pre-check (Spec 03 Sec 10.2/10.3) — compute only, no persistence.</summary>
        [HttpPost]
        [Route("CalculateEligibility")]
        public async Task<ActionResult<LoanEligibilityResultVM>> CalculateEligibility([FromBody] LoanEligibilityInputVM input)
        {
            var result = await _loanEligibilityHandler.CalculateAsync(input);
            return Ok(result);
        }

        /// <summary>Saves a surety loan sanction (Spec 03 Sec 13) with the Sec 12 integrity guards.</summary>
        [HttpPost]
        [Route("SaveSanction")]
        public async Task<ActionResult<SanctionSaveResultVM>> SaveSanction([FromBody] SuretyLoanSanctionSaveVM vm)
        {
            // Audit context is taken from the authenticated user's claims — never trusted from the client.
            var user = this.GetUserInfoDto();
            var brCode = user.BrCode ?? "";
            var sanctionDate = user.CurrentDate == default ? DateTime.Today : user.CurrentDate;

            var result = await _suretyLoanSanctionHandler.SaveSanctionAsync(vm, user.UserId, user.YrId, brCode, sanctionDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
