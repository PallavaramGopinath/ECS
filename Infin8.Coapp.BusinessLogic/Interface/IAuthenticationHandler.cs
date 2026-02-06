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
        Task<bool> RevokeTokenAsync(string token);
        (bool isValid, ClaimsPrincipal? claims) ValidateToken(string token);
    }
}
