using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IUserHandler
    {
        Task<bool> AddUser(Users users);
        Task<bool> EditUser(Users users);

        Users? GetUserByUsernameAsync(string username, string password);
        Task<AuthenticationResponse> AuthenticateAsync(string username, string password);

        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

        Task<AppState> GetAppStateAsync(Users user);
    }
}
