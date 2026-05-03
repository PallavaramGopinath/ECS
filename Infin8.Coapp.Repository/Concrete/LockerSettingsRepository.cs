using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LockerSettingsRepository : Repository<Locker_Settings>, ILockerSettingsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerSettingsRepository(DbContext context) : base(context)
        {
        }

        public Task<List<Locker_Settings>> AddLockerSettings(Locker_Settings lockerSettings)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Settings>> EditLockerSettings(Locker_Settings lockerSettings)
        {
            throw new NotImplementedException();
        }

        public async Task<Locker_Settings> GetLockerSetting(string brCode)
        {
            Locker_Settings setting = new();
            var query = await  CSISContext.Locker_Settings.Where(x => x.BrCode == brCode).FirstOrDefaultAsync();
            if (query != null)
            {
                setting = query;
            }
            return setting;
        }
        
    }
}