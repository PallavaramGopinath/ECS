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
    internal class LockerClosuresRepository : Repository<Locker_Closures>, ILockerClosuresRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerClosuresRepository(DbContext context) : base(context)
        {
        }
        public Task<List<Locker_Closures>> AddLockerClosure(Locker_Closures lockerClosure)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Closures>> EditLockerClosure(Locker_Closures lockerClosure)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Closures>> GetLockerClosuresListAsync(string brCode)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CloseLockerAllotment(Locker_Closures lockerClosure)
        {
            bool result = false;
            try
            {
                decimal maxId = await CSISContext.Locker_Closures
               .MaxAsync(x => (decimal?)x.Id) ?? 0;
                if (maxId == 0)
                {
                    decimal.TryParse(lockerClosure.BrCode + "0000000", out maxId);
                }
                maxId++;
                lockerClosure.Id = maxId;
                await AddAsync(lockerClosure);
                CSISContext.SaveChanges();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

    }
}