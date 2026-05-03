using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    internal class LockerRentTemplateRepository : Repository<Locker_Rent_Template>, ILockerRentTemplateRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerRentTemplateRepository(DbContext context) : base(context)
        {
        }
        public Task<List<Locker_Rent_Template>> AddLockerRentTemplate(Locker_Rent_Template lockerRentTemplate)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Rent_Template>> EditLockerRentTemplate(Locker_Rent_Template lockerRentTemplate)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Rent_Template>> GetLockerRentTemplateListAsync(string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
