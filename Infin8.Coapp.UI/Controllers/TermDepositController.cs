using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

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
        [HttpGet]
        [Route("GetFDNosForRenewal")]
        public async Task<ActionResult<DropdownItem>> GetFDNosForRenewal(decimal memId, string tdSchemeType, DateTime trnDate, string brCode)
        {
            List<DropdownItem> fdNos = new List<DropdownItem>();
            fdNos = await _termDepositTrnHandler.GetTDNosByMemIdForRenewal(memId, tdSchemeType, trnDate, brCode);
            if (fdNos != null) return Ok(fdNos);
            else return NotFound();
        }
        [HttpGet]
        [Route("GetNominee")]
        public async Task<ActionResult<DtoNominee>> GetNominee(decimal memId, string tdSchemeType, string brCode)
        {
            DtoNominee nominee = new DtoNominee();
            nominee = await _termDepositTrnHandler.GetNomineeForTermDeposit(memId, tdSchemeType, brCode);
            if (nominee != null) return Ok(nominee);
            else return NotFound();
        }

        [HttpGet]
        [Route("GetFDData/{tdId:decimal}/{brCode}")]
        public async Task<ActionResult<FDDetailsVM>> GetFDDataByTDId(decimal tdId, string brCode)
        {
            FDDetailsVM fd = new();
            try
            {
                var result = await _termDepositTrnHandler.GetFDDataByTDId(tdId, brCode);
                if (result != null )
                {
                    fd = result;
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return Ok(fd);
        }

        [HttpGet]
        [Route("GetSecurityDepositData/{empId:decimal}/{brCode}")]
        public async Task<ActionResult<DtoSecurityDepositData>> GetSecurityDepositData(decimal empId, string brCode)
        {
            DtoSecurityDepositData sd = new();
            try
            {
                var result = await _termDepositTrnHandler.GetSecurityDepositData(empId, brCode);
                if (result != null)
                {
                    sd = result;
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            return Ok(sd);
        }

        [HttpGet]
        [Route("GetTDRateOfInterestList")]
        public async Task<ActionResult<List<TDRateOfInterstDto>>> GetTDRateOfInterestList ([FromQuery] string[] tdSchemeTypeList)
        {
            List<TDRateOfInterstDto> roiList = new();
            var details = await _termDepositROITemplateHandler.GetTermDepositROITemplateListAsync(tdSchemeTypeList);
            if (details != null && details.Count >0) roiList = details.ToList();
            return Ok(roiList);
        }

        [HttpGet]
        [Route("GetTDRateOfInterestList/{SchemeType}/{brCode}")]
        public async Task<ActionResult<List<TermDeposit_Roi_Template>>> GetTDRateOfInterestList(string SchemeType, string brCode)
        {
            List<TermDeposit_Roi_Template> roiList = new();
            var details = await _termDepositROITemplateHandler.GetTermDepositRateOfInterestList(SchemeType, brCode);
            if (details != null && details.Count > 0) roiList = details.ToList();
            return Ok(roiList);
        }

        [HttpGet]
        [Route("GetTermDepositSchemeList/{brCode}")]
        public async Task<ActionResult<List<TermDeposit_Schemes>>> GetTermDepositSchemeListAsync(string brCode)
        {
            List<TermDeposit_Schemes > schemeList = new();
            var query = await _termDepositSchemeHandler.GetTermDepositSchemeListAsync(brCode);
            if(query != null && query.Count > 0) schemeList = query.ToList();
            return Ok(schemeList);
        }

        [HttpPost]
        [Route("AddTDTemplates")]
        public async Task<ActionResult<List<TermDeposit_Roi_Template>>> AddTDTemplates([FromBody] List<TermDeposit_Roi_Template > templateList)
        {
            List<TermDeposit_Roi_Template> roiList = new();
            var query = await  _termDepositROITemplateHandler.AddTermDepositROITemplateList(templateList);
            if (query != null && query.Count > 0) roiList = query.ToList();
            return Ok(roiList);
        }
    }
}
