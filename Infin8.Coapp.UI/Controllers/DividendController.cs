using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DividendController : ControllerBase
    {
        private IMemPayableMasterHandler _memPayableMasterHandler;
        public DividendController( IMemPayableMasterHandler memPayableMasterHandler )
        {
            _memPayableMasterHandler = memPayableMasterHandler;
        }

        [HttpGet]
        [Route("GetLastPayableData/{pbleType:int}/{status}/{brCode}")]
        public async Task<ActionResult<Mem_Payable_Master>> GetLastPayableData(int pbleType, string status, string brCode)
        {
            Mem_Payable_Master master = new();
            var result = await  _memPayableMasterHandler.GetDividendLastCalculatedData(pbleType, status, brCode);
            if (result != null && master.ROI_Pble > 0)
                master = result;
            return Ok(master);
        }
    }
}
