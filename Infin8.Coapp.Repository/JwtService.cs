using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Utility
{
    using Infin8.Coapp.Models;
    using Infin8.Coapp.Repository.Entities;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Text;

    public interface IJwtService
    {
        public int GetRefreshTokenExpireDays();
        public int GetAccessTokenExpireMinutes();

        public CookieOptions GetCookieOptions();

        public CookieOptions GetCookieDeleteOptions();
        public ClaimsPrincipal ValidateToken(string token);
        public string GenerateAccessToken(decimal userId, string username, string roles, string brCode, decimal yrId, DateTime yrBeginningDate, DateTime yrEndDate, DateTime currentDate);
        public string GenerateRefreshToken();
    }

    public class JwtService: IJwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpireMinutes;
        private readonly int _refreshTokenExpireDays;
        private readonly int _cookieExpireHour;

        public JwtService(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _secretKey = configuration["Jwt:Key"]?? "";
            _issuer = configuration["Jwt:Issuer"] ?? "";
            _audience = configuration["Jwt:Audience"] ?? "";
            _accessTokenExpireMinutes = int.Parse(configuration["Jwt:AccessTokenExpireMinutes"] ?? "5");
            _refreshTokenExpireDays = int.Parse(configuration["Jwt:RefreshTokenExpireDays"] ?? "7") ;
            _cookieExpireHour = int.Parse(configuration["Jwt:CookieExpireHour"] ?? "8");
        }

        public int GetRefreshTokenExpireDays()
        {
            return _refreshTokenExpireDays;
        }

        public int GetAccessTokenExpireMinutes()
        {
            return _accessTokenExpireMinutes;
        }

        public string GenerateAccessToken(decimal userId, string username, string roles,string brCode,decimal yrId, DateTime yrBeginningDate, DateTime yrEndDate, DateTime currentDate)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userRoles = (roles ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);

            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("BrCode", brCode), // Example of a custom claim)
                    new Claim("YrId", yrId.ToString()),
                    new Claim("YrBeginningDate", yrBeginningDate.ToString("o")),
                    new Claim("YrEndDate", yrEndDate.ToString("o")),
                    new Claim("CurrentDate", currentDate.ToString("o"))
                };

            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Trim()));
            }

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_accessTokenExpireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// Use this method when your API doesn't use ASP.NET’s JWT authentication middleware but still 
        /// needs to validate tokens.Also, when handling JWT authentication from third-party clients 
        /// or services.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>ClaimsPrincipal</returns>
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }

        public CookieOptions GetCookieOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(_cookieExpireHour) // or whatever
            };
        }

        public CookieOptions GetCookieDeleteOptions()
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(-_cookieExpireHour)
            };
        }
    }
}
