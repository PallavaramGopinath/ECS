using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface ICalendarRepository
    {
        Task<bool> VerifyDayBegin(string brCode);
        Task<DateTime> GetCurrentDate(string brCode);
        Task<int> UpdateCalendarStatus(DateTime currentDate,string brCode,string newStatus);
        Task<bool> DayEndProcess(string brCode);
        Task<bool> CanBeginDay(string brCode);
        Task<bool> DayBeginProcess(string brCode);
        Task<List<decimal>> GetFixedDepositIdForInterestCalculation(string tdSchemeType, DateTime toDate, string brCode);
    }
}
