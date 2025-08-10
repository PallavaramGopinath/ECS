using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsDashboardController : ControllerBase
    {
        //private readonly IWebHostEnvironment _environment;
        readonly IFinVoucherTrnHandler _finVoucherTrnHandler;
        public AccountsDashboardController( IFinVoucherTrnHandler finVoucherTrnHandler)
        {
            _finVoucherTrnHandler = finVoucherTrnHandler;
        }
        [HttpGet]
        [Route("GetTransactionByVocId/{vocId:decimal}/{brCode}")]
        public async Task<ActionResult<DtoVoucher>> GetTransactionByVocId(decimal vocId, string brCode)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionById(vocId, brCode);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }

        [HttpGet]
        [Route("GetTransactionByNo")]
        public async Task<ActionResult<DtoVoucher>> GetTransactionByNo([FromQuery] string rptNo,[FromQuery] string pmtNo, [FromQuery] decimal yrId)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionByNo(rptNo,pmtNo, yrId);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }

        [HttpGet]
        [Route("GetTransactionByRptNo")]
        public async Task<ActionResult<DtoVoucher>> GetTransactionByRptNo([FromQuery] string rptNo,  [FromQuery] decimal yrId)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionByNo(rptNo, string.Empty , yrId);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }
        [HttpGet]
        [Route("GetTransactionByPmtNo")]
        public async Task<ActionResult<DtoVoucher>> GetTransactionByPmtNo([FromQuery] string pmtNo, [FromQuery] decimal yrId)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionByNo(string.Empty , pmtNo, yrId);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }
    }
}
