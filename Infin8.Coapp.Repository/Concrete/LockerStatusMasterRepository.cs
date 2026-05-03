using Infin8.Coapp.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public class LockerStatusMasterRepository : Repository<Locker_Status_Master>, ILockerStatusMasterRepository
    {
        public CSISContext CSISContext => (CSISContext)Context;
        public LockerStatusMasterRepository(DbContext context) : base(context)
        {
        }
        public Task<List<Locker_Status_Master>> AddLockerStatusMaster(Locker_Status_Master lockerStatusMaster)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Status_Master>> EditLockerStatusMaster(Locker_Status_Master lockerStatusMaster)
        {
            throw new NotImplementedException();
        }

        public Task<List<Locker_Status_Master>> GetLockerStatusMasterListAsync(string brCode)
        {
            throw new NotImplementedException();
        }
    }
}
