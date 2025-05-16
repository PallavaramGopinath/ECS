using Infin8.Coapp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infin8.Coapp.Repository
{
    public interface IPaySlipTrnRepository
    {
        Task<bool> AddPaySlipTrnAsync(Pay_Slip_Trn paySlipTrn);
        Task<bool> EditPaySlipTrnAsync(Pay_Slip_Trn paySlipTrn);
    }
}
