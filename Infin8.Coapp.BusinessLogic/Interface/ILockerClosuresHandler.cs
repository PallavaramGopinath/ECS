using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface ILockerClosuresHandler
    {
        Task<List<Locker_Closures>> AddLockerClosure(Locker_Closures lockerClosure);
        Task<List<Locker_Closures>> EditLockerClosure(Locker_Closures lockerClosure);
        Task<List<Locker_Closures>> GetLockerClosuresListAsync(string brCode);
        Task<List<LockerAllotmentVM>> CloseLockerAllotment(Locker_Closures lockerClosure);
    }
}
