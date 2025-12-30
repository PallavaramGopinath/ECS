using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IRefreshTokenRepository
    {
        Task<Refresh_Token> GetByTokenAsync(string token);
        Task AddRefreshTokenAsync(Refresh_Token refreshToken);
        void UpdateRefreshToken(Refresh_Token refreshToken);
        void RemoveRefreshToken(Refresh_Token refreshToken);
    }

}
