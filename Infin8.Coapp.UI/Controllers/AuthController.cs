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
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


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
        private readonly IGeneralHandler _generalHandler;
        private readonly CSISContext _context;
        //private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// , IUserHandler userHandler
        /// </summary>
        /// <param name="authenticationHandler"></param>
        public AuthController(IAuthenticationHandler authenticationHandler,
            IUserHandler userHandler, IAntiforgery antiforgery, IGeneralHandler generalHandler, CSISContext context)
        {
            _authenticationHandler = authenticationHandler;
            _userHandler = userHandler;
            _antiforgery = antiforgery;
            _generalHandler = generalHandler;
            _context = context;
        }

        [HttpGet("get-antiforgery-token")]
        public IActionResult GetAntiforgeryToken()
        {
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            return Ok(new { token = tokens.RequestToken });
        }

        [HttpPost("Register")]
        public IActionResult Register([FromBody] UserRegistration userRegistration) =>
            StatusCode(410, new { Message = "Self-registration is disabled. Contact your society administrator." });

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
                if (!Utilities.IsValidUsername(loginModel.Username)
                    && !new EmailAddressAttribute().IsValid(loginModel.Username))
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
                    authResponse.AuthenticatedUserDetailsDto.BrCode,
                    MustChangePassword = await _context.Users.Where(x => x.Id == authResponse.AuthenticatedUserDetailsDto.Id)
                        .Select(x => x.Must_Change_Password).SingleAsync(),
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

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var origin = Request.Headers.Origin.ToString();
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || uri.Scheme != Request.Scheme || uri.Authority != Request.Host.Value)
                return Forbid();
            if (request is null || request.NewPassword != request.ConfirmPassword
                || request.NewPassword is null || request.NewPassword.Length < 12
                || !request.NewPassword.Any(char.IsUpper)
                || !request.NewPassword.Any(char.IsLower)
                || !request.NewPassword.Any(char.IsDigit))
                return BadRequest("The new password must be at least 12 characters with upper and lower case letters and a digit.");
            if (!decimal.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return Unauthorized();
            var user = await _context.Users.FindAsync(userId);
            if (user is null || !user.Must_Change_Password || !user.Is_Active
                || user.Invitation_Expires_At <= DateTime.UtcNow)
                return BadRequest("The invitation is no longer active. Contact the platform administrator.");
            if (string.IsNullOrEmpty(request.CurrentPassword)) return BadRequest("Enter the temporary password.");
            using (var hmac = new System.Security.Cryptography.HMACSHA512(user.Password_Salt))
            {
                var entered = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.CurrentPassword));
                if (!CryptographicOperations.FixedTimeEquals(entered, user.Password_Hash))
                    return BadRequest("The temporary password is incorrect.");
            }
            if (request.CurrentPassword == request.NewPassword)
                return BadRequest("Choose a different password.");
            _userHandler.CreatePasswordHash(request.NewPassword, out var hash, out var salt);
            user.Password_Hash = hash;
            user.Password_Salt = salt;
            user.Must_Change_Password = false;
            user.Invitation_Expires_At = null;
            user.Security_Stamp = Guid.NewGuid().ToString("N");
            user.Updated_At = DateTime.UtcNow;
            _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(x => x.User_Id == userId));
            var onboarding = await _context.Society_Admin_Onboarding.AsNoTracking()
                .FirstOrDefaultAsync(x => x.User_Id == userId);
            if (onboarding is not null)
                _context.Society_Admin_Invitation_Audit.Add(new()
                {
                    Society_Id = onboarding.Society_Id, User_Id = userId, Actor_Id = userId,
                    Event = "PasswordChanged", Occurred_At = user.Updated_At
                });
            await _context.SaveChangesAsync();
            Response.Cookies.Delete("auth-token", CookieOptions());
            Response.Cookies.Delete("refresh-token", CookieOptions());
            return Ok(new { Message = "Password changed. Sign in with your new password." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("auth-token", CookieOptions());
            Response.Cookies.Delete("refresh-token", CookieOptions());
            return Ok(new { Message = "Logged out successfully" });

        }

        /// <summary>
        /// Silent refresh used by the WASM client when an API call returns 401. Reads the HttpOnly
        /// refresh-token cookie, rotates it and re-issues the auth-token cookie.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refresh-token"];
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized(new { Message = "Login again. No refresh token." });
            try
            {
                var (accessToken, newRefreshToken) = await _authenticationHandler.RotateTokensAsync(refreshToken);
                Response.Cookies.Append("auth-token", accessToken, CookieOptions());
                Response.Cookies.Append("refresh-token", newRefreshToken, CookieOptions());
                return Ok(new { Message = "Session refreshed." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
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

        [HttpPost("GetJWt")]
        public IActionResult GetJwt([FromBody] LoginModel model) =>
            StatusCode(410, new { Message = "This demonstration token endpoint is disabled." });

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userInfoDto = this.GetUserInfoDto();
            // Surface the institution type (Gen_Bank_Name.Bank_Type) so the client can gate
            // ECS-only loan products. Defensive: a lookup failure leaves SocietyType = 0
            // (Unknown), which is treated as the incumbent PCARDB and never hides PCARDB items.
            try
            {
                if (!string.IsNullOrEmpty(userInfoDto.BrCode))
                {
                    var society = await _generalHandler.GetSocietyData(userInfoDto.BrCode);
                    userInfoDto.SocietyType = society?.Bank_Type ?? 0;
                }
            }
            catch
            {
                userInfoDto.SocietyType = 0;
            }
            return Ok(userInfoDto);
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
    public class ChangePasswordRequest
    {
        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
