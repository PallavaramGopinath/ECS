using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayGenInfoHandler 
    {
        Task<bool> AddPayGenInfoAsync(Pay_Gen_Info payGenInfo);
        Task<bool> EditPayGenInfoAsync(Pay_Gen_Info payGenInfo);
    }
}
