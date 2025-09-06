using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayInitHandler
    {
        Task<Pay_Init> AddPayInitAsync(Pay_Init payInit);
        Task<bool> EditPayInitAsync(Pay_Init payInit);
    }
}
