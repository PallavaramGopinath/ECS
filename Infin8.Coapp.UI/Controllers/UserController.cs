using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Infin8.Coapp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserHandler _userHandler;
        public UserController(IUserHandler userHandler)
        {
            _userHandler = userHandler;
        }
        [HttpPost("UserRegister")]
        public IActionResult UserRegister([FromBody] UserRegistration userRegistration) =>
            StatusCode(410, new { Message = "Self-registration is disabled. Contact your society administrator." });

        [HttpGet("get-appstate/{userName}/{password}")]
        public IActionResult GetAppState(string userName, string password) =>
            StatusCode(410, new { Message = "This legacy endpoint is disabled." });
    }
}
