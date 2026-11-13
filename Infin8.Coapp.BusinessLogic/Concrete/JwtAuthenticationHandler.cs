using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace Infin8.Coapp.BusinessLogic
{
    public class JwtAuthenticationHandler : IAuthenticationHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public JwtAuthenticationHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            var user = _unitOfWork.UserRepository.GetUserByUsername(username);
            var daybeginInfo = await _unitOfWork.FinYearMaster.GetDayBeginInfo(user?.BrCode ?? "");
            if (user == null || !VerifyPassword(password, user.Password_Hash, user.Password_Salt))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            
            var accessToken = _jwtService.GenerateAccessToken(user.Id,user.Username!, user.Role!,user.BrCode!,daybeginInfo.YearId,daybeginInfo.YearBeginningDate,daybeginInfo.YearEndDate,daybeginInfo.CurrentDate,daybeginInfo.CalendarStatus! );
            var refreshToken = _jwtService.GenerateRefreshToken();

            var userRoles = (user.Role ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);

            // Save the refresh token to the database
            var refreshTokenEntity = new Refresh_Token
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpireDays()),
                Created = DateTime.UtcNow,
                User_Id = user.Id // Replace with the actual user ID
            };

            await _unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();

            // Return tokens in a DTO
            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 60, // Access token expiry in minutes
                AuthenticatedUserDetailsDto = new AuthenticatedUserDetailsDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Roles = userRoles,
                    BrCode = user.BrCode,
                },
            };
        }

        private bool VerifyPassword(string enteredPassword, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));

            return computedHash.SequenceEqual(storedHash);
        }

        public async Task<string> RefreshTokenAsync(string refreshToken)
            => (await RotateTokensAsync(refreshToken)).AccessToken;

        public async Task<(string AccessToken, string RefreshToken)> RotateTokensAsync(string refreshToken)
        {
            var tokenEntity = await _unitOfWork.RefreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null || DateTime.UtcNow >= tokenEntity.Expires)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            // The refresh token belongs to a login user (users.id), not a member.
            var user = _unitOfWork.UserRepository.GetUserById(tokenEntity.User_Id);
            if (user == null) throw new UnauthorizedAccessException("Invalid user.");

            // Re-read the business day so a Day Begin / Day End since login is picked up.
            var daybeginInfo = await _unitOfWork.FinYearMaster.GetDayBeginInfo(user.BrCode ?? "");
            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username!, user.Role!, user.BrCode!,
                daybeginInfo.YearId, daybeginInfo.YearBeginningDate, daybeginInfo.YearEndDate,
                daybeginInfo.CurrentDate, daybeginInfo.CalendarStatus!);

            // Rotate: the old refresh token stops working once the new one is issued.
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            tokenEntity.Token = newRefreshToken;
            tokenEntity.Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpireDays());
            tokenEntity.Created = DateTime.UtcNow;

            _unitOfWork.RefreshTokenRepository.UpdateRefreshToken(tokenEntity);
            await _unitOfWork.CompleteAsync();

            return (accessToken, newRefreshToken);
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var tokenEntity = await _unitOfWork.RefreshTokenRepository.GetByTokenAsync(token);
            if (tokenEntity == null)
            {
                return false;
            }

            _unitOfWork.RefreshTokenRepository.RemoveRefreshToken(tokenEntity);
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public (bool isValid, ClaimsPrincipal? claims) ValidateToken(string token)
        {
            var claimsPrincipal = _jwtService.ValidateToken(token);
            return (claimsPrincipal != null, claimsPrincipal);
        }

        //public UserInfoDto GetUserInfoDto()
        //{
        //    return _jwtService.GetUserInfoDto();
        //}
    }
}
