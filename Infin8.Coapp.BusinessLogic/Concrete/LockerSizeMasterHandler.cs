using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockerSizeMasterHandler : ILockerSizeMasterHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerSizeMasterHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Locker_Size_Master>> AddLockerSizeMaster(Locker_Size_Master lockerSizeMaster)
        {
            return await _unitOfWork.LockerSizeMaster.AddLockerSizeMaster(lockerSizeMaster);
        }

        public async Task<List<Locker_Size_Master>> EditLockerSizeMaster(Locker_Size_Master lockerSizeMaster)
        {
            return await _unitOfWork.LockerSizeMaster.EditLockerSizeMaster(lockerSizeMaster);
        }

        public async Task<List<Locker_Size_Master>> GetLockerSizeMasterListAsync(string brCode)
        {
            return await _unitOfWork.LockerSizeMaster.GetLockerSizeMasterListAsync(brCode);
        }
    }
}
