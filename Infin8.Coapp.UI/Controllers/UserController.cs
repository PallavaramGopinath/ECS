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
        [HttpPost]
        [Route("UserRegister")]
        public async Task<ActionResult> UserRegister([FromBody] UserRegistration userRegistration)
        {
            try
            {
                if (userRegistration == null)
                {
                    return BadRequest(new { Message = "Invalid username format." });
                }

                byte[] passwordHash;
                byte[] passwordSalt;
                _userHandler.CreatePasswordHash(userRegistration.password!, out passwordHash, out passwordSalt);
                Users user = new Users()
                {
                    Id = 0,
                    Username = userRegistration.username!,
                    Email = userRegistration.email,
                    First_Name = userRegistration.first_name,
                    Last_Name = userRegistration.last_name,
                    Password_Hash = passwordHash,
                    Password_Salt = passwordSalt,
                    Is_Active = true,
                    Is_Account_Closed = false,
                    Account_Closed_Date = null,
                    Last_Login_Date = null,
                    Failed_Login_Attempts = 0,
                    Is_Locked = false,
                    Account_Locked_Until = null,
                    Created_At = DateTime.UtcNow,
                    Updated_At =  DateTime.UtcNow ,
                    Mobile_Number = userRegistration.mobile_number ,
                    Reset_Token = null,
                    Reset_Token_Expires_At = null,
                    BrCode = "11001",
                    Role = userRegistration.role
                };
                var authResponse = await _userHandler.AddUser(user);
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while registring user", Error = ex.Message });
            }
        }

        [HttpGet]
        [Route("get-appstate/{userName}/{password}")]
        public async Task<ActionResult<AppState>> GetAppState(string userName, string password)
        {
            Users user = new();
            AppState appState = new AppState();
            var result = _userHandler.GetUserByUsernameAsync(userName, password);
            if (result != null && result.Id > 0)
            {
                user = result;
                var appsettingResult = await _userHandler.GetAppStateAsync(user);
                if (appsettingResult != null && appsettingResult.SocietyType > 0)
                {
                    appState = appsettingResult;
                    return Ok(appState);
                }
            }
            else
            {
                return StatusCode(500, new { errorMessage = "error in fetcing user credentials" });
            }
            // Ensure all code paths return a value
            return NotFound(new { errorMessage = "App state not found for the user." });
        }
    }
}
