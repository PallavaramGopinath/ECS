using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMemPassbookRepository
    {
        Task<bool> AddMemPassbookAsync(Mem_PassBook memPassbook);
        //Task<bool> EditMemPassbookAsync(Mem_PassBook memPassbook);
        Task<bool> DeleteMemPassbookAsync(Mem_PassBook memPassbook);

    }
}
