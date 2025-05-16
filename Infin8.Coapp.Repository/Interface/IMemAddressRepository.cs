using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMemAddressRepository
    {
        Task<bool> AddMemAddressAsync(Mem_Address memAddress);
        Task<bool> EditMemAddressAsync(Mem_Address memAddress);
    }
}
