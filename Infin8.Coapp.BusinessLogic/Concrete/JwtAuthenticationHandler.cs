using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            //var user = await _unitOfWork.Members.GetMemberDetailsByUsernameAsync(username);
            Users user = new Users(); 

            if (user == null || !VerifyPassword(password, new byte[] { }, new byte[] { }))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user.Id,"user.username", "user.role");
            var refreshToken = _jwtService.GenerateRefreshToken();

            // Save the refresh token to the database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpireDays()),
                Created = DateTime.UtcNow,
                MemberId = user.Id // Replace with the actual user ID
            };

            await _unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();

            // Return tokens in a DTO
            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 15 // Access token expiry in minutes
            };

            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        private bool VerifyPassword(string enteredPassword, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));

            return computedHash.SequenceEqual(storedHash);
        }

        public async Task<string> RefreshTokenAsync(string refreshToken)
        {
            var tokenEntity = await _unitOfWork.RefreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null || tokenEntity.IsExpired)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            // Generate a new access token
            var member = await _unitOfWork.Members.GetMemberDetailsByMemIdAsync(tokenEntity.MemberId);
            if(member == null || member.MemberName==null) throw new UnauthorizedAccessException("Invalid member.");
            // Pass proper role from DB or from the decision JSON file.
            var accessToken = _jwtService.GenerateAccessToken(member.Mem_Id, member.MemberName , "user.Role"); 

            // Optionally, generate a new refresh token and update the database
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            tokenEntity.Token = newRefreshToken;
            tokenEntity.Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpireDays());
            tokenEntity.Created = DateTime.UtcNow;

            _unitOfWork.RefreshTokenRepository.UpdateRefreshToken(tokenEntity);
            await _unitOfWork.CompleteAsync();

            return accessToken;
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

    }
}
