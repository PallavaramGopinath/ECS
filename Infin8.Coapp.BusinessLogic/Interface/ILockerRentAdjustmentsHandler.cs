using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface ILockerRentAdjustmentsHandler
    {
        Task<bool> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment);
        Task<List<Locker_Rent_Adjustments>> EditLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment);
        Task<List<Locker_Rent_Adjustments>> GetLockerRentAdjustmentsListAsync(string brCode);
        Task<List<LockerRentReceiptAllotmentWiseDto>> GetLockerRentAllotmentWiseListAsync(decimal customerId, string brCode);
        Task<List<LockerClosureBalanceDto>> GetLockerClosureBalanceListAsync(decimal customerId, string brCode);
    }
}
