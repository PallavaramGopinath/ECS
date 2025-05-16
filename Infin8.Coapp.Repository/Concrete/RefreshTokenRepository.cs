using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public RefreshTokenRepository(CSISContext context) : base(context)
        {
        }

        public async Task<RefreshToken> GetByTokenAsync(string token)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return await CSISContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token);
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
        {
            await CSISContext.RefreshTokens.AddAsync(refreshToken);
        }

        public void UpdateRefreshToken(RefreshToken refreshToken)
        {
            CSISContext.RefreshTokens.Update(refreshToken);
        }

        public void RemoveRefreshToken(RefreshToken refreshToken)
        {
            CSISContext.RefreshTokens.Remove(refreshToken);
        }
    }
}
