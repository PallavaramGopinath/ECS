using Infin8.Coapp.Models;
//using Infin8.Coapp.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LockerSizeMasterRepository : Repository<Locker_Size_Master>, ILockerSizeMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerSizeMasterRepository(DbContext context) : base(context)
        {
        }
        public Task<List<Locker_Size_Master>> AddLockerSizeMaster(Locker_Size_Master lockerSizeMaster)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Size_Master>> EditLockerSizeMaster(Locker_Size_Master lockerSizeMaster)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Size_Master>> GetLockerSizeMasterListAsync(string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
