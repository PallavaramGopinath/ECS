using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockerRentAdjustmentsHandler : ILockerRentAdjustmentsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerRentAdjustmentsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> AddLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            return await _unitOfWork.LockerRentAdjustments.AddLockerRentAdjustment(lockerRentAdjustment);
        }

        public async Task<List<Locker_Rent_Adjustments>> EditLockerRentAdjustment(Locker_Rent_Adjustments lockerRentAdjustment)
        {
            return await _unitOfWork.LockerRentAdjustments.EditLockerRentAdjustment(lockerRentAdjustment);
        }

        public async Task<List<Locker_Rent_Adjustments>> GetLockerRentAdjustmentsListAsync(string brCode)
        {
           return await _unitOfWork.LockerRentAdjustments.GetLockerRentAdjustmentsListAsync(brCode);
        }
    }
}
