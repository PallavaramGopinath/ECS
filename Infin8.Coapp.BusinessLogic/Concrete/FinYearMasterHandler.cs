using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class FinYearMasterHandler : IFinYearMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        readonly ICalendarHandler _calendarHandler;
        public FinYearMasterHandler(IUnitOfWork unitOfWork, ICalendarHandler calendarHandler)
        {
            _unitOfWork = unitOfWork;
            _calendarHandler = calendarHandler;
        }

        public async Task<bool> AddFinYearMasterAsync(Fin_Yr_Master finYrMaster)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.FinYearMaster.AddFinYearMasterAsync(finYrMaster);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! New financial year not saved");
            }
            return result;
        }

        public async Task<Fin_Yr_Master> GetWorkingYear()
        {
            return await _unitOfWork.FinYearMaster.GetWorkingYear();
        }

        public async Task<DayBeginInfo> GetDayBeginInfo(string brCode)
        {
            DayBeginInfo dayBeginInfo = new DayBeginInfo();
            DateTime currentDate = DateTime.Now;
            try
            {
                dayBeginInfo = await  _unitOfWork.FinYearMaster.GetDayBeginInfo(brCode);
                if (dayBeginInfo != null && dayBeginInfo.YearId > 0)
                {
                    currentDate = await _calendarHandler.GetCurrentDate(brCode);
                    dayBeginInfo.CurrentDate = currentDate;
                }
            }
            catch (Exception)
            {
                dayBeginInfo = new();
            }
            return dayBeginInfo!;
        }

        public async Task<List<Fin_Yr_Master>> GetFinancialYearList(string brCode)
        {
            return await _unitOfWork.FinYearMaster.GetFinancialYearList(brCode);
        }
    }
}
