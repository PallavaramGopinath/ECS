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

        [HttpPost]
        [Route("AddSBAccountScheme")]
        public async Task<ActionResult<List<SBCA_Schemes>>> AddSBAccountScheme([FromBody] SBCA_Schemes scheme)
        {
            List<SBCA_Schemes> sbcaSchemes = new();
            var result = await _sbcaSchemesHandler.AddSBCASchemesAsync(scheme,scheme.BrCode! );
            if (result != null && result.Count >0) sbcaSchemes = result.ToList();
            return Ok(sbcaSchemes);
        }

        [HttpPost]
        [Route("EditSBAccountScheme")]
        public async Task<ActionResult<List<SBCA_Schemes>>> EditSBAccountScheme([FromBody] SBCA_Schemes scheme)
        {
            List<SBCA_Schemes> schemes = new();
            var query = await _sbcaSchemesHandler.EditSBCASchemesAsync(scheme, scheme.BrCode!);
            if (query != null && query.Count > 0) schemes = query.ToList();
            return Ok(schemes);
        }

        [HttpPost]
        [Route("AddSBAccount")]
        public async Task<ActionResult<(bool result, decimal accId, string accNo)>> AddSBAccount([FromBody] SBCA_Master sbcaMaster)
        {
            var result = await _sbcaMasterHandler.AddSBCAMasterAsync(sbcaMaster);
            return Ok(result);
        }

        [HttpPost]
        [Route("AddNewSBAccount")]
        public async Task<ActionResult<SBCA_Master>> AddNewSBAccount([FromBody] SBCA_Master sbcaMaster)
        {
            var result = await _sbcaMasterHandler.AddNewSBAccount(sbcaMaster);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetSBAccountDataByMemId/{memId:decimal}/{brCode}")]
        public async Task<ActionResult<DtoSBAccountNo>> GetSBAccountDataByMemId(decimal memId, string brCode)
        {
            DtoSBAccountNo sbAccountData = new();
            var result = await _sbcaMasterHandler.GetSBAccountDataByMemIdAsync(memId, brCode);
            if(result != null && result.SBAccountId >0)
            {
                sbAccountData = result;
                return Ok(sbAccountData);
            }
            else
            {
                sbAccountData = new();
                return Ok(sbAccountData);
            }
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
        [Route("GetSBAccountNoByMemId/{memId:decimal}/{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetSBAccountNoByMemId(decimal memId,string brCode)
        {
            List<DropdownItem> sbAccountNos = new();
            var result = await _sbcaMasterHandler.GetSBCANosByMemIdAsync(memId, brCode);
            if (result != null && result.Count >0)
            {
                sbAccountNos = result.ToList();
                return Ok(sbAccountNos);
            }
            else
            {
                sbAccountNos = new();
                return Ok(sbAccountNos);
            }
        }

        [HttpGet]
        [Route("GetSBAccountNoByMemIdAsync/{memId:decimal}/{brCode}")]
        public async Task<ActionResult<string>> GetSBAccountNoByMemIdAsync(decimal memId, string brCode)
        {
            string sbAccountNo = string.Empty;
            var result = await _sbcaMasterHandler.GetSBCANoByMemIdAsync(memId, brCode);
            if (result != null && result.Length > 0)
            {
                sbAccountNo = result;
                return Ok(sbAccountNo);
            }
            else
            {
                sbAccountNo = string.Empty;
                return Ok(sbAccountNo);
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

        [HttpGet]
        [Route("GetSBCASchemes/{brCode}")]
        public async Task<ActionResult<List<SBCA_Schemes>>> GetSBCASchemes(string brCode)
        {
            List<SBCA_Schemes> sbcaSchemes = new();
            try
            {
                var response = await _sbcaSchemesHandler.GetSBCASchemes(brCode);
                if (response != null && response.Count >0) sbcaSchemes = response.ToList();
            }
            catch (Exception)
            {
                sbcaSchemes = new();
            }
            return Ok(sbcaSchemes);
        }
    }
}
