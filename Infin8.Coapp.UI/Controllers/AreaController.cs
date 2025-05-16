using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AreaController : ControllerBase
    {
        readonly IAreaMasterHandler _areaMasterHandler;
        public AreaController(IAreaMasterHandler areaMasterHandler)
        {
            _areaMasterHandler = areaMasterHandler;
        }
        [HttpGet]
        [Route("{brCode}")]
        public async Task<ActionResult<List<DropdownItem>>> GetAreaItems(string brCode)
        {
            List<DropdownItem> items = new List<DropdownItem>();
            items = await _areaMasterHandler.GetAreaItems(brCode);
            if (items.Count > 0) return Ok(items);
            else return NotFound();
        }
    }
}
