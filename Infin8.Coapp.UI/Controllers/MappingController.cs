using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
             IMapBanksHandler mapBanksHandler  )
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

    }
}
