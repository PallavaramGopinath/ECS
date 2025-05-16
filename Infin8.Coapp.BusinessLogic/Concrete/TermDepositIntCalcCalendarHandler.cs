using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public class TermDepositIntCalcCalendarHandler : ITermDepositIntCalcCalendarHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public TermDepositIntCalcCalendarHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddTermDepositIntCalcCalendarAsync(TermDeposit_IntCalc_Calendar termDepositIntCalcCalendar)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositIntCalcCalendar.AddTermDepositIntCalcCalendarAsync(termDepositIntCalcCalendar);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Interest calculation calendar for term deposit not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositIntCalcCalendarAsync(TermDeposit_IntCalc_Calendar termDepositIntCalcCalendar)
        {
            bool result = false;
            try
            {
                result = await _unitOfWork.TermDepositIntCalcCalendar.EditTermDepositIntCalcCalendarAsync(termDepositIntCalcCalendar);
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Interest calculation calendar for term deposit not deleted");
            }
            return result;
        }

        public async Task<bool> IsInterestOnFDAlreadyCalculatedAsync(DateTime dt)
        {
            return await _unitOfWork.TermDepositIntCalcCalendar.IsInterestOnFDAlreadyCalculatedAsync(dt);
        }
    }
}
