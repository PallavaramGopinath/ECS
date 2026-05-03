using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LockersRepository : Repository<Lockers>, ILockersRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockersRepository(DbContext context) : base(context)
        {
        }
        public Task<List<Lockers>> AddLocker(Lockers locker)
        {
            throw new NotImplementedException();
        }

        public Task<List<Lockers>> EditLocker(Lockers locker)
        {
            throw new NotImplementedException();
        }

        public async Task<List<LockerLookupDto>> GetAvailableLockerList(string brCode)
        {
            List<LockerLookupDto> lockers = new();
            try
            {
                var query = await (from l in CSISContext.Lockers
                                   join s in CSISContext.Locker_Size_Master on l.Size_Id equals s.Id
                                   where l.BrCode == brCode
                           && l.Status == "Vacant"
                                   select new LockerLookupDto
                                   {
                                       Id = l.Id,
                                       LockerNumber = l.Locker_Number!,
                                       Size = s.Size_Name!,
                                       DepositAmount = s.Deposit_Amount,
                                       InterestRate = s.Interest_Rate,
                                       RentAmount = s.Rent_Amount,
                                       Status = l.Status!
                                   }).ToListAsync();
                if (query != null && query.Count > 0)
                {
                    lockers = query.ToList();
                }
            }
            catch (Exception)
            {
                lockers = new();
            }
            return lockers;
        }

        public async Task<bool> UpdateLockerStatusAsAllotted(decimal lockerId, string brCode)
        {
            bool isUpdated = false;
            try
            {
                var locker = await CSISContext.Lockers.Where(x => x.Id == lockerId && x.BrCode == brCode).FirstOrDefaultAsync();
                if (locker != null)
                {
                    locker.Status = "Allotted";
                    CSISContext.Lockers.Update(locker);
                    await CSISContext.SaveChangesAsync();
                    isUpdated = true;
                }
            }
            catch (Exception)
            {
                isUpdated = false;
            }
            return isUpdated;
        }
    }
}
