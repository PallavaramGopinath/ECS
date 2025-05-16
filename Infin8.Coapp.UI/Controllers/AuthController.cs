using Infin8.Coapp.BusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Infin8.Coapp.Models;
using Microsoft.AspNetCore.Authorization;
using Infin8.Coapp.Utility;
using Infin8.Coapp.Dto;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Drawing.Text;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infin8.Coapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationHandler _authenticationHandler;
        private readonly IUserHandler _userHandler;
        /// <summary>
        /// , IUserHandler userHandler
        /// </summary>
        /// <param name="authenticationHandler"></param>
        public AuthController(IAuthenticationHandler authenticationHandler, IUserHandler userHandler)
        {
            _authenticationHandler = authenticationHandler;
            _userHandler = userHandler;

        }

        //[HttpPost]
        //[Route("/Register")]
        //public async Task<IActionResult> UserRegister([FromBody] UserRegistration userRegistration)
        //{
        //    try
        //    {
        //        if (userRegistration == null)
        //        {
        //            return BadRequest(new { Message = "Invalid username format." });
        //        }

        //        byte[] passwordHash;
        //        byte[] passwordSalt;
        //        _userHandler.CreatePasswordHash(userRegistration.password!, out passwordHash, out passwordSalt);
        //        Users user = new Users()
        //        {
        //            id = 0,
        //            username = userRegistration.username!,
        //            email = userRegistration.email,
        //            first_name = userRegistration.first_name,
        //            last_name = userRegistration.last_name,
        //            password_hash = passwordHash,
        //            password_salt = passwordSalt,
        //            is_active = true,
        //            is_account_closed = false,
        //            account_closed_date = null,
        //            last_login_date = null,
        //            failed_login_attempts = 0,
        //            is_locked = false,
        //            account_locked_until = null,
        //            created_at = DateTime.UtcNow,
        //            updated_at = DateTime.UtcNow,
        //            mobile_number = userRegistration.mobile_number,
        //            reset_token = null,
        //            reset_token_expires_at = null,
        //            brcode = "11001",
        //            role = userRegistration.role
        //        };
        //        var authResponse = await _userHandler.AddUser(user);
        //        return Ok(authResponse);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { Message = "An unexpected error occurred while registring user", Error = ex.Message });
        //    }
        //}

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                // Validate input
                if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
                {
                    return BadRequest(new { Message = "Username and password are required." });
                }

                // Check for valid username & password format
                if (!Utilities.IsValidUsername(loginModel.Username))
                {
                    return BadRequest(new { Message = "Invalid username format." });
                }

                if (!Utilities.IsValidPassword(loginModel.Password))
                {
                    return BadRequest(new { Message = "Password does not meet security requirements." });
                }

                var authResponse = await _authenticationHandler.AuthenticateAsync(loginModel.Username, loginModel.Password);

                // Check if authentication was successful
                if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.AccessToken))
                {
                    return Unauthorized(new { Message = "Invalid credentials." });
                }

                return Ok(authResponse);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred.", Error = ex.Message });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                // Validate the request object and refresh token
                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(new { Message = "Invalid refresh token request." });
                }

                var accessToken = await _authenticationHandler.RefreshTokenAsync(request.RefreshToken);

                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    return Unauthorized(new { Message = "Login again. Invalid or expired refresh token." });
                }

                return Ok(new { AccessToken = accessToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            try
            {
                // Validate input
                if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return BadRequest(new { Message = "Refresh token is required." });
                }

                // Attempt to revoke the token
                var isRevoked = await _authenticationHandler.RevokeTokenAsync(request.RefreshToken);

                if (!isRevoked)
                {
                    return BadRequest(new { Message = "Invalid or expired token." });
                }

                return Ok(new { Message = "Token revoked successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An unexpected error occurred.", Error = ex.Message });
            }
        }

        [HttpPost]
        [Route("GetJWt")]
        public IActionResult Index([FromBody] LoginModel model)
        {
            if(model.Username=="Admin" && model.Password == "aaa123")
            {
                var claim = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,model.Username!),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                };
                var singningkey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecureKey"));
                var token = new JwtSecurityToken(
                    issuer: "https://localhost:7020",
                    audience: "https://localhost:7020",
                    expires:DateTime.UtcNow.AddHours(1),
                    //claims:claims,
                    signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(singningkey,SecurityAlgorithms.HmacSha256)
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
             }
            return Unauthorized();
        }
    }

    public class LoginModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string? RefreshToken { get; set; }
    }
    public class RevokeTokenRequest
    {
        public string? RefreshToken { get; set; }
    }
}
