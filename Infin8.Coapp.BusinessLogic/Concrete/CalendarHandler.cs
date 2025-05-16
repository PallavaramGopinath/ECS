using Infin8.Coapp.Repository;
using Infin8.Coapp.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class CalendarHandler : ICalendarHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly IUtilityHandler _UtilityHandler;
        public CalendarHandler(IUnitOfWork unitOfWork, IUtilityHandler utilityHandler)
        {
            _unitOfWork = unitOfWork;
            _UtilityHandler = utilityHandler;
        }
        public async Task<bool> VerifyDayBegin(string brCode)
        {
            return await _unitOfWork.Calendars.VerifyDayBegin(brCode);
        }
        public async Task<DateTime> GetCurrentDate(string brCode)
        {
            return await _unitOfWork.Calendars.GetCurrentDate(brCode);
        }
        public async Task<int> UpdateCalendarStatus(DateTime currentDate, string brCode, string newStatus)
        {
            return await _unitOfWork.Calendars.UpdateCalendarStatus(currentDate, brCode, newStatus);
        }
        public Task<bool> DayEndProcess(DateTime date)
        {
            throw new NotImplementedException();
        }


    }
}
