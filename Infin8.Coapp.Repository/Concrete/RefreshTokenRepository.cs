using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class RefreshTokenRepository : Repository<Refresh_Token>, IRefreshTokenRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public RefreshTokenRepository(CSISContext context) : base(context)
        {
        }

        public async Task<Refresh_Token> GetByTokenAsync(string token)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await CSISContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task AddRefreshTokenAsync(Refresh_Token refreshToken)
        {
            await CSISContext.RefreshTokens.AddAsync(refreshToken);
        }

        public void UpdateRefreshToken(Refresh_Token refreshToken)
        {
            CSISContext.RefreshTokens.Update(refreshToken);
        }

        public void RemoveRefreshToken(Refresh_Token refreshToken)
        {
            CSISContext.RefreshTokens.Remove(refreshToken);
        }
    }
}
