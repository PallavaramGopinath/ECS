using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ITermDepositIntCalcCalendarHandler
    {
        Task<bool> AddTermDepositIntCalcCalendarAsync(TermDeposit_IntCalc_Calendar termDepositIntCalcCalendar);
        Task<bool> EditTermDepositIntCalcCalendarAsync(TermDeposit_IntCalc_Calendar termDepositIntCalcCalendar);
        Task<bool> IsInterestOnFDAlreadyCalculatedAsync(DateTime dt);
    }
}
