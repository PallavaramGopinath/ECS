using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface ILockerSettingsHandler
    {
        Task<List<Locker_Settings>> AddLockerSettings(Locker_Settings lockerSettings);
        Task<List<Locker_Settings>> EditLockerSettings(Locker_Settings lockerSettings);
        Task<Locker_Settings> GetLockerSetting(string brCode);
    }
}
