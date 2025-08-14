using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ICalendarHandler
    {
        Task<bool> VerifyDayBegin(string brCode);
        Task<DateTime> GetCurrentDate(string brCode);
        Task<int> UpdateCalendarStatus(DateTime currentDate, string brCode, string newStatus);
        Task<bool> DayEndProcess(DateTime toDate,decimal createdBy,decimal yrId, string brCode);
        Task<bool> CanBeginDay(string brCode);
        Task<bool> DayBeginProcess(string brCode);
        Task<List<decimal>> GetFixedDepositIdForInterestCalculation(string tdSchemeType, DateTime toDate,  string brCode);
    }
}
