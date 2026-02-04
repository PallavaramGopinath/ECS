using Infin8.Coapp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public  interface IMenuMainRepository
    {
        Task<List<MenuMain>> GetMenuStructureAsync(string brCode,string role);
    }
}
