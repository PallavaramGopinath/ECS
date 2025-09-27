using Infin8.Coapp.Dto;
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
        Task<bool> DayEndProcess(DtoDayProcess dayProcess);
        Task<bool> CanBeginDay(string brCode);
        Task<DateTime> DayBeginProcess(string brCode);
        Task<List<decimal>> GetFixedDepositIdForInterestCalculation(string tdSchemeType, DateTime toDate,  string brCode);
    }
}
