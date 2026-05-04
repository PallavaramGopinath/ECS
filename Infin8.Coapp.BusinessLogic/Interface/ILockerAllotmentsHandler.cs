using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface ILockerAllotmentsHandler
    {
        Task<(bool result, decimal allotmentId)> AddLockerAllotment(LockerAllotmentVM lockerAllotment);
        Task<List<Locker_Allotments>> EditLockerAllotment(Locker_Allotments lockerAllotment);
        Task<List<LockerAllotmentVM>> GetLockerAllotmentList(string brCode);
        Task<LockerAllotmentVM> GetLockerAllotmentByAllotmentId(decimal allotmentId, string brCode);
    }
}
