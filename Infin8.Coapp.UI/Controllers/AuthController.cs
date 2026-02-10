using Infin8.Coapp.BusinessLogic;
using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.UI.Controllers;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Infin8.Coapp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationHandler _authenticationHandler;
        private readonly IUserHandler _userHandler;
        private readonly IAntiforgery _antiforgery;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// , IUserHandler userHandler
        /// </summary>
        /// <param name="authenticationHandler"></param>
        public AuthController(IAuthenticationHandler authenticationHandler,
            IUserHandler userHandler, IAntiforgery antiforgery)
        {
            _authenticationHandler = authenticationHandler;
            _userHandler = userHandler;
            _antiforgery = antiforgery;
        }

        [HttpGet("get-antiforgery-token")]
        public IActionResult GetAntiforgeryToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }

        [HttpPost("Register")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Register([FromBody] UserRegistration userRegistration)
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
                    //Id = 11001001, No need to pass Id, it will be generated while adding from repository
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
                    Updated_At = DateTime.UtcNow,
                    Mobile_Number = userRegistration.mobile_number,
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

        [HttpPost("login")]
        [IgnoreAntiforgeryToken]
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
                if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.AccessToken)
                    || string.IsNullOrWhiteSpace(authResponse.RefreshToken))
                {
                    return Unauthorized(new { Message = "Invalid credentials." });
                }

                Response.Cookies.Append("auth-token", authResponse.AccessToken, CookieOptions());
                Response.Cookies.Append("refresh-token", authResponse.RefreshToken, CookieOptions());

                return Ok(new
                {
                    authResponse.AuthenticatedUserDetailsDto.Id,
                    authResponse.AuthenticatedUserDetailsDto.Username,
                    authResponse.AuthenticatedUserDetailsDto.Email,
                    authResponse.AuthenticatedUserDetailsDto.Roles,
                    authResponse.AuthenticatedUserDetailsDto.BrCode
                });
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

        private static CookieOptions CookieOptions() => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth-token", CookieOptions());
            Response.Cookies.Delete("refresh-token", CookieOptions());
            return Ok(new { Message = "Logged out successfully" });

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
            if (model.Username == "Admin" && model.Password == "aaa123")
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
                    expires: DateTime.UtcNow.AddHours(1),
                    //claims:claims,
                    signingCredentials: new Microsoft.IdentityModel.Tokens.SigningCredentials(singningkey, SecurityAlgorithms.HmacSha256)
                    );
                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo
                });
            }
            return Unauthorized();
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            return Ok(this.GetUserInfoDto());
            //return Ok(new UserInfoDto
            //{
            //    UserId = Convert.ToDecimal ( User.FindFirstValue(ClaimTypes.NameIdentifier)),
            //    Username = User.Identity?.Name!,
            //    Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList (),
            //    BrCode = User.FindFirst("BrCode")?.Value,
            //    YrId = User.FindFirst("YrId")?.Value,
            //    YrBeginningDate = User.FindFirst("YrBeginningDate")?.Value,
            //    YrEndDate = User.FindFirst("YrEndDate")?.Value,
            //    CurrentDate = User.FindFirst("CurrentDate")?.Value
            //});
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
