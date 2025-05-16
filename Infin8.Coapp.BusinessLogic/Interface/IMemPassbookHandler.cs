using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IMemPassbookHandler
    {
        Task<bool> AddMemPassbookAsync(Mem_PassBook memPassbook);
        //Task<bool> EditMemPassbookAsync(Mem_PassBook memPassbook);
        Task<bool> DeleteMemPassbookAsync(Mem_PassBook memPassbook);
    }
}
