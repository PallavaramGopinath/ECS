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

        public Users? GetUserByUsernameAsync(string username, string password)
        {
            return _unitOfWork.UserRepository.GetUserByUsernameAsync(username, password);
        }
        public async Task<AuthenticationResponse> AuthenticateAsync(string username, string password)
        {
            //var user = CSISContext.Users.Where(x => x.username == username).FirstOrDefault();
            Users? user = new Users();
            user = _unitOfWork.UserRepository.GetUserByUsernameAsync(username, password);
            if (user == null || !_unitOfWork.UserRepository.VerifyPassWord(password, user.Password_Hash!, user.Password_Salt!))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var accessToken = _jwtService.GenerateAccessToken(user.Id,user.Username! , user.Role!);
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                Expires = DateTime.UtcNow.AddDays(_jwtService.GetRefreshTokenExpireDays()),
                Created = DateTime.UtcNow,
                MemberId = user.Id
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

        public async Task<AppState> GetAppStateAsync(Users user)
        {
            AppState appState = new();
            Fin_Yr_Master currentYear = new();
            Gen_Bank_Name societyData = new Gen_Bank_Name();
            DateTime workingDate = DateTime.UtcNow;
            try
            {
                currentYear = await  _unitOfWork.FinYearMaster.GetWorkingYear();
                societyData = await _unitOfWork.General.GetSocietyData(user.BrCode!);
                workingDate = await _unitOfWork.Calendars.GetCurrentDate(user.BrCode !);
                appState = new()
                {
                    UserId = user.Id,
                    UserName = user.Username,
                    BrCode = user.BrCode,
                    Working_Date = workingDate,
                    Yr_FromDate = currentYear.From_Date,
                    Yr_ToDate = currentYear.To_Date ,
                    Society_Name = societyData.Bank_Name,
                    SocietyType = societyData.Bank_Type
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return appState;
        }
    }
}
