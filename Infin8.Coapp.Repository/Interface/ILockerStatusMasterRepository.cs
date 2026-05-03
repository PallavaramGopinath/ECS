using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ILockerStatusMasterRepository
    {
        Task<List<Locker_Status_Master>> GetLockerStatusMasterListAsync(string brCode);
        Task<List<Locker_Status_Master>> AddLockerStatusMaster(Locker_Status_Master lockerStatusMaster);
        Task<List<Locker_Status_Master>> EditLockerStatusMaster(Locker_Status_Master lockerStatusMaster);
    }
}
