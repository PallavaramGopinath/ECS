using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface ILockerSizeMasterRepository
    {
        Task<List<Locker_Size_Master>> AddLockerSizeMaster(Locker_Size_Master lockerSizeMaster);
        Task<List<Locker_Size_Master>> EditLockerSizeMaster(Locker_Size_Master lockerSizeMaster);
        Task<List<Locker_Size_Master>> GetLockerSizeMasterListAsync(string brCode);
    }
}
