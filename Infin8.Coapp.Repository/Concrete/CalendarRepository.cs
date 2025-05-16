using Infin8.Coapp.Models;
using Infin8.Coapp.Utility;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infin8.Coapp.Repository
{
    public class CalendarRepository :Repository<Business_Day>, ICalendarRepository
    {
        
        public CSISContext CSISContext => (CSISContext)Context;
        public CalendarRepository(DbContext context) : base(context)
        {
        }
        public async Task<bool> VerifyDayBegin(string brCode)
        {
            bool result = false;
            var verifyDate = await CSISContext.Business_Day
                            .Where(x => x.Calendar_Status == "N" && x.BrCode == brCode)
                            .CountAsync();
            if (verifyDate > 0) { result = true; }
            return result;
        }
        public async Task<DateTime> GetCurrentDate(string brCode)
        {
            DateTime currentDate;
            try
            {
                currentDate = await CSISContext.Business_Day
                    .Where(c => c.Calendar_Status == "N" && c.BrCode == brCode)
                    .OrderBy(c => c.Calendar_Id)
                    .Select(c => c.Calendar_Date)
                    .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return currentDate;
        }
        public async Task<int> UpdateCalendarStatus(DateTime currentDate, string brCode, string newStatus)
        {
            return await CSISContext.Business_Day
                .Where(b => b.Calendar_Date == currentDate && b.BrCode == brCode)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.Calendar_Status, newStatus));
        }
        public Task<bool> DayEndProcess(DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
