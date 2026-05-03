using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockerRentTemplateHandler : ILockerRentTemplateHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerRentTemplateHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Locker_Rent_Template>> AddLockerRentTemplate(Locker_Rent_Template lockerRentTemplate)
        {
            return await _unitOfWork.LockerRentTemplate.AddLockerRentTemplate(lockerRentTemplate);
        }

        public async Task<List<Locker_Rent_Template>> EditLockerRentTemplate(Locker_Rent_Template lockerRentTemplate)
        {
            return await _unitOfWork.LockerRentTemplate.EditLockerRentTemplate(lockerRentTemplate);
        }

        public async Task<List<Locker_Rent_Template>> GetLockerRentTemplateListAsync(string brCode)
        {
            return await _unitOfWork.LockerRentTemplate.GetLockerRentTemplateListAsync(brCode);
        }
    }
}
