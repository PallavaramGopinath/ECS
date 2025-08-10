using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SBAccountController : ControllerBase
    {
        readonly IMemTrnHandler _memTrnHandler;
        readonly ISBCAMasterHandler _sbcaMasterHandler;
        readonly ISBCASchemesHandler _sbcaSchemesHandler;
        public SBAccountController(IMemTrnHandler memTrnHandler,ISBCAMasterHandler sbcaMasterHandler,ISBCASchemesHandler sbcaSchemeHandler)
        {
            _memTrnHandler = memTrnHandler;
            _sbcaMasterHandler = sbcaMasterHandler;
            _sbcaSchemesHandler = sbcaSchemeHandler;
        }
        [HttpGet]
        [Route("GetSBAccountBalanceWithIds/{memId:decimal}/{brCode}")]
        public async Task<ActionResult<DtoSBAccountBalanceWithIds>> GetSBAccountBalanceWithIds(decimal memId, string brCode)
        {
            DtoSBAccountBalanceWithIds sbData = new();
            var result = await _memTrnHandler.GetSBAccountBalanceWithIds(memId, brCode);
            if(result != null )
            {
                sbData = result;
                return Ok(sbData);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("GetSBAccountNoByMemId/{memId:decimal}")]
        public async Task<ActionResult<List<DropdownItem>>> GetSBAccountNoByMemId(decimal memId)
        {
            List<DropdownItem> sbAccountNos = new();
            var result = await _sbcaMasterHandler.GetSBCANosByMemIdAsync(memId);
            if (result != null && result.Count >0)
            {
                sbAccountNos = result.ToList();
                return Ok(sbAccountNos);
            }
            else
            {
                return NotFound();
            }
        }   

        [HttpGet]
        [Route("GetSBAccountLedgerIds/{brCode}")]
        public async Task<ActionResult<(decimal ledId, decimal intLedId)>> GetSBAccountLedgerIds(string brCode)
        {
            decimal ledId = 0;
            decimal intLedId = 0;
            try
            {
                (ledId,intLedId) = await _sbcaSchemesHandler.GetSBAccountLedgerIds(brCode);
                if(ledId > 0 && intLedId > 0)
                { 
                    return Ok((ledId, intLedId));
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [Route("GetSBCAScheme/{brCode}")]
        public async Task<ActionResult<SBCA_Schemes>> GetSBCAScheme(string brCode)
        {
            SBCA_Schemes sbcaScheme = new();
            try
            {
                sbcaScheme = await _sbcaSchemesHandler.GetSBCAScheme(brCode);
                if (sbcaScheme != null)
                {
                    return Ok(sbcaScheme);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
