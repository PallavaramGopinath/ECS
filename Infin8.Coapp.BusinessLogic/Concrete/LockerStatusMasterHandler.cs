using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockerStatusMasterHandler : ILockerStatusMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerStatusMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Locker_Status_Master>> AddLockerStatusMaster(Locker_Status_Master lockerStatusMaster)
        {
            return await _unitOfWork.LockerStatusMaster.AddLockerStatusMaster(lockerStatusMaster);
        }

        public async Task<List<Locker_Status_Master>> EditLockerStatusMaster(Locker_Status_Master lockerStatusMaster)
        {
            return await _unitOfWork.LockerStatusMaster.EditLockerStatusMaster(lockerStatusMaster);
        }

        public async Task<List<Locker_Status_Master>> GetLockerStatusMasterListAsync(string brCode)
        {
            return await _unitOfWork.LockerStatusMaster.GetLockerStatusMasterListAsync(brCode);
        }
    }
}
