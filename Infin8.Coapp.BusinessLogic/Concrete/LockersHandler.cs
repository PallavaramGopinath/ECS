using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockersHandler : ILockersHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockersHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Lockers>> AddLocker(Lockers locker)
        {
            return await _unitOfWork.Lockers.AddLocker(locker);
        }

        public async Task<List<Lockers>> EditLocker(Lockers locker)
        {
            return await _unitOfWork.Lockers.EditLocker(locker);
        }

        public async Task<List<LockerLookupDto>> GetAvailableLockerList(string brCode)
        {
            return await _unitOfWork.Lockers.GetAvailableLockerList(brCode);
        }

        public async Task<bool> UpdateLockerStatusAsAllotted(decimal lockerId, string brCode)
        {
            return await _unitOfWork.Lockers.UpdateLockerStatusAsAllotted(lockerId, brCode);
        }
    }
}
