using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IUserRepository
    {
        Task<bool> AddUser(Users users);
        Task<bool> EditUser(Users users);
        Users? GetUserByUsername(string username);
        string? GetUserRoles(decimal userId);
        //Task<AuthenticationResponse> AuthenticateAsync(string username, string password);
        void CreatePasswordHash(string password, out byte[] passwordHash,out byte[] passwordSalt);
        bool VerifyPassWord(string enteredPassword, byte[] storedHash, byte[] storedSalt);
    }
}
