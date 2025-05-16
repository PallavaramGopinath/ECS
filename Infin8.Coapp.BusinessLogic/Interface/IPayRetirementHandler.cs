using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayRetirementHandler
    {
        Task<bool> AddPayRetirementAsync(Pay_Retirement payRetirement);
        Task<bool> EditPayRetirementAsync(Pay_Retirement payRetirement);
    }
}
