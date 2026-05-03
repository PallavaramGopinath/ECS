using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface ILockerRentTemplateHandler
    {
        Task<List<Locker_Rent_Template>> AddLockerRentTemplate(Locker_Rent_Template lockerRentTemplate);
        Task<List<Locker_Rent_Template>> EditLockerRentTemplate(Locker_Rent_Template lockerRentTemplate);
        Task<List<Locker_Rent_Template>> GetLockerRentTemplateListAsync(string brCode);
    }
}
