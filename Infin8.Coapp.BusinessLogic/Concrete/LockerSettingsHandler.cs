using Infin8.Coapp.Models;
using Infin8.Coapp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  class LockerSettingsHandler : ILockerSettingsHandler
    {
        readonly IUnitOfWork _unitOfWork;
        public LockerSettingsHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Locker_Settings>> AddLockerSettings(Locker_Settings lockerSettings)
        {
            return await _unitOfWork.LockerSettings.AddLockerSettings(lockerSettings);
        }

        public async Task<List<Locker_Settings>> EditLockerSettings(Locker_Settings lockerSettings)
        {
           return await _unitOfWork.LockerSettings.EditLockerSettings(lockerSettings);
        }

        public async Task<Locker_Settings> GetLockerSetting(string brCode)
        {
            return await _unitOfWork.LockerSettings.GetLockerSetting(brCode);
        }
    }
}
