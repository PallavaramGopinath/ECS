using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    internal class LockerRentAdjustmentsRepository : Repository<Locker_Rent_Adjustments>, ILockerRentAdjustmentsRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerRentAdjustmentsRepository(DbContext context) : base(context)
        {
        }
        public async Task<bool> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Locker_Rent_Adjustments 
                .MaxAsync(x => (decimal?)x.Id) ?? 0;
                if (maxId == 0)
                {
                    decimal.TryParse(lockerRentAdjustment.BrCode + "0000000", out maxId);
                }
                maxId++;
                lockerRentAdjustment.Id = maxId;
                await AddAsync(lockerRentAdjustment);
                result = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message + " Something went wrong! An error occurred while adding locker allotment");
            }
            return result;
        }

        public Task<List<Locker_Rent_Adjustments>> EditLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Rent_Adjustments>> GetLockerRentAdjustmentsListAsync(string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
