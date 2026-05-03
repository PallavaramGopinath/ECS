using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface ILockersHandler
    {
        Task<List<Lockers>> AddLocker(Lockers locker);
        Task<List<Lockers>> EditLocker(Lockers locker);
        Task<List<LockerLookupDto>> GetAvailableLockerList(string brCode);
        Task<bool> UpdateLockerStatusAsAllotted(decimal lockerId, string brCode);
    }
}
