using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IMapBanksRepository
    {
        Task<bool> AddMapBanksAsync(Map_Banks mapBanks);
        Task<bool> DeleteMapBanksAsync(Map_Banks mapBanks);
    }
}
