using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IAuthenticationHandler
    {
        Task<AuthenticationResponse> AuthenticateAsync(string username, string password);
        Task<string> RefreshTokenAsync(string refreshToken);
        /// <summary>
        /// Validates a refresh token, issues a new access token for its user and rotates the refresh token.
        /// Throws <see cref="UnauthorizedAccessException"/> when the token is unknown or expired.
        /// </summary>
        Task<(string AccessToken, string RefreshToken)> RotateTokensAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string token);
        (bool isValid, ClaimsPrincipal? claims) ValidateToken(string token);
        //UserInfoDto GetUserInfoDto();
    }
}
