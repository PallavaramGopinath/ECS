using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
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
        readonly ITermDepositTrnHandler _TermDepositTrnHandler;
        public CalendarHandler(IUnitOfWork unitOfWork, IUtilityHandler utilityHandler, ITermDepositTrnHandler termDepositTrnHandler)
        {
            _unitOfWork = unitOfWork;
            _UtilityHandler = utilityHandler;
            _TermDepositTrnHandler = termDepositTrnHandler;
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

        public async Task<bool> DayEndProcess(DateTime toDate, decimal createdBy, decimal yrId, string brCode)
        {
            bool result = false;
            List<decimal> fdIdList = new();
            List<FDDetailsVM> fdList = new();
            List<TermDeposit_Trn> fdInterestCalculatedList = new();
            try
            {
                _unitOfWork.BeginTransaction();
                result = await _unitOfWork.Calendars.DayBeginProcess(brCode);

                fdIdList = await _unitOfWork.Calendars.GetFixedDepositIdForInterestCalculation("F", toDate, brCode);
                fdList = await _unitOfWork.TermDepositTrn.GetFDPayableByTDIdsAsync(fdIdList.ToArray());
                foreach (var fd in fdList)
                {
                    TermDeposit_Trn tdTrn = new();
                    tdTrn = Utility.GetModalObject.GetTermDepositTrn(0, toDate, fd.FDId, 0, 0, 0, fd.FDIntCalculatedNow, fd.FDIntCalculatedDateNow, 0, 0, 0, null, 0, 0, 0, null, null, false, false, 0, createdBy, yrId, 0, 0, brCode);
                    fdInterestCalculatedList.Add(tdTrn);
                }
                result = await _unitOfWork.TermDepositTrn.AddTermDepositTrnListAsync(fdInterestCalculatedList);
                _unitOfWork.CommitTransaction();
                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _unitOfWork.RollBack();
                result = false;
            }
            return result;
        }

        public async Task<bool> CanBeginDay(string brCode)
        {
            return await _unitOfWork.Calendars.CanBeginDay(brCode);
        }

        public async Task<bool> DayBeginProcess( string brCode)
        {
            return await _unitOfWork.Calendars.DayBeginProcess(brCode );
        }

        public async Task<List<decimal>> GetFixedDepositIdForInterestCalculation(string tdSchemeType, DateTime toDate, string brCode)
        {
            return await _unitOfWork.Calendars.GetFixedDepositIdForInterestCalculation(tdSchemeType, toDate, brCode);
        }

       
    }
}
