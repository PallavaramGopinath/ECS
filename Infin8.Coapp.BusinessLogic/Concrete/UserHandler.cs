using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class UserHandler : IUserHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IJwtService _jwtService;
        public UserHandler(IUnitOfWork unitOfWork,IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<bool> AddUser(Users users)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.UserRepository.AddUser(users); 
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! User registration failed");
            }
            return result;
        }

        public async Task<bool> EditUser(Users users)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.UserRepository.EditUser(users);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while modifying the user registration");
            }
            return result;
        }
        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            //var user = CSISContext.Users.Where(x => x.username == username).FirstOrDefault();
            Users? user = new Users();
            user = _unitOfWork.UserRepository.GetUserByUsernameAsync(username, password);
            if (user == null || !_unitOfWork.UserRepository.VerifyPassWord(password, user.password_hash!, user.password_salt!))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user.id,user.username! , user.role!);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpireDays()),
                Created = DateTime.UtcNow,
                MemberId = user.id
            };

            await _unitOfWork.RefreshTokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();

            return new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 15 // Access token expiry in minutes
            };
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(); // Generates a unique key
            passwordSalt = hmac.Key; // Store this salt
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hash the password
        }
    }
}
