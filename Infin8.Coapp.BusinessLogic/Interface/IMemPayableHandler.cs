using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public  interface IMemPayableHandler
    {
        Task<bool> AddMemPayableAsync(Mem_Payable memPayable);
        Task<bool> EditMemPayableAsync(Mem_Payable memPayable);
    }
}
