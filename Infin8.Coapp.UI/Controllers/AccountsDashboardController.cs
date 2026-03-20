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
        readonly IFinLedgerTrnHandler _finLedgerTrnHandler;
        readonly IFinLedgerGroupHandler _finLedgerGroupHandler ;
        public AccountsDashboardController( IFinVoucherTrnHandler finVoucherTrnHandler,
            IFinLedgerTrnHandler  finLedgerTrnHandler,IFinLedgerGroupHandler finLedgerGroupHandler  )
        {
            _finVoucherTrnHandler = finVoucherTrnHandler;
            _finLedgerTrnHandler = finLedgerTrnHandler;
            _finLedgerGroupHandler = finLedgerGroupHandler;
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
        public async Task<ActionResult<DtoVoucher>> GetTransactionByNo([FromQuery] string rptNo,[FromQuery] string pmtNo, [FromQuery] decimal yrId, [FromQuery] string brCode)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionByNo(rptNo,pmtNo, yrId,brCode);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }

        [HttpGet]
        [Route("GetTransactionByRptNo")]
        public async Task<ActionResult<DtoVoucher>> GetTransactionByRptNo([FromQuery] string rptNo,  [FromQuery] decimal yrId, [FromQuery] string brCode)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionByNo(rptNo, string.Empty , yrId,brCode);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }

        [HttpGet]
        [Route("GetTransactionByPmtNo")]
        public async Task<ActionResult<DtoVoucher>> GetTransactionByPmtNo([FromQuery] string pmtNo, [FromQuery] decimal yrId, [FromQuery] string brCode)
        {
            DtoVoucher voc = new();
            voc = await _finVoucherTrnHandler.GetTransactionByNo(string.Empty , pmtNo, yrId,brCode);
            if (voc == null)
            {
                return NotFound();
            }
            return Ok(voc);
        }

        [HttpGet]
        [Route("UpdateLedgerBalance/{yrId:decimal}/{fromDate}/{toDate}/{brCode}")]
        ///decimal yrId, DateTime fromDate, DateTime toDate,string brCode
        public async Task<ActionResult<bool>> UpdateLedgerBalance(decimal yrId, string fromDate, string toDate, string brCode)
        {
            DateTime.TryParse(fromDate, out DateTime fromDateFormatted);
            DateTime.TryParse(toDate, out DateTime toDateFormatted);
            bool result = await _finLedgerTrnHandler.UpdateLedgerBalance(yrId, fromDateFormatted,toDateFormatted ,brCode);
            return Ok(result);
        }


        [HttpGet]
        [Route("GetGroupLedgerBalance/{yrId:decimal}/{brCode}")]
        public async Task<ActionResult<List<FinBal>>> GetGroupLedgerBalance(decimal yrId, string brCode)
        {
            List<FinBal> balList = new();
            var result = await  _finLedgerGroupHandler .GetGroupLedgerBalance(yrId, brCode);
            if(result != null && result.Count >0) balList = result.ToList();
            return Ok(balList);
        }

        [HttpGet]
        [Route("GetGeneralLedgerBalance/{grpId:int}/{yrId:decimal}/{brCode}")]
        public async Task<ActionResult<List<FinBal>>> GetGeneralLedgerBalance(int grpId, decimal yrId, string brCode)
        {
            List<FinBal> balList = new();
            var result = await _finLedgerTrnHandler.GetGeneralLedgerBalance (grpId, yrId, brCode);
            if (result != null && result.Count > 0) balList = result.ToList();
            return Ok(balList);
        }

        [HttpGet]
        [Route("GetGeneralLedgerBalanceMonthWise/{ledId:decimal}/{yrId:decimal}/{brCode}")]
        public async Task<ActionResult<List<FinBal>>> GetGeneralLedgerBalanceMonthWise(decimal ledId, decimal yrId, string brCode)
        {
            List<FinBal> balList = new();
            var result = await _finVoucherTrnHandler.GetLedgerBalanceMonthWise(ledId, yrId, brCode);
            if (result != null && result.Count > 0) balList = result.ToList();
            return Ok(balList);
        }

        [HttpGet]
        [Route("GetGeneralLedgerBalanceDateWise/{ledId:decimal}/{yrId:decimal}/{month:int}/{year:int}/{brCode}")]
        public async Task<ActionResult<List<FinBalGL>>> GetGeneralLedgerBalanceDateWise(decimal ledId, decimal yrId, int month, int year, string brCode)
        {
            List<FinBalGL> balList = new();
            var result = await _finVoucherTrnHandler.GetLedgerBalanceDateWise(ledId, yrId,month,year, brCode);
            if (result != null && result.Count > 0) balList = result.ToList();
            return Ok(balList);
        }

        [HttpGet]
        [Route("GetGeneralLedgerBalanceSlipWise/{ledId:decimal}/{yrId:decimal}/{vocDate}/{brCode}")]
        public async Task<ActionResult<List<FinBalVoucherTrn>>> GetGeneralLedgerBalanceSlipWise(decimal ledId, decimal yrId, string vocDate, string brCode)
        {
            DateTime.TryParse(vocDate, out var vocDateFormatted);
            List<FinBalVoucherTrn> balList = new();
            var result = await _finVoucherTrnHandler.GetLedgerBalanceSlipWise(ledId, yrId,vocDateFormatted,  brCode);
            if (result != null && result.Count > 0) balList = result.ToList();
            return Ok(balList);
        }
    }
}
