using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IJLOrnmentHandler
    {
        Task<bool> AddJLOrnmentListAsync(List<JL_Ornments> jLOrnmentList);
        Task<bool> EditJLOrnmentListAsync(List<JL_Ornments> jLOrnmentList);
    }
}
