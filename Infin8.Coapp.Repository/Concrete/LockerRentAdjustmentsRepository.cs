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
        public Task<List<Locker_Rent_Adjustments>> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            throw new NotImplementedException();
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
