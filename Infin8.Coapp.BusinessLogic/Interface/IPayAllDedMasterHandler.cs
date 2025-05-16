using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IPayAllDedMasterHandler
    {
        Task<bool> AddAllDedMasterAsync(Pay_All_Ded_Master payAllDedMaster);
        Task<bool> EditAllDedMasterAsync(Pay_All_Ded_Master payAllDedMaster);
    }
}
