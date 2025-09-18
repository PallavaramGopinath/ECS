using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/loan")]
    [ApiController]
    //[Authorize]
    public class LoanController : ControllerBase
    {
        readonly ILoanSchemeHandler _loanSchemeHandler;
        readonly ILoanTrnHandler _loanTrnHandler;
        readonly IJLDetailsHandler _jlDetailsHandler;
        readonly IJLEligibleHandler _jleligibleHandler;
        readonly IJLMaximimumLimitHandler _jlelMaximimumLimitHandler;
        readonly ILoanROITemplateHandler _loanROITemplateHandler;

        public LoanController(ILoanSchemeHandler loanSchemeHandler, ILoanTrnHandler loanTrnHandler,
            IJLDetailsHandler jLDetailsHandler, IJLEligibleHandler jLEligibleHandler,
            IJLMaximimumLimitHandler jLMaximimumLimitHandler, ILoanROITemplateHandler loanROITemplateHandler)
        {
            _loanSchemeHandler = loanSchemeHandler;
            _loanTrnHandler = loanTrnHandler;
            _jlDetailsHandler = jLDetailsHandler;
            _jleligibleHandler = jLEligibleHandler;
            _jlelMaximimumLimitHandler = jLMaximimumLimitHandler;
            _loanROITemplateHandler = loanROITemplateHandler;
        }
        //[Authorize(Roles = $"{RoleConstants.Admin},{RoleConstants.Maker},{RoleConstants.Checker},{RoleConstants.Configurator}")] // to authorize two roles, use $"{RoleConstants.Admin},{RoleConstants.Maker}"
        //[Authorize(Roles = RoleConstants.Configurator)]
        [HttpPost]
        public async Task<ActionResult<Loan_Schemes>> AddScheme(Loan_Schemes loanscheme)
        {
            var result = await _loanSchemeHandler.AddLoanSchemeAsync(loanscheme);
            if (result) return Ok(result);
            else return NotFound();
        }

        #region Loan Scheme
        [HttpGet]
        [Route("GetLoanSchemes/{loanType:int}")]
        public async Task<ActionResult<List<DropdownItem>>> GetLoanSchemeItems(int loanType)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = await _loanSchemeHandler.GetLoanSchemeItemsAsync(loanType);
            if (items.Count > 0) return Ok(items);
            else
                return NotFound();
        }

        [HttpGet]
        [Route("GetLoanScheme/{scheme_id:int}")]
        public async Task<ActionResult<Loan_Schemes>> GetLoanScheme(int scheme_id)
        {
            Loan_Schemes scheme = new Loan_Schemes();
            try
            {
                scheme = await _loanSchemeHandler.GetLoanSchemesAsync(scheme_id);
            }
            catch (Exception)
            {
                return NotFound();
            }
            return scheme;
        }

        [HttpGet]
        [Route("GetLoanSchemeList/{loanType:int}/{brCode}")]
        public async Task<ActionResult<List<Loan_Schemes>>> GetLoanSchemeList(int loanType, string brCode)
        {
            List<Loan_Schemes> schemeList = new List<Loan_Schemes>();
            try
            {
                var result = await _loanSchemeHandler.GetLoanSchemeListByType(loanType, brCode);
                if (result != null && result.Any())
                {
                    schemeList = result.ToList();
                    return Ok(schemeList);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        #endregion

        #region Jewel Loan
        [HttpGet]
        [Route("GetJLNosByMemId/{memId:decimal}/{loantype:int}")]
        public async Task<ActionResult<List<DropdownItem>>> GetJewelLoanNosByMemId(decimal memId, int loanType)
        {
            List<DropdownItem> loanNoList = new List<DropdownItem>();
            var result = await _loanTrnHandler.GetLoanNosAsync(memId, loanType);
            if (result.Count > 0) loanNoList = result.ToList();
            return Ok(loanNoList);
        }
        [HttpGet]
        [Route("GetJLBalance")]
        public async Task<ActionResult<List<JewelLoanBalance>>> GetJewelLoanBalance([FromQuery] decimal[] loanIdList, [FromQuery] DateTime endDate, [FromQuery] string brCode)
        {
            List<JewelLoanBalance>? jlBalanceList = new List<JewelLoanBalance>();
            var result = await _loanTrnHandler.GetJewelLoanNoBalanceAsync(loanIdList, endDate, brCode);
            if (result != null && result.Count > 0)
            {
                jlBalanceList = result.ToList();
                return Ok(jlBalanceList);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("GetMarketRate")]
        public async Task<ActionResult<JewelLoanMarketRate>> GetJewelLoanMarketRate()
        {
            try
            {
                var rate = await _jlDetailsHandler.GetMarketRateAndAdoptedRateAsync();
                return Ok(rate);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetJLEligiblePercentage")]
        public async Task<ActionResult<double>> GetJewelLoanEligiblePercentage()
        {
            double eligiblePercentage = 0;
            try
            {
                eligiblePercentage = await _jleligibleHandler.GetJLEligiblePercentageAsync();
            }
            catch (Exception)
            {
                return NotFound();
            }
            return Ok(eligiblePercentage);
        }

        [HttpGet]
        [Route("GetJLMaximumLimit")]
        public async Task<ActionResult<double>> GetJewelLoanMaximumLimit([FromQuery] DateTime wef)
        {
            double maxLimit = 0;
            try
            {
                maxLimit = await _jlelMaximimumLimitHandler.GetJLMaximumLimitAsync(wef);
            }
            catch (Exception)
            {
                return NotFound();
            }
            return Ok(maxLimit);
        }


        [HttpGet]
        [Route("GetJLPeriod/{schemeId:int}")]
        public async Task<ActionResult<int>> GetJewelLoanPeriodOfLoan(int schemeId)
        {
            int period = 0;
            try
            {
                period = await _loanSchemeHandler.GetPeriodOfLoan(schemeId);
            }
            catch (Exception)
            {
                return NotFound();
            }
            return Ok(period);
        }

        [HttpGet]
        [Route("GetJLExistingLoanOutstandingByMemId/{memId:decimal}")]
        public async Task<ActionResult<double>> GetJewelLoanExistingBalanceByMemId(decimal memId)
        {
            double loanBalance = 0;
            try
            {
                loanBalance = await _loanTrnHandler.GetJLExistingLoanOutstandingAsync(memId);
            }
            catch (Exception)
            {
                return NotFound();
            }
            return Ok(loanBalance);
        }
        #endregion

        #region Rate of Interest
        [HttpGet]
        [Route("GetRateOfInterest")]
        public async Task<ActionResult<LoanROIAndPIVM>> GetLoanRateOfInterest([FromQuery] int schemeId, [FromQuery] string agency, [FromQuery] DateTime wef)
        {
            LoanROIAndPIVM roi = new LoanROIAndPIVM();
            try
            {
                roi = await _loanROITemplateHandler.GetLoanROIAndPIFromTemplateAsync(schemeId, agency, wef);
            }
            catch (Exception)
            {
                return NotFound();
            }
            return Ok(roi);
        }

        [HttpGet]
        [Route("GetRateOfInterest/{schemeId:int}/{agency}/{wef}/{brCode}")]
        public async Task<ActionResult<LoanROIAndPIVM>> GetLoanRateOfInterest(int schemeId, string agency, string wef, string brCode)
        {
            LoanROIAndPIVM roi = new LoanROIAndPIVM();
            try
            {
                DateTime.TryParse(wef, out DateTime wefFormated);
                roi = await _loanROITemplateHandler.GetLoanROIAndPIFromTemplateAsync(schemeId, agency, wefFormated, brCode);
            }
            catch (Exception)
            {
                return NotFound();
            }
            return Ok(roi);
        }
        #endregion

        #region Term Deposit Loans
        [HttpGet]
        [Route("GetLoanOnTD")]
        public async Task<ActionResult<List<LoanDetailsVM>>> GetTDLoanDetailsByTDIdsAsync([FromQuery] decimal[] TDNos, [FromQuery] DateTime toDate)
        {
            List<LoanDetailsVM> loanDetails = new List<LoanDetailsVM>();
            try
            {
                loanDetails = await _loanTrnHandler.GetTDLoanDetailsByTDIdsAsync(TDNos, toDate);
                return Ok(loanDetails);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetTDLoanBalance")]
        public async Task<ActionResult<List<TDLoanData>>> GetTDLoanBalance([FromQuery] decimal[] tdIds)
        {
            List<TDLoanData> loanList = new List<TDLoanData>();
            try
            {
                loanList = await _loanTrnHandler.GetTDLoanDetailsByTDIds(tdIds);
                return Ok(loanList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetTDLoanData")] ///this is final
        public async Task<ActionResult<List<DtoTermDepositLoan>>> GetTDLoanData([FromQuery] List<decimal> tdIds, [FromQuery] DateTime toDate)
        {
            List<DtoTermDepositLoan> loanList = new List<DtoTermDepositLoan>();
            try
            {
                loanList = await _loanTrnHandler.GetTDLoanDataByTDIds(tdIds, toDate);
                return Ok(loanList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        [HttpGet]
        [Route("GetTDLoanBalanceByLoanId")]
        public async Task<ActionResult<List<DtoTermDepositLoanBalance>>> GetFDLoanBalanceByLoanId([FromQuery] List<decimal> loanIdList, [FromQuery] DateTime toDate, [FromQuery] string brCode)
        {
            List<DtoTermDepositLoanBalance> LoanList = new List<DtoTermDepositLoanBalance>();
            try
            {
                LoanList = await _loanTrnHandler.GetTDLoanBalanceByTDIds(loanIdList, toDate, brCode);
                return Ok(LoanList);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }
        #endregion

        #region Salary Loan
        [HttpGet]
        [Route("GetPayLoanBalance/{empId:decimal}/{loanType:int}/{toDate}/{brCode}")]
        public async Task<ActionResult<List<PayLoanBalanceVM>>> GetPayLoanBalance(decimal empId, int loanType, string toDate, string brCode)
        {
            List<PayLoanBalanceVM> loanBalance = new List<PayLoanBalanceVM>();
            try
            {
                DateTime.TryParse(toDate, out DateTime toDateFormated);
                var result = await _loanTrnHandler.GetPayLoanBalance(empId, loanType, toDateFormated, brCode);
                if (result != null && result.Any())
                {
                    loanBalance = result.ToList();
                    return Ok(loanBalance);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
                Console.Write(ex.Message);
            }
        }

        [HttpGet]
        [Route("GetPayLoanBalance")]
        public async Task<ActionResult<List<PayLoanBalanceVM>>> GetPayLoanBalance([FromQuery] decimal[] loanIdList, [FromQuery] DateTime endDate, [FromQuery] string brCode)
        {
            List<PayLoanBalanceVM>? jlBalanceList = new List<PayLoanBalanceVM>();
            var result = await _loanTrnHandler.GetPayLoanBalance(loanIdList, endDate, brCode);
            if (result != null && result.Count > 0)
            {
                jlBalanceList = result.ToList();
                return Ok(jlBalanceList);
            }
            else
            {
                return NotFound();
            }

        }

        [HttpGet]
        [Route("GetStaffLoanDisbursementData/{vocId:decimal}/{brCode}")]
        public async Task<ActionResult <DtoLoanDisbursementStaff>> GetStaffLoanDisbursementData(decimal vocId, string brCode)
        {
            DtoLoanDisbursementStaff staffLoan = new();
            var result = await _loanTrnHandler.GetStaffLoanDisbursement(vocId, brCode);
            if (result != null && result.Scheme_Id  > 0)
            {
                staffLoan = result;
                return Ok(staffLoan);
            }
            else
            {
                return NotFound();
            }
        }
        #endregion
    }
}

