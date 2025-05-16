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
        Task<RefreshToken> GetByTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        void UpdateRefreshToken(RefreshToken refreshToken);
        void RemoveRefreshToken(RefreshToken refreshToken);
    }

}
