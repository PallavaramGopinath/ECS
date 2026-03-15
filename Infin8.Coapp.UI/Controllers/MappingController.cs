using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MappingController : ControllerBase
    {
        private readonly IMapGeneralHandler _mapGeneralHandler;
        private readonly IMapSuspenseAccountsHandler _mapSuspenseAccountsHandler;
        private readonly IMapBanksHandler _mapBanksHandler;
        public MappingController(IMapGeneralHandler mapGeneralHandler, 
            IMapSuspenseAccountsHandler mapSuspenseAccountsHandler, 
             IMapBanksHandler mapBanksHandler)
        {
            _mapGeneralHandler = mapGeneralHandler;
            _mapSuspenseAccountsHandler = mapSuspenseAccountsHandler;
            _mapBanksHandler = mapBanksHandler;
        }

        [HttpGet]
        [Route("GetMapGeneral/{brCode}")]
        public async Task<ActionResult<Map_General>> GetMapGeneralAsync( string brCode)
        {
            Map_General mapGeneral = new Map_General();

            var result  = await _mapGeneralHandler.GetMapGeneralAsync(brCode );

            if (result == null)
            {
                return NotFound();
            }
            else
                mapGeneral = (Map_General)result;
            return Ok(mapGeneral);
        }

        [HttpGet]
        [Route("GetMapSuspenseAccounts/{brCode}")]
        public async Task<ActionResult<List<Map_SuspenseAccounts>>> GetMapSuspenseAccounts(string brCode)
        {
            List<Map_SuspenseAccounts> mapSusenseAccounts = new List<Map_SuspenseAccounts>();

            var result = await _mapSuspenseAccountsHandler.GetSuspenseLedgerAsync(brCode);

            if (result != null && result!.Any())
            {
                mapSusenseAccounts = result.ToList();
            }
            return Ok(mapSusenseAccounts);
        }

        [HttpGet]
        [Route("GetMapSuspenseAccountsForIntCalc/{brCode}")]
        public async Task<ActionResult<List<Map_SuspenseAccounts>>> GetMapSuspenseAccountsForIntCalc(string brCode)
        {
            List<Map_SuspenseAccounts> mapSusenseAccounts = new List<Map_SuspenseAccounts>();

            var result = await _mapSuspenseAccountsHandler.GetLedgerOnWhichInterestCalculate(brCode);

            if (result != null && result!.Any())
            {
                mapSusenseAccounts = result.ToList();
            }
            return Ok(mapSusenseAccounts);
        }

        [HttpPost]
        [Route("UpdateMapGeneral")]
        public async Task<ActionResult<Map_General>> UpdateMapGeneralAsync([FromBody] Map_General mapGeneral)
        {
            var result = await _mapGeneralHandler.EditMapGeneralAsync(mapGeneral);
            if (result == true)
                return Ok(mapGeneral);
            else
                return BadRequest();
        }

        [HttpPost]
        [Route("UpdateMapSuspenseAccounts")]
        public async Task<ActionResult<List<Map_SuspenseAccounts>>> UpdateMapSuspenseAccounts([FromBody] List<Map_SuspenseAccounts> susAccouontList)
        {
            var result = await _mapSuspenseAccountsHandler.UpdateMapSuspenseAccountsAsync(susAccouontList);
            if (result == true)
                return Ok(susAccouontList);
            else
            {
                List<Map_SuspenseAccounts> nullAccounts = new();
                return Ok(nullAccounts);
            }
        }

        [HttpGet]
        [Route("GetMapBankAccounts/{brCode}")]
        public async Task<ActionResult<List<Map_Banks>>> GetMapBankAccounts(string brCode)
        {
            List<Map_Banks> mapBankAccounts = new List<Map_Banks>();

            var result = await _mapBanksHandler.GetMapBanksListAsync(brCode);

            if (result != null && result!.Any())
            {
                mapBankAccounts = result.ToList();
            }
            return Ok(mapBankAccounts);
        }

        [HttpPost]
        [Route("UpdateMapBankAccounts")]
        public async Task<ActionResult<List<Map_SuspenseAccounts>>> UpdateMapBankAccounts([FromBody] List<Map_Banks> mapBankAccountList)
        {
            var result = await _mapBanksHandler.UpdateMapBankAccountsAsync(mapBankAccountList);
            if (result == true)
                return Ok(mapBankAccountList);
            else
            {
                List<Map_Banks> nullAccounts = new();
                return Ok(nullAccounts);
            }
        }
    }
}
