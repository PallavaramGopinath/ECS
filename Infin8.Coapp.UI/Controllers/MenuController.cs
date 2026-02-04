using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        readonly IMenuMainHandler _menuMainHandler;
        public MenuController(IMenuMainHandler menuMainHandler)
        {
            _menuMainHandler = menuMainHandler;
        }

        [HttpGet]
        [Route("GetMenuStructure/{brCode}/{role}")]
        public async Task<ActionResult<List<MenuMain>>> GetMenuStructure(string brcode, string role)
        {
            List<MenuMain> menuStructure = new();
            var result = await _menuMainHandler.GetMenuStructureAsync(brcode);
            if (result != null && result!.Any())
            {
                menuStructure = result!.ToList();
                return Ok(menuStructure);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
