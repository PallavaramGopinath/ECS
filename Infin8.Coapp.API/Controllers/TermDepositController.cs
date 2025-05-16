using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TermDepositController : ControllerBase
    {
        readonly ITermDepositMasterHandler _termDepositMasterHandler;
        readonly ITermDepositSchemeHandler _termDepositSchemeHandler;
        readonly ITermDepositROITemplateHandler _termDepositROITemplateHandler;
        readonly ITermDepositTrnHandler _termDepositTrnHandler;
        readonly ITermDepositLoanEligibleTemplateHandler _termDepositLoanEligibleTemplateHandler;
        public TermDepositController(ITermDepositSchemeHandler termDepositSchemeHandler, 
            ITermDepositROITemplateHandler  termDepositROITemplateHandler,
            ITermDepositTrnHandler termDepositTrnHandler,
            ITermDepositMasterHandler termDepositMasterHandler,
            ITermDepositLoanEligibleTemplateHandler termDepositLoanEligibleTemplateHandler )
        {
            _termDepositSchemeHandler = termDepositSchemeHandler;
            _termDepositROITemplateHandler = termDepositROITemplateHandler;
            _termDepositTrnHandler = termDepositTrnHandler;
            _termDepositMasterHandler = termDepositMasterHandler;
            _termDepositLoanEligibleTemplateHandler = termDepositLoanEligibleTemplateHandler; 
        }
        [HttpGet]
        [Route("GetTDScheme/{schemeTypes}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetTermDepositSchemeList(string schemeTypes, string brCode)
        {
            List<DropdownItem> schemeList = new List<DropdownItem>();
            string[] schemeType = schemeTypes.Split(',');
            var schList = await  _termDepositSchemeHandler.GetTermDepositSchemeListBySchemeTypeArrayAsync(schemeType, "11001");
            if(schList != null) {schemeList = schList.ToList();}
            return Ok(schemeList);
        }
        [HttpGet]
        [Route("GetTDROI")]
        public async Task<ActionResult<double>> GetTDROI([FromQuery] DateTime depositDate, [FromQuery] int  schemeId, [FromQuery] int prdInMonths, [FromQuery] int prdInDays, string brCode)
        {
            double roi = 0;
            roi = await _termDepositROITemplateHandler.GetROIForTermDepositAsync(depositDate, schemeId, prdInMonths, prdInDays, brCode);
            return Ok(roi);
        }

        [HttpGet]
        [Route("GetFDNosByMemId/{memId:decimal}/{tdSchemeType}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetFDNosByMemId(decimal memId, string tdSchemeType,string brCode)
        {
            List<DropdownItem> fdNosList = new List<DropdownItem>();
            var fdNos = await _termDepositTrnHandler.GetTDNosByMemIdAsync(memId, tdSchemeType, brCode);
            if(fdNos != null) fdNosList = fdNos.ToList();
            return Ok(fdNosList);
        }
        [HttpGet]
        [Route("GetFDPayable")]
        public async Task<ActionResult<List<FDDetailsVM>>> GetFDPayable([FromQuery] decimal[] fdNos, [FromQuery] DateTime toDate, [FromQuery] int accountId,  [FromQuery] string brCode)
        {
            List<FDDetailsVM> fdDetails = new List<FDDetailsVM>();
            var details = await _termDepositTrnHandler.GetFDPayableByTDIdsAsync(fdNos, toDate, accountId,  brCode);
            if(details != null) fdDetails = details.ToList();
            return Ok(fdDetails);
        }
        [HttpGet]
        [Route("GetFDDetailsForLoan")]
        public async Task<ActionResult<List<FDDataForLoan>>> GetFDDetailsForLoan([FromQuery] decimal[] fdIds)
        {
            List<FDDataForLoan> fdDetails = new List<FDDataForLoan>();
            var deatils = await _termDepositMasterHandler.GetFDDetailsForLoan(fdIds);
            if (deatils != null) fdDetails = deatils.ToList();
            return Ok(fdDetails);
        }
        [HttpGet]
        [Route("GetTDLoanEligiblePercentage/{tdSchemeType:int}/{brCode}")]
        public async Task<ActionResult<double>> GetTDLoanEligiblePercentage(int tdSchemeType, string brCode)
        {
            double percentage = 0;
            percentage = await _termDepositLoanEligibleTemplateHandler.GetTDLoanEligiblePercentageAsync(tdSchemeType, brCode);
            return Ok(percentage);
        }
    }
}
