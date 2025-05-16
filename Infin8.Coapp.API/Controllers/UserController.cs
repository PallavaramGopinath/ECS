using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
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
        [Route("/UserRegister")]
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
                    id = 0,
                    username = userRegistration.username!,
                    email = userRegistration.email,
                    first_name = userRegistration.first_name,
                    last_name = userRegistration.last_name,
                    password_hash = passwordHash,
                    password_salt = passwordSalt,
                    is_active = true,
                    is_account_closed = false,
                    account_closed_date = null,
                    last_login_date = null,
                    failed_login_attempts = 0,
                    is_locked = false,
                    account_locked_until = null,
                    created_at = DateTime.UtcNow,
                    updated_at =  DateTime.UtcNow ,
                    mobile_number = userRegistration.mobile_number ,
                    reset_token = null,
                    reset_token_expires_at = null,
                    brcode = "11001",
                    role = userRegistration.role
                };
                var authResponse = await _userHandler.AddUser(user);
                return Ok(authResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred while registring user", Error = ex.Message });
            }
        }
    }
}
