using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPayAllDedRepository
    {
        Task<bool> AddPayAllDedAsync(Pay_All_Ded payAllDed);
        Task<bool> EditPayAllDedAsync(Pay_All_Ded payAllDed);
    }
}
