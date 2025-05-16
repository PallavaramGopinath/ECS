using Infin8.Coapp.Dto;
using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IReferenceConstituencyHandler
    {
        Task<bool> AddConstituency(Refer_Constituency refer_Constituency);
        Task<bool> EditConstituency(Refer_Constituency refer_Constituency);
        Task<List<DropdownItem>> GetConstituencyItems();
    }
}
