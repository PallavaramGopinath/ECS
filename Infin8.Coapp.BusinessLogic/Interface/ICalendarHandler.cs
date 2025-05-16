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
        Task<bool> DayEndProcess(DateTime date);
    }
}
