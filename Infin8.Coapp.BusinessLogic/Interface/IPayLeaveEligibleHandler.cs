using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayLeaveEligibleHandler
    {
        Task<bool> AddPayLeaveEligibleAsync(Pay_Leave_Eligible payLeaveEligible);
        Task<bool> EditPayLeaveEligibleAsync(Pay_Leave_Eligible payLeaveEligible);
    }
}
