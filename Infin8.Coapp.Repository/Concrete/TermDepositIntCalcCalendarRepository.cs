using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class TermDepositIntCalcCalendarRepository : Repository<TermDeposit_IntCalc_Calendar>, ITermDepositIntCalcCalendarRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public TermDepositIntCalcCalendarRepository(DbContext context) : base(context)
        {
        }

        public async Task<bool> AddTermDepositIntCalcCalendarAsync(TermDeposit_IntCalc_Calendar termDepositIntCalcCalendar)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.TermDeposit_IntCalc_Calender.MaxAsync(x => x.TDCalc_Id);
                maxId++;
                termDepositIntCalcCalendar.TDCalc_Id = maxId;
                await AddAsync(termDepositIntCalcCalendar);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit interest calculation calendar not saved");
            }
            return result;
        }

        public async Task<bool> EditTermDepositIntCalcCalendarAsync(TermDeposit_IntCalc_Calendar termDepositIntCalcCalendar)
        {
            bool result = false;
            try
            {
                await EditAsync(termDepositIntCalcCalendar);
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! Term deposit interest calculation calendar not deleted");
            }
            return result;
        }

        public async Task<bool> IsInterestOnFDAlreadyCalculatedAsync(DateTime dt)
        {
            bool result = false;
            try
            {
                int count = await CSISContext.TermDeposit_IntCalc_Calender
                .Where(x => x.TDCalc_Date == dt.Date && x.TDCalc_Delete == false)
                .CountAsync();

                if (count > 0) result = true;
                else result = false;
            }
            catch (Exception ex)
            {
                result = false;
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while checking if fixed deposit interest was calculated on the same date");
            }
            return result;
        }
    }
}
