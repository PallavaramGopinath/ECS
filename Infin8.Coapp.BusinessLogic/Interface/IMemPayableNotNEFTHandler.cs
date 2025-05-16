using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.BusinessLogic
{
    public interface IMemPayableNotNEFTHandler
    {
        Task<bool> AddMemPayableNotNEFTAsync(Mem_Payable_NotNEFT memPayableNotNEFT);
        Task<bool> DeleteMemPayableNotNEFTAsync(Mem_Payable_NotNEFT memPayableNotNEFT);
    }
}
